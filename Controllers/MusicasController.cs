using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using EcommerceApp.Data;
using EcommerceApp.Models;
using EcommerceApp.Services;
using CloudinaryDotNet.Actions;

namespace EcommerceApp.Controllers
{
    [Authorize(Roles = "Admin")]
    public class MusicasController : Controller
    {
        private readonly ApplicationDbContext _db;
        private readonly CloudinaryService _cloudinary;

        public MusicasController(ApplicationDbContext db, CloudinaryService cloudinary)
        {
            _db = db;
            _cloudinary = cloudinary;
        }

        // GET: Musicas
        public async Task<IActionResult> Index()
        {
            var musicas = await _db.Musicas
                .Include(m => m.MusicaGeneros).ThenInclude(mg => mg.Genero)
                .Include(m => m.MusicaArtistas).ThenInclude(ma => ma.Artista)
                .OrderByDescending(m => m.CreadoEn)
                .ToListAsync();
            return View(musicas);
        }

        // GET: Musicas/Create
        public async Task<IActionResult> Create()
        {
            var vm = new MusicaViewModel
            {
                GenerosDisponibles = await GetGenerosSelectList(),
                ArtistasDisponibles = await GetArtistasSelectList()
            };
            return View(vm);
        }

        // POST: Musicas/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        [RequestSizeLimit(52428800)] // 50 MB
        public async Task<IActionResult> Create(MusicaViewModel vm)
        {
            if (!ModelState.IsValid)
            {
                vm.GenerosDisponibles = await GetGenerosSelectList();
                vm.ArtistasDisponibles = await GetArtistasSelectList();
                return View(vm);
            }

            if (vm.ArchivoAudio == null)
            {
                ModelState.AddModelError("ArchivoAudio", "El archivo de audio es obligatorio.");
                vm.GenerosDisponibles = await GetGenerosSelectList();
                vm.ArtistasDisponibles = await GetArtistasSelectList();
                return View(vm);
            }

            try
            {
                var musica = new Musica { Nombre = vm.Nombre };

                var (audioUrl, audioPid) = await _cloudinary.SubirAudioAsync(vm.ArchivoAudio);
                musica.AudioUrl = audioUrl;
                musica.AudioPublicId = audioPid;

                if (vm.ArchivoImagen != null)
                {
                    var (imgUrl, imgPid) = await _cloudinary.SubirImagenAsync(vm.ArchivoImagen);
                    musica.ImagenUrl = imgUrl;
                    musica.ImagenPublicId = imgPid;
                }

                _db.Musicas.Add(musica);
                await _db.SaveChangesAsync();

                // Géneros
                foreach (var gid in vm.GenerosSeleccionados)
                    _db.Set<MusicaGenero>().Add(new MusicaGenero { MusicaId = musica.Id, GeneroId = gid });

                // Artistas
                foreach (var aid in vm.ArtistasSeleccionados)
                    _db.Set<MusicaArtista>().Add(new MusicaArtista { MusicaId = musica.Id, ArtistaId = aid });

                await _db.SaveChangesAsync();
                TempData["Success"] = "Canción creada exitosamente.";
                return RedirectToAction(nameof(Index));
            }
            catch (Exception ex)
            {
                ModelState.AddModelError("", $"Error al subir archivos: {ex.Message}");
                vm.GenerosDisponibles = await GetGenerosSelectList();
                vm.ArtistasDisponibles = await GetArtistasSelectList();
                return View(vm);
            }
        }

        // GET: Musicas/Edit/5
        public async Task<IActionResult> Edit(int id)
        {
            var musica = await _db.Musicas
                .Include(m => m.MusicaGeneros)
                .Include(m => m.MusicaArtistas)
                .FirstOrDefaultAsync(m => m.Id == id);

            if (musica == null) return NotFound();

            var vm = new MusicaViewModel
            {
                Id = musica.Id,
                Nombre = musica.Nombre,
                AudioUrlActual = musica.AudioUrl,
                ImagenUrlActual = musica.ImagenUrl,
                GenerosSeleccionados = musica.MusicaGeneros.Select(mg => mg.GeneroId).ToList(),
                ArtistasSeleccionados = musica.MusicaArtistas.Select(ma => ma.ArtistaId).ToList(),
                GenerosDisponibles = await GetGenerosSelectList(),
                ArtistasDisponibles = await GetArtistasSelectList()
            };
            return View(vm);
        }

        // POST: Musicas/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        [RequestSizeLimit(52428800)]
        public async Task<IActionResult> Edit(int id, MusicaViewModel vm)
        {
            if (!ModelState.IsValid)
            {
                vm.GenerosDisponibles = await GetGenerosSelectList();
                vm.ArtistasDisponibles = await GetArtistasSelectList();
                return View(vm);
            }

            var musica = await _db.Musicas
                .Include(m => m.MusicaGeneros)
                .Include(m => m.MusicaArtistas)
                .FirstOrDefaultAsync(m => m.Id == id);

            if (musica == null) return NotFound();

            try
            {
                musica.Nombre = vm.Nombre;

                if (vm.ArchivoAudio != null)
                {
                    if (!string.IsNullOrEmpty(musica.AudioPublicId))
                        await _cloudinary.EliminarRecursoAsync(musica.AudioPublicId, ResourceType.Raw);

                    var (audioUrl, audioPid) = await _cloudinary.SubirAudioAsync(vm.ArchivoAudio);
                    musica.AudioUrl = audioUrl;
                    musica.AudioPublicId = audioPid;
                }

                if (vm.ArchivoImagen != null)
                {
                    if (!string.IsNullOrEmpty(musica.ImagenPublicId))
                        await _cloudinary.EliminarRecursoAsync(musica.ImagenPublicId, ResourceType.Image);

                    var (imgUrl, imgPid) = await _cloudinary.SubirImagenAsync(vm.ArchivoImagen);
                    musica.ImagenUrl = imgUrl;
                    musica.ImagenPublicId = imgPid;
                }

                // Update géneros
                _db.Set<MusicaGenero>().RemoveRange(musica.MusicaGeneros);
                foreach (var gid in vm.GenerosSeleccionados)
                    _db.Set<MusicaGenero>().Add(new MusicaGenero { MusicaId = musica.Id, GeneroId = gid });

                // Update artistas
                _db.Set<MusicaArtista>().RemoveRange(musica.MusicaArtistas);
                foreach (var aid in vm.ArtistasSeleccionados)
                    _db.Set<MusicaArtista>().Add(new MusicaArtista { MusicaId = musica.Id, ArtistaId = aid });

                await _db.SaveChangesAsync();
                TempData["Success"] = "Canción actualizada exitosamente.";
                return RedirectToAction(nameof(Index));
            }
            catch (Exception ex)
            {
                ModelState.AddModelError("", $"Error: {ex.Message}");
                vm.GenerosDisponibles = await GetGenerosSelectList();
                vm.ArtistasDisponibles = await GetArtistasSelectList();
                return View(vm);
            }
        }

        // POST: Musicas/Delete/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(int id)
        {
            var musica = await _db.Musicas.FindAsync(id);
            if (musica == null) return NotFound();

            try
            {
                if (!string.IsNullOrEmpty(musica.AudioPublicId))
                    await _cloudinary.EliminarRecursoAsync(musica.AudioPublicId, ResourceType.Raw);
                if (!string.IsNullOrEmpty(musica.ImagenPublicId))
                    await _cloudinary.EliminarRecursoAsync(musica.ImagenPublicId, ResourceType.Image);

                _db.Musicas.Remove(musica);
                await _db.SaveChangesAsync();
                TempData["Success"] = "Canción eliminada correctamente.";
            }
            catch (Exception ex)
            {
                TempData["Error"] = $"Error al eliminar: {ex.Message}";
            }
            return RedirectToAction(nameof(Index));
        }

        // ── Helpers ──
        private async Task<List<SelectListItem>> GetGenerosSelectList() =>
            await _db.Generos.OrderBy(g => g.Nombre)
                .Select(g => new SelectListItem { Value = g.Id.ToString(), Text = g.Nombre })
                .ToListAsync();

        private async Task<List<SelectListItem>> GetArtistasSelectList() =>
            await _db.Artistas.OrderBy(a => a.Nombre)
                .Select(a => new SelectListItem { Value = a.Id.ToString(), Text = a.Nombre })
                .ToListAsync();
    }
}
