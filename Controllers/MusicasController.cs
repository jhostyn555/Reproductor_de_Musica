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

        // GET: Musicas?q=texto&vista=tarjetas|cuadricula
        // Busca por nombre de la canción, artista o género (solo al enviar el formulario).
        public async Task<IActionResult> Index(string? q, string? vista)
        {
            q = q?.Trim();

            var query = _db.Musicas
                .Include(m => m.MusicaGeneros).ThenInclude(mg => mg.Genero)
                .Include(m => m.MusicaArtistas).ThenInclude(ma => ma.Artista)
                .AsQueryable();

            if (!string.IsNullOrEmpty(q))
            {
                // Escapar comodines de LIKE para que el texto se busque literal.
                var escapado = q.Replace("\\", "\\\\").Replace("%", "\\%").Replace("_", "\\_");
                var patron = $"%{escapado}%";

                // ILike = coincidencia sin distinguir mayúsculas (PostgreSQL)
                query = query.Where(m =>
                    EF.Functions.ILike(m.Nombre, patron) ||
                    m.MusicaArtistas.Any(ma => EF.Functions.ILike(ma.Artista.Nombre, patron)) ||
                    m.MusicaGeneros.Any(mg => EF.Functions.ILike(mg.Genero.Nombre, patron)));
            }

            var musicas = await query
                .OrderByDescending(m => m.CreadoEn)
                .ToListAsync();

            ViewData["Q"] = q ?? string.Empty;
            ViewData["Vista"] = vista == "cuadricula" ? "cuadricula" : "tarjetas";
            ViewData["Total"] = string.IsNullOrEmpty(q)
                ? musicas.Count
                : await _db.Musicas.CountAsync();

            return View(musicas);
        }

        // GET: Musicas/Reproducir/5
        // Página de reproducción a pantalla completa (estilo Spotify).
        public async Task<IActionResult> Reproducir(int id)
        {
            var musica = await _db.Musicas
                .Include(m => m.MusicaGeneros).ThenInclude(mg => mg.Genero)
                .Include(m => m.MusicaArtistas).ThenInclude(ma => ma.Artista)
                .FirstOrDefaultAsync(m => m.Id == id);

            if (musica == null) return NotFound();
            return View(musica);
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
            var urlAudio = vm.UrlAudio?.Trim();
            var urlImagen = vm.UrlImagen?.Trim();

            if (!ModelState.IsValid)
            {
                await RellenarListasAsync(vm);
                return View(vm);
            }

            // El audio es obligatorio: archivo o enlace.
            if (vm.ArchivoAudio == null && string.IsNullOrEmpty(urlAudio))
            {
                ModelState.AddModelError("ArchivoAudio", "Sube un archivo de audio o indica un enlace de audio.");
            }
            if (!string.IsNullOrEmpty(urlAudio) && !EsUrlHttp(urlAudio))
            {
                ModelState.AddModelError("UrlAudio", "El enlace de audio debe ser una URL válida que empiece por http:// o https://");
            }
            if (!string.IsNullOrEmpty(urlImagen) && !EsUrlHttp(urlImagen))
            {
                ModelState.AddModelError("UrlImagen", "El enlace de imagen debe ser una URL válida que empiece por http:// o https://");
            }
            if (!ModelState.IsValid)
            {
                await RellenarListasAsync(vm);
                return View(vm);
            }

            try
            {
                var musica = new Musica { Nombre = vm.Nombre };

                // Si hay archivo, tiene prioridad sobre el enlace.
                var audio = vm.ArchivoAudio != null
                    ? await _cloudinary.SubirAudioAsync(vm.ArchivoAudio)
                    : await _cloudinary.SubirAudioDesdeUrlAsync(urlAudio!);
                musica.AudioUrl = audio.Url;
                musica.AudioPublicId = audio.PublicId;

                if (vm.ArchivoImagen != null)
                {
                    var img = await _cloudinary.SubirImagenAsync(vm.ArchivoImagen);
                    musica.ImagenUrl = img.Url;
                    musica.ImagenPublicId = img.PublicId;
                }
                else if (!string.IsNullOrEmpty(urlImagen))
                {
                    var img = await _cloudinary.SubirImagenDesdeUrlAsync(urlImagen);
                    musica.ImagenUrl = img.Url;
                    musica.ImagenPublicId = img.PublicId;
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
                await RellenarListasAsync(vm);
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
            var urlAudio = vm.UrlAudio?.Trim();
            var urlImagen = vm.UrlImagen?.Trim();

            if (!string.IsNullOrEmpty(urlAudio) && !EsUrlHttp(urlAudio))
                ModelState.AddModelError("UrlAudio", "El enlace de audio debe ser una URL válida que empiece por http:// o https://");
            if (!string.IsNullOrEmpty(urlImagen) && !EsUrlHttp(urlImagen))
                ModelState.AddModelError("UrlImagen", "El enlace de imagen debe ser una URL válida que empiece por http:// o https://");

            if (!ModelState.IsValid)
            {
                await RellenarListasAsync(vm);
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

                // Subir primero el nuevo recurso y borrar el anterior después,
                // así un fallo en la subida no deja la canción sin audio.
                if (vm.ArchivoAudio != null || !string.IsNullOrEmpty(urlAudio))
                {
                    var audio = vm.ArchivoAudio != null
                        ? await _cloudinary.SubirAudioAsync(vm.ArchivoAudio)
                        : await _cloudinary.SubirAudioDesdeUrlAsync(urlAudio!);

                    var anteriorAudio = musica.AudioPublicId;
                    musica.AudioUrl = audio.Url;
                    musica.AudioPublicId = audio.PublicId;

                    if (!string.IsNullOrEmpty(anteriorAudio))
                        await _cloudinary.EliminarRecursoAsync(anteriorAudio, ResourceType.Raw);
                }

                if (vm.ArchivoImagen != null || !string.IsNullOrEmpty(urlImagen))
                {
                    var img = vm.ArchivoImagen != null
                        ? await _cloudinary.SubirImagenAsync(vm.ArchivoImagen)
                        : await _cloudinary.SubirImagenDesdeUrlAsync(urlImagen!);

                    var anteriorImg = musica.ImagenPublicId;
                    musica.ImagenUrl = img.Url;
                    musica.ImagenPublicId = img.PublicId;

                    if (!string.IsNullOrEmpty(anteriorImg))
                        await _cloudinary.EliminarRecursoAsync(anteriorImg, ResourceType.Image);
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
                await RellenarListasAsync(vm);
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

        // Solo se aceptan enlaces http/https absolutos.
        private static bool EsUrlHttp(string? url) =>
            Uri.TryCreate(url, UriKind.Absolute, out var uri) &&
            (uri.Scheme == Uri.UriSchemeHttp || uri.Scheme == Uri.UriSchemeHttps);

        private async Task RellenarListasAsync(MusicaViewModel vm)
        {
            vm.GenerosDisponibles = await GetGenerosSelectList();
            vm.ArtistasDisponibles = await GetArtistasSelectList();
        }

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
