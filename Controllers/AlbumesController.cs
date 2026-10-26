using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using CloudinaryDotNet.Actions;
using EcommerceApp.Data;
using EcommerceApp.Models;
using EcommerceApp.Services;

namespace EcommerceApp.Controllers
{
    // CRUD de álbumes (sección Álbumes del panel de admin).
    [Authorize(Roles = "Admin")]
    public class AlbumesController : Controller
    {
        private readonly ApplicationDbContext _db;
        private readonly CloudinaryService _cloudinary;

        public AlbumesController(ApplicationDbContext db, CloudinaryService cloudinary)
        {
            _db = db;
            _cloudinary = cloudinary;
        }

        // GET: Albumes?q=texto — busca por título o nombre del artista
        public async Task<IActionResult> Index(string? q)
        {
            q = q?.Trim() ?? string.Empty;

            var query = _db.Albumes
                .Include(a => a.Artista)
                .Include(a => a.AlbumMusicas).ThenInclude(am => am.Musica)
                .AsQueryable();

            if (q.Length > 0)
            {
                var patron = "%" + EscaparLike(q) + "%";
                query = query.Where(a =>
                    EF.Functions.ILike(a.Titulo, patron) ||
                    EF.Functions.ILike(a.Artista.Nombre, patron));
            }

            var albumes = await query.OrderByDescending(a => a.FechaCreacion).ToListAsync();

            ViewData["Q"] = q;
            ViewData["Total"] = q.Length > 0 ? await _db.Albumes.CountAsync() : albumes.Count;
            ViewData["ActiveMenu"] = "Albumes";
            return View(albumes);
        }

        // GET: Albumes/Escuchar/5 — página con las canciones del álbum para escucharlas
        public async Task<IActionResult> Escuchar(int id)
        {
            var album = await _db.Albumes
                .Include(a => a.Artista)
                .Include(a => a.AlbumMusicas).ThenInclude(am => am.Musica).ThenInclude(m => m.MusicaArtistas).ThenInclude(ma => ma.Artista)
                .FirstOrDefaultAsync(a => a.Id == id);

            if (album == null) return NotFound();
            ViewData["ActiveMenu"] = "Albumes";
            return View(album);
        }

        // GET: Albumes/Create
        public async Task<IActionResult> Create()
        {
            var vm = new AlbumViewModel { FechaCreacion = DateTime.Today };
            await RellenarAsync(vm);
            ViewData["ActiveMenu"] = "Albumes";
            return View(vm);
        }

        // POST: Albumes/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        [RequestSizeLimit(20971520)] // 20 MB
        public async Task<IActionResult> Create(AlbumViewModel vm)
        {
            await ValidarAsync(vm, tieneImagenActual: false);
            if (!ModelState.IsValid)
            {
                await RellenarAsync(vm);
                ViewData["ActiveMenu"] = "Albumes";
                return View(vm);
            }

            try
            {
                var (url, publicId) = await SubirImagenAsync(vm);

                var album = new Album
                {
                    Titulo = vm.Titulo.Trim(),
                    ArtistaId = vm.ArtistaId!.Value,
                    // La fecha la escribe el administrador; se guarda en UTC para PostgreSQL.
                    FechaCreacion = DateTime.SpecifyKind(vm.FechaCreacion!.Value.Date, DateTimeKind.Utc),
                    ImagenUrl = url,
                    ImagenPublicId = publicId
                };
                _db.Albumes.Add(album);
                await _db.SaveChangesAsync();

                await GuardarTracklistAsync(album.Id, vm.CancionesIds);

                TempData["Success"] = "Álbum creado exitosamente.";
                return RedirectToAction(nameof(Index));
            }
            catch (Exception ex)
            {
                ModelState.AddModelError("", $"Error al guardar el álbum: {ex.Message}");
                await RellenarAsync(vm);
                ViewData["ActiveMenu"] = "Albumes";
                return View(vm);
            }
        }

        // GET: Albumes/Edit/5
        public async Task<IActionResult> Edit(int id)
        {
            var album = await _db.Albumes
                .Include(a => a.AlbumMusicas)
                .FirstOrDefaultAsync(a => a.Id == id);
            if (album == null) return NotFound();

            var vm = new AlbumViewModel
            {
                Id = album.Id,
                Titulo = album.Titulo,
                ArtistaId = album.ArtistaId,
                FechaCreacion = album.FechaCreacion,
                ImagenUrlActual = album.ImagenUrl,
                CancionesIds = album.AlbumMusicas.OrderBy(am => am.Orden).Select(am => am.MusicaId).ToList()
            };
            await RellenarAsync(vm);
            ViewData["ActiveMenu"] = "Albumes";
            return View(vm);
        }

        // POST: Albumes/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        [RequestSizeLimit(20971520)]
        public async Task<IActionResult> Edit(int id, AlbumViewModel vm)
        {
            var album = await _db.Albumes
                .Include(a => a.AlbumMusicas)
                .FirstOrDefaultAsync(a => a.Id == id);
            if (album == null) return NotFound();

            vm.Id = id;
            await ValidarAsync(vm, tieneImagenActual: !string.IsNullOrEmpty(album.ImagenUrl));
            if (!ModelState.IsValid)
            {
                vm.ImagenUrlActual = album.ImagenUrl;
                await RellenarAsync(vm);
                ViewData["ActiveMenu"] = "Albumes";
                return View(vm);
            }

            try
            {
                // Imagen nueva: se sube primero y luego se borra la anterior.
                if (vm.ArchivoImagen != null || !string.IsNullOrWhiteSpace(vm.UrlImagen))
                {
                    var (url, publicId) = await SubirImagenAsync(vm);
                    var anterior = album.ImagenPublicId;
                    album.ImagenUrl = url;
                    album.ImagenPublicId = publicId;
                    if (!string.IsNullOrEmpty(anterior))
                        await _cloudinary.EliminarRecursoAsync(anterior, ResourceType.Image);
                }

                album.Titulo = vm.Titulo.Trim();
                album.ArtistaId = vm.ArtistaId!.Value;
                album.FechaCreacion = DateTime.SpecifyKind(vm.FechaCreacion!.Value.Date, DateTimeKind.Utc);

                // Reemplazar el tracklist completo respetando el orden de selección.
                _db.AlbumMusicas.RemoveRange(album.AlbumMusicas);
                await _db.SaveChangesAsync();
                await GuardarTracklistAsync(album.Id, vm.CancionesIds);

                TempData["Success"] = "Álbum actualizado exitosamente.";
                return RedirectToAction(nameof(Index));
            }
            catch (Exception ex)
            {
                ModelState.AddModelError("", $"Error al actualizar el álbum: {ex.Message}");
                vm.ImagenUrlActual = album.ImagenUrl;
                await RellenarAsync(vm);
                ViewData["ActiveMenu"] = "Albumes";
                return View(vm);
            }
        }

        // POST: Albumes/Delete/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(int id)
        {
            var album = await _db.Albumes.FindAsync(id);
            if (album == null) return NotFound();

            try
            {
                if (!string.IsNullOrEmpty(album.ImagenPublicId))
                    await _cloudinary.EliminarRecursoAsync(album.ImagenPublicId, ResourceType.Image);

                _db.Albumes.Remove(album);
                await _db.SaveChangesAsync();
                TempData["Success"] = "Álbum eliminado.";
            }
            catch (Exception ex)
            {
                TempData["Error"] = $"Error al eliminar: {ex.Message}";
            }
            return RedirectToAction(nameof(Index));
        }

        // GET: Albumes/CancionesDeArtista?artistaId=3
        // Solo las canciones de ese artista pueden entrar en un álbum.
        [HttpGet]
        public async Task<IActionResult> CancionesDeArtista(int artistaId)
        {
            var canciones = await _db.Musicas
                .Where(m => m.Estado == "Aprobada" && m.MusicaArtistas.Any(ma => ma.ArtistaId == artistaId))
                .OrderBy(m => m.Nombre)
                .Select(m => new { m.Id, m.Nombre, m.DuracionSegundos, m.AudioUrl })
                .ToListAsync();
            return Json(canciones);
        }

        // ── Helpers ──

        // Escapa los comodines de LIKE para buscar el texto literal.
        private static string EscaparLike(string q) =>
            q.Replace("\\", "\\\\").Replace("%", "\\%").Replace("_", "\\_");

        // Cola FIFO: el orden de la lista es el orden de las pistas (1, 2, 3...).
        private async Task GuardarTracklistAsync(int albumId, List<int> cancionesIds)
        {
            var orden = 1;
            foreach (var musicaId in cancionesIds.Distinct())
            {
                _db.AlbumMusicas.Add(new AlbumMusica { AlbumId = albumId, MusicaId = musicaId, Orden = orden++ });
            }
            await _db.SaveChangesAsync();
        }

        private async Task ValidarAsync(AlbumViewModel vm, bool tieneImagenActual)
        {
            if (string.IsNullOrWhiteSpace(vm.Titulo))
                ModelState.AddModelError("Titulo", "El título es obligatorio.");
            if (vm.ArtistaId == null)
                ModelState.AddModelError("ArtistaId", "Selecciona un artista.");
            if (vm.FechaCreacion == null)
                ModelState.AddModelError("FechaCreacion", "Indica la fecha de creación.");

            var url = vm.UrlImagen?.Trim();
            if (vm.ArchivoImagen == null && string.IsNullOrEmpty(url) && !tieneImagenActual)
                ModelState.AddModelError("ArchivoImagen", "Sube una imagen o indica un enlace.");
            if (!string.IsNullOrEmpty(url) && !EsUrlHttp(url))
                ModelState.AddModelError("UrlImagen", "El enlace debe empezar por http:// o https://");

            var ids = vm.CancionesIds.Distinct().ToList();
            if (ids.Count == 0)
            {
                ModelState.AddModelError("CancionesIds", "Agrega al menos una canción del artista.");
            }
            else if (vm.ArtistaId != null)
            {
                // Regla: todas las canciones deben ser del mismo artista.
                var validas = await _db.Musicas
                    .Where(m => ids.Contains(m.Id) && m.MusicaArtistas.Any(ma => ma.ArtistaId == vm.ArtistaId))
                    .Select(m => m.Id)
                    .ToListAsync();
                if (validas.Count != ids.Count)
                    ModelState.AddModelError("CancionesIds", "Todas las canciones deben ser del artista seleccionado.");
            }
        }

        private async Task<(string? Url, string? PublicId)> SubirImagenAsync(AlbumViewModel vm)
        {
            if (vm.ArchivoImagen != null)
            {
                var img = await _cloudinary.SubirImagenAsync(vm.ArchivoImagen);
                return (img.Url, img.PublicId);
            }
            var imgUrl = await _cloudinary.SubirImagenDesdeUrlAsync(vm.UrlImagen!.Trim());
            return (imgUrl.Url, imgUrl.PublicId);
        }

        private async Task RellenarAsync(AlbumViewModel vm)
        {
            vm.ArtistasDisponibles = await _db.Artistas
                .OrderBy(a => a.Nombre)
                .Select(a => new SelectListItem { Value = a.Id.ToString(), Text = a.Nombre })
                .ToListAsync();
        }

        private static bool EsUrlHttp(string? url) =>
            Uri.TryCreate(url, UriKind.Absolute, out var uri) &&
            (uri.Scheme == Uri.UriSchemeHttp || uri.Scheme == Uri.UriSchemeHttps);
    }
}
