using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using EcommerceApp.Data;
using EcommerceApp.Models;
using EcommerceApp.Services;

namespace EcommerceApp.Controllers
{
    // Playlists privadas del cliente (y lectura de las públicas del admin).
    [Authorize]
    public class MisPlaylistsController : Controller
    {
        private readonly ApplicationDbContext _db;

        public MisPlaylistsController(ApplicationDbContext db)
        {
            _db = db;
        }

        private string? UserId => User.FindFirstValue(ClaimTypes.NameIdentifier);

        // GET: MisPlaylists
        public async Task<IActionResult> Index()
        {
            var uid = UserId;
            var propias = await _db.Playlists
                .Where(p => !p.EsPublica && p.CreadoPorId == uid)
                .Include(p => p.Canciones)
                .OrderByDescending(p => p.CreadoEn)
                .ToListAsync();

            ViewData["Publicas"] = await _db.Playlists
                .Where(p => p.EsPublica)
                .Include(p => p.Canciones)
                .OrderBy(p => p.Nombre)
                .ToListAsync();
            ViewData["ActiveMenu"] = "MisPlaylists";
            return View(propias);
        }

        // GET: MisPlaylists/Create
        public async Task<IActionResult> Create()
        {
            var vm = new PlaylistViewModel { Disponibles = await PlaylistHelpers.DisponiblesAsync(_db) };
            ViewData["ActiveMenu"] = "MisPlaylists";
            return View(vm);
        }

        // POST: MisPlaylists/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(PlaylistViewModel vm)
        {
            var uid = UserId;
            if (uid == null) return Challenge();

            var ids = await PlaylistHelpers.ValidarAsync(_db, vm, ModelState);
            if (!ModelState.IsValid)
            {
                vm.Disponibles = await PlaylistHelpers.DisponiblesAsync(_db);
                ViewData["ActiveMenu"] = "MisPlaylists";
                return View(vm);
            }

            var playlist = new Playlist { Nombre = vm.Nombre, EsPublica = false, CreadoPorId = uid };
            _db.Playlists.Add(playlist);
            await _db.SaveChangesAsync();

            PlaylistHelpers.GuardarCola(_db, playlist.Id, ids);
            await _db.SaveChangesAsync();

            TempData["Success"] = "Playlist creada.";
            return RedirectToAction(nameof(Index));
        }

        // GET: MisPlaylists/Edit/5 — solo las propias privadas
        public async Task<IActionResult> Edit(int id)
        {
            var uid = UserId;
            var playlist = await _db.Playlists.Include(p => p.Canciones)
                .FirstOrDefaultAsync(p => p.Id == id && !p.EsPublica && p.CreadoPorId == uid);
            if (playlist == null) return NotFound();

            var vm = new PlaylistViewModel
            {
                Id = playlist.Id,
                Nombre = playlist.Nombre,
                MusicaIds = playlist.Canciones.OrderBy(c => c.Orden).Select(c => c.MusicaId).ToList(),
                Disponibles = await PlaylistHelpers.DisponiblesAsync(_db)
            };
            ViewData["ActiveMenu"] = "MisPlaylists";
            return View(vm);
        }

        // POST: MisPlaylists/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, PlaylistViewModel vm)
        {
            var uid = UserId;
            var playlist = await _db.Playlists.Include(p => p.Canciones)
                .FirstOrDefaultAsync(p => p.Id == id && !p.EsPublica && p.CreadoPorId == uid);
            if (playlist == null) return NotFound();

            var ids = await PlaylistHelpers.ValidarAsync(_db, vm, ModelState);
            if (!ModelState.IsValid)
            {
                vm.Id = id;
                vm.Disponibles = await PlaylistHelpers.DisponiblesAsync(_db);
                ViewData["ActiveMenu"] = "MisPlaylists";
                return View(vm);
            }

            playlist.Nombre = vm.Nombre;
            _db.PlaylistMusicas.RemoveRange(playlist.Canciones);
            await _db.SaveChangesAsync();

            PlaylistHelpers.GuardarCola(_db, playlist.Id, ids);
            await _db.SaveChangesAsync();

            TempData["Success"] = "Playlist actualizada.";
            return RedirectToAction(nameof(Index));
        }

        // POST: MisPlaylists/Delete/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(int id)
        {
            var uid = UserId;
            var playlist = await _db.Playlists.FirstOrDefaultAsync(p => p.Id == id && !p.EsPublica && p.CreadoPorId == uid);
            if (playlist == null) return NotFound();

            _db.Playlists.Remove(playlist);
            await _db.SaveChangesAsync();
            TempData["Success"] = "Playlist eliminada.";
            return RedirectToAction(nameof(Index));
        }

        // GET: MisPlaylists/Mis?musicaId=5 — playlists privadas del usuario (indica si ya tienen la canción)
        [HttpGet]
        public async Task<IActionResult> Mis(int musicaId)
        {
            var uid = UserId;
            if (uid == null) return Unauthorized();

            var lista = await _db.Playlists
                .Where(p => !p.EsPublica && p.CreadoPorId == uid)
                .OrderBy(p => p.Nombre)
                .Select(p => new
                {
                    id = p.Id,
                    nombre = p.Nombre,
                    total = p.Canciones.Count(),
                    contiene = p.Canciones.Any(c => c.MusicaId == musicaId)
                })
                .ToListAsync();
            return Json(lista);
        }

        // POST: MisPlaylists/AgregarCancion — agrega la canción a varias playlists propias
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> AgregarCancion(int musicaId, List<int> playlistIds)
        {
            var uid = UserId;
            if (uid == null) return Unauthorized();
            if (!await _db.Musicas.AnyAsync(m => m.Id == musicaId && m.Estado == "Aprobada")) return NotFound();

            var ids = (playlistIds ?? new List<int>()).Distinct().ToList();
            var playlists = await _db.Playlists
                .Where(p => ids.Contains(p.Id) && !p.EsPublica && p.CreadoPorId == uid)
                .Include(p => p.Canciones)
                .ToListAsync();

            int agregadas = 0;
            foreach (var p in playlists)
            {
                if (p.Canciones.Any(c => c.MusicaId == musicaId)) continue;
                var orden = p.Canciones.Count == 0 ? 1 : p.Canciones.Max(c => c.Orden) + 1;
                _db.PlaylistMusicas.Add(new PlaylistMusica { PlaylistId = p.Id, MusicaId = musicaId, Orden = orden });
                agregadas++;
            }
            await _db.SaveChangesAsync();
            return Json(new { agregadas });
        }

        // POST: MisPlaylists/CrearConCancion — crea una playlist privada que ya contiene la canción
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CrearConCancion(string nombre, int musicaId)
        {
            var uid = UserId;
            if (uid == null) return Unauthorized();

            var limpio = (nombre ?? string.Empty).Trim();
            if (limpio.Length == 0 || limpio.Length > 120)
                return BadRequest("El nombre debe tener entre 1 y 120 caracteres.");
            if (!await _db.Musicas.AnyAsync(m => m.Id == musicaId && m.Estado == "Aprobada")) return NotFound();

            var playlist = new Playlist { Nombre = limpio, EsPublica = false, CreadoPorId = uid };
            _db.Playlists.Add(playlist);
            await _db.SaveChangesAsync();

            _db.PlaylistMusicas.Add(new PlaylistMusica { PlaylistId = playlist.Id, MusicaId = musicaId, Orden = 1 });
            await _db.SaveChangesAsync();

            return Json(new { id = playlist.Id, nombre = playlist.Nombre });
        }

        // GET: MisPlaylists/Details/5 — propias privadas o cualquier pública
        public async Task<IActionResult> Details(int id)
        {
            var uid = UserId;
            var playlist = await PlaylistHelpers.CargarConCancionesAsync(_db, id);
            if (playlist == null || (!playlist.EsPublica && playlist.CreadoPorId != uid)) return NotFound();

            ViewData["Arbol"] = NodoArbol.PorGenero(playlist.Nombre,
                playlist.Canciones.OrderBy(c => c.Orden).Select(c => c.Musica));
            ViewData["EsPropia"] = !playlist.EsPublica && playlist.CreadoPorId == uid;
            ViewData["ActiveMenu"] = "MisPlaylists";
            return View(playlist);
        }
    }
}
