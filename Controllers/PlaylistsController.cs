using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using EcommerceApp.Data;
using EcommerceApp.Models;
using EcommerceApp.Services;

namespace EcommerceApp.Controllers
{
    // Playlists públicas creadas por el admin.
    [Authorize(Roles = "Admin")]
    public class PlaylistsController : Controller
    {
        private readonly ApplicationDbContext _db;

        public PlaylistsController(ApplicationDbContext db)
        {
            _db = db;
        }

        // GET: Playlists
        public async Task<IActionResult> Index()
        {
            var lista = await _db.Playlists
                .Where(p => p.EsPublica)
                .Include(p => p.Canciones)
                .OrderByDescending(p => p.CreadoEn)
                .ToListAsync();
            ViewData["ActiveMenu"] = "Playlists";
            return View(lista);
        }

        // GET: Playlists/Create
        public async Task<IActionResult> Create()
        {
            var vm = new PlaylistViewModel { Disponibles = await PlaylistHelpers.DisponiblesAsync(_db) };
            ViewData["ActiveMenu"] = "Playlists";
            return View(vm);
        }

        // POST: Playlists/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(PlaylistViewModel vm)
        {
            var ids = await PlaylistHelpers.ValidarAsync(_db, vm, ModelState);
            if (!ModelState.IsValid)
            {
                vm.Disponibles = await PlaylistHelpers.DisponiblesAsync(_db);
                ViewData["ActiveMenu"] = "Playlists";
                return View(vm);
            }

            var playlist = new Playlist { Nombre = vm.Nombre, EsPublica = true };
            _db.Playlists.Add(playlist);
            await _db.SaveChangesAsync();

            PlaylistHelpers.GuardarCola(_db, playlist.Id, ids);
            await _db.SaveChangesAsync();

            TempData["Success"] = "Playlist pública creada.";
            return RedirectToAction(nameof(Index));
        }

        // GET: Playlists/Edit/5
        public async Task<IActionResult> Edit(int id)
        {
            var playlist = await _db.Playlists.Include(p => p.Canciones).FirstOrDefaultAsync(p => p.Id == id && p.EsPublica);
            if (playlist == null) return NotFound();

            var vm = new PlaylistViewModel
            {
                Id = playlist.Id,
                Nombre = playlist.Nombre,
                MusicaIds = playlist.Canciones.OrderBy(c => c.Orden).Select(c => c.MusicaId).ToList(),
                Disponibles = await PlaylistHelpers.DisponiblesAsync(_db)
            };
            ViewData["ActiveMenu"] = "Playlists";
            return View(vm);
        }

        // POST: Playlists/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, PlaylistViewModel vm)
        {
            var playlist = await _db.Playlists.Include(p => p.Canciones).FirstOrDefaultAsync(p => p.Id == id && p.EsPublica);
            if (playlist == null) return NotFound();

            var ids = await PlaylistHelpers.ValidarAsync(_db, vm, ModelState);
            if (!ModelState.IsValid)
            {
                vm.Id = id;
                vm.Disponibles = await PlaylistHelpers.DisponiblesAsync(_db);
                ViewData["ActiveMenu"] = "Playlists";
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

        // POST: Playlists/Delete/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(int id)
        {
            var playlist = await _db.Playlists.FirstOrDefaultAsync(p => p.Id == id && p.EsPublica);
            if (playlist == null) return NotFound();

            _db.Playlists.Remove(playlist);
            await _db.SaveChangesAsync();
            TempData["Success"] = "Playlist eliminada.";
            return RedirectToAction(nameof(Index));
        }

        // GET: Playlists/Details/5 — vista en árbol (playlist > género > canciones)
        public async Task<IActionResult> Details(int id)
        {
            var playlist = await PlaylistHelpers.CargarConCancionesAsync(_db, id);
            if (playlist == null || !playlist.EsPublica) return NotFound();

            ViewData["Arbol"] = NodoArbol.PorGenero(playlist.Nombre,
                playlist.Canciones.OrderBy(c => c.Orden).Select(c => c.Musica));
            ViewData["ActiveMenu"] = "Playlists";
            return View(playlist);
        }
    }
}
