using System.Security.Claims;
using CloudinaryDotNet.Actions;
using Microsoft.AspNetCore.Identity;
using EcommerceApp.Services;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using EcommerceApp.Data;
using EcommerceApp.Models;

namespace EcommerceApp.Controllers
{
    [Authorize]
    public class UserController : Controller
    {
        private readonly ApplicationDbContext _db;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly SignInManager<ApplicationUser> _signInManager;
        private readonly CloudinaryService _cloudinary;

        public UserController(
            ApplicationDbContext db,
            UserManager<ApplicationUser> userManager,
            SignInManager<ApplicationUser> signInManager,
            CloudinaryService cloudinary)
        {
            _db = db;
            _userManager = userManager;
            _signInManager = signInManager;
            _cloudinary = cloudinary;
        }

        private string? UserId => User.FindFirstValue(ClaimTypes.NameIdentifier);

        // GET: User/Inicio?q=&vista=genero|cuadricula
        // Igual que Músicas, pero además muestra anuncios activos (si no hay, la sección no aparece).
        [AllowAnonymous]
        public async Task<IActionResult> Inicio(string? q, string? vista)
        {
            var vm = await ConstruirAsync(q, vista);
            vm.Anuncios = await _db.Anuncios
                .Where(a => a.Activo && !string.IsNullOrEmpty(a.ImagenUrl))
                .OrderByDescending(a => a.CreadoEn)
                .ToListAsync();
            return View(vm);
        }

        // GET: User/Musicas?q=&vista=genero|cuadricula
        public async Task<IActionResult> Musicas(string? q, string? vista)
            => View(await ConstruirAsync(q, vista));

        // GET: User/Artista/5 — canciones de un artista
        public async Task<IActionResult> Artista(int id)
        {
            var artista = await _db.Artistas.FirstOrDefaultAsync(a => a.Id == id);
            if (artista == null) return NotFound();

            var canciones = (await CargarMusicasAsync())
                .Where(m => m.MusicaArtistas.Any(ma => ma.ArtistaId == id))
                .ToList();

            ViewData["ArtistaNombre"] = artista.Nombre;
            return View(new MusicaBrowseViewModel
            {
                Vista = "cuadricula",
                Todas = canciones,
                FavoritosIds = await FavoritosDelUsuarioAsync()
            });
        }

        // POST: User/ToggleFavorito/5 — agrega o quita de favoritos
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ToggleFavorito(int id, string? returnUrl)
        {
            var uid = UserId;
            if (uid == null) return Challenge();

            var existente = await _db.Favoritos.FirstOrDefaultAsync(f => f.UserId == uid && f.MusicaId == id);
            if (existente != null)
            {
                _db.Favoritos.Remove(existente);
                TempData["Success"] = "Canción quitada de favoritos.";
            }
            else
            {
                if (!await _db.Musicas.AnyAsync(m => m.Id == id)) return NotFound();
                _db.Favoritos.Add(new Favorito { UserId = uid, MusicaId = id });
                TempData["Success"] = "Canción agregada a favoritos.";
            }
            await _db.SaveChangesAsync();

            if (!string.IsNullOrEmpty(returnUrl) && Url.IsLocalUrl(returnUrl))
                return LocalRedirect(returnUrl);
            return RedirectToAction(nameof(Musicas));
        }

        // GET: User/Escuchar/5 — reproductor a pantalla completa (solo lectura)
        public async Task<IActionResult> Escuchar(int id)
        {
            var musica = await _db.Musicas
                .Include(m => m.MusicaGeneros).ThenInclude(mg => mg.Genero)
                .Include(m => m.MusicaArtistas).ThenInclude(ma => ma.Artista)
                .FirstOrDefaultAsync(m => m.Id == id);

            // Una canción pendiente solo la puede escuchar quien la envió.
            if (musica == null || (musica.Estado != "Aprobada" && musica.CreadoPorId != UserId)) return NotFound();
            return View(musica);
        }

        // GET: User/Album/5 — tracklist del álbum (solo lectura)
        // GET: User/Albumes — los álbumes que creó el admin
        public async Task<IActionResult> Albumes()
        {
            var albumes = await _db.Albumes
                .Include(a => a.Artista)
                .Include(a => a.AlbumMusicas)
                .OrderByDescending(a => a.FechaCreacion)
                .ToListAsync();
            ViewData["ActiveMenu"] = "Albumes";
            return View(albumes);
        }

        // GET: User/MasDeArtista?artistaId=3&excluirId=5 — otras canciones aprobadas del artista (JSON)
        [HttpGet]
        public async Task<IActionResult> MasDeArtista(int artistaId, int excluirId)
        {
            var lista = await _db.Musicas
                .Where(m => m.Estado == "Aprobada" && m.Id != excluirId && m.MusicaArtistas.Any(ma => ma.ArtistaId == artistaId))
                .Include(m => m.MusicaArtistas).ThenInclude(ma => ma.Artista)
                .OrderBy(m => m.Nombre)
                .Take(12)
                .ToListAsync();

            return Json(lista.Select(m => new
            {
                id = m.Id,
                nombre = m.Nombre,
                artista = string.Join(", ", m.MusicaArtistas.Select(ma => ma.Artista.Nombre)),
                audio = m.AudioUrl,
                imagen = m.ImagenUrl
            }));
        }

        // GET: User/AlbumDelArtista?artistaId=3 — álbum del artista (null si no tiene)
        [HttpGet]
        public async Task<IActionResult> AlbumDelArtista(int artistaId)
        {
            var albumId = await _db.Albumes
                .Where(a => a.ArtistaId == artistaId)
                .OrderByDescending(a => a.FechaCreacion)
                .Select(a => (int?)a.Id)
                .FirstOrDefaultAsync();
            return Json(new { albumId });
        }

        public async Task<IActionResult> Album(int id)
        {
            var album = await _db.Albumes
                .Include(a => a.Artista)
                .Include(a => a.AlbumMusicas).ThenInclude(am => am.Musica).ThenInclude(m => m.MusicaArtistas).ThenInclude(ma => ma.Artista)
                .FirstOrDefaultAsync(a => a.Id == id);

            if (album == null) return NotFound();
            return View(album);
        }

        // POST: User/MeGusta (pulgar). Usa la misma relación que los favoritos.
        // Responde JSON: { meGusta: true|false }
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> MeGusta(int id)
        {
            var uid = UserId;
            if (uid == null) return Unauthorized();

            var existente = await _db.Favoritos.FirstOrDefaultAsync(f => f.UserId == uid && f.MusicaId == id);
            bool meGusta;
            if (existente != null)
            {
                _db.Favoritos.Remove(existente);
                meGusta = false;
            }
            else
            {
                if (!await _db.Musicas.AnyAsync(m => m.Id == id)) return NotFound();
                _db.Favoritos.Add(new Favorito { UserId = uid, MusicaId = id });
                meGusta = true;
            }
            await _db.SaveChangesAsync();
            return Json(new { meGusta });
        }

        // GET: User/Historial — canciones escuchadas por el cliente (la más reciente primero)
        public async Task<IActionResult> Historial()
        {
            var uid = UserId;
            var registros = uid == null
                ? new List<HistorialReproduccion>()
                : await _db.HistorialReproducciones
                    .Where(h => h.UserId == uid)
                    .Include(h => h.Musica).ThenInclude(m => m.MusicaArtistas).ThenInclude(ma => ma.Artista)
                    .OrderByDescending(h => h.FechaReproduccion)
                    .Take(500)
                    .ToListAsync();

            var canciones = registros
                .GroupBy(h => h.MusicaId)
                .Select(g => new HistorialItem
                {
                    Musica = g.First().Musica,
                    Reproducciones = g.Count(),
                    Ultima = g.First().FechaReproduccion
                })
                .OrderByDescending(x => x.Ultima)
                .ToList();

            ViewData["ActiveMenu"] = "Historial";
            return View(canciones);
        }

        // POST: User/RegistrarReproduccion/5 — la llama el reproductor cuando una canción empieza a sonar
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> RegistrarReproduccion(int id)
        {
            var uid = UserId;
            if (uid == null) return Unauthorized();
            if (!await _db.Musicas.AnyAsync(m => m.Id == id)) return NotFound();

            _db.HistorialReproducciones.Add(new HistorialReproduccion
            {
                UserId = uid,
                MusicaId = id,
                FechaReproduccion = DateTime.UtcNow
            });
            await _db.SaveChangesAsync();
            return Ok();
        }
        // GET: User/Novedades — canciones y anuncios creados hoy (hora local del servidor)
        public async Task<IActionResult> Novedades()
        {
            var inicio = DateTime.Now.Date.ToUniversalTime();
            var fin = DateTime.Now.Date.AddDays(1).ToUniversalTime();

            var canciones = await _db.Musicas
                .Where(m => m.Estado == "Aprobada" && m.CreadoEn >= inicio && m.CreadoEn < fin)
                .Include(m => m.MusicaArtistas).ThenInclude(ma => ma.Artista)
                .OrderBy(m => m.Nombre)
                .ToListAsync();

            var anuncios = await _db.Anuncios
                .Where(a => a.Activo && a.CreadoEn >= inicio && a.CreadoEn < fin)
                .OrderByDescending(a => a.CreadoEn)
                .ToListAsync();

            ViewData["ActiveMenu"] = "Novedades";
            return View(new NovedadesViewModel { Canciones = canciones, Anuncios = anuncios });
        }

        // GET: User/Perfil
        public async Task<IActionResult> Perfil()
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null) return Challenge();

            ViewData["ActiveMenu"] = "Perfil";
            return View(new PerfilViewModel
            {
                UserName = user.UserName ?? string.Empty,
                Email = user.Email ?? string.Empty,
                AvatarUrl = user.AvatarUrl
            });
        }

        // POST: User/PerfilNombre — cambia el nombre de usuario
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> PerfilNombre(PerfilViewModel model)
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null) return Challenge();

            var nombre = (model.UserName ?? string.Empty).Trim();
            if (nombre.Length < 3 || nombre.Length > 30)
            {
                TempData["Error"] = "El nombre de usuario debe tener entre 3 y 30 caracteres.";
                return RedirectToAction(nameof(Perfil));
            }

            var resultado = await _userManager.SetUserNameAsync(user, nombre);
            if (!resultado.Succeeded)
            {
                TempData["Error"] = string.Join(" ", resultado.Errors.Select(e => e.Description));
                return RedirectToAction(nameof(Perfil));
            }

            await _signInManager.RefreshSignInAsync(user);
            TempData["Success"] = "Nombre de usuario actualizado.";
            return RedirectToAction(nameof(Perfil));
        }

        // POST: User/PerfilAvatar — sube una nueva imagen de perfil
        [HttpPost]
        [ValidateAntiForgeryToken]
        [RequestSizeLimit(5242880)] // 5 MB
        public async Task<IActionResult> PerfilAvatar(IFormFile? archivo)
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null) return Challenge();

            if (archivo == null || archivo.Length == 0)
            {
                TempData["Error"] = "Selecciona una imagen.";
                return RedirectToAction(nameof(Perfil));
            }
            if (!(archivo.ContentType ?? string.Empty).StartsWith("image/"))
            {
                TempData["Error"] = "El archivo debe ser una imagen.";
                return RedirectToAction(nameof(Perfil));
            }

            try
            {
                var anterior = user.AvatarPublicId;
                var (url, publicId) = await _cloudinary.SubirImagenAsync(archivo, "usuarios/avatares");
                user.AvatarUrl = url;
                user.AvatarPublicId = publicId;
                await _userManager.UpdateAsync(user);

                if (!string.IsNullOrEmpty(anterior))
                    await _cloudinary.EliminarRecursoAsync(anterior, ResourceType.Image);

                TempData["Success"] = "Avatar actualizado.";
            }
            catch (Exception ex)
            {
                TempData["Error"] = "No se pudo subir la imagen: " + ex.Message;
            }
            return RedirectToAction(nameof(Perfil));
        }

        // GET: User/CrearCancion — el cliente envía una canción para aprobación
        public async Task<IActionResult> CrearCancion()
        {
            var vm = new CrearCancionViewModel();
            await RellenarCrearCancionAsync(vm);
            ViewData["ActiveMenu"] = "Perfil";
            return View(vm);
        }

        // POST: User/CrearCancion
        [HttpPost]
        [ValidateAntiForgeryToken]
        [RequestSizeLimit(52428800)] // 50 MB
        public async Task<IActionResult> CrearCancion(CrearCancionViewModel vm)
        {
            var uid = UserId;
            if (uid == null) return Challenge();

            vm.Nombre = (vm.Nombre ?? string.Empty).Trim();
            if (vm.Nombre.Length == 0) ModelState.AddModelError("Nombre", "El nombre es obligatorio.");
            if (vm.ArtistaId == null) ModelState.AddModelError("ArtistaId", "Selecciona un artista.");
            if (vm.GeneroId == null) ModelState.AddModelError("GeneroId", "Selecciona un género.");
            if (vm.ArchivoAudio == null || vm.ArchivoAudio.Length == 0)
                ModelState.AddModelError("ArchivoAudio", "Sube el archivo de audio.");

            if (!ModelState.IsValid)
            {
                await RellenarCrearCancionAsync(vm);
                ViewData["ActiveMenu"] = "Perfil";
                return View(vm);
            }

            try
            {
                var audio = await _cloudinary.SubirAudioAsync(vm.ArchivoAudio!);
                var musica = new Musica
                {
                    Nombre = vm.Nombre,
                    AudioUrl = audio.Url,
                    AudioPublicId = audio.PublicId,
                    Estado = "Pendiente",
                    CreadoPorId = uid
                };

                if (vm.ArchivoImagen != null && vm.ArchivoImagen.Length > 0)
                {
                    var img = await _cloudinary.SubirImagenAsync(vm.ArchivoImagen);
                    musica.ImagenUrl = img.Url;
                    musica.ImagenPublicId = img.PublicId;
                }

                _db.Musicas.Add(musica);
                await _db.SaveChangesAsync();

                _db.Set<MusicaArtista>().Add(new MusicaArtista { MusicaId = musica.Id, ArtistaId = vm.ArtistaId!.Value });
                _db.Set<MusicaGenero>().Add(new MusicaGenero { MusicaId = musica.Id, GeneroId = vm.GeneroId!.Value });
                await _db.SaveChangesAsync();

                TempData["Success"] = "Canción enviada. Será pública cuando el administrador la apruebe.";
                return RedirectToAction(nameof(Perfil));
            }
            catch (Exception ex)
            {
                ModelState.AddModelError("", "No se pudo subir la canción: " + ex.Message);
                await RellenarCrearCancionAsync(vm);
                ViewData["ActiveMenu"] = "Perfil";
                return View(vm);
            }
        }

        private async Task RellenarCrearCancionAsync(CrearCancionViewModel vm)
        {
            vm.ArtistasDisponibles = await _db.Artistas.OrderBy(a => a.Nombre)
                .Select(a => new SelectListItem { Value = a.Id.ToString(), Text = a.Nombre }).ToListAsync();
            vm.GenerosDisponibles = await _db.Generos.OrderBy(g => g.Nombre)
                .Select(g => new SelectListItem { Value = g.Id.ToString(), Text = g.Nombre }).ToListAsync();
        }

        // GET: User/Comentarios?musicaId=5 — comentarios de una canción (JSON)
        [HttpGet]
        public async Task<IActionResult> Comentarios(int musicaId)
        {
            var lista = await _db.Comentarios
                .Where(c => c.MusicaId == musicaId)
                .Include(c => c.User)
                .OrderByDescending(c => c.FechaCreacion)
                .Take(100)
                .Select(c => new
                {
                    id = c.Id,
                    usuario = c.User.UserName ?? "Usuario",
                    texto = c.Texto,
                    fecha = c.FechaCreacion
                })
                .ToListAsync();
            return Json(lista);
        }

        // POST: User/Comentar — publica un comentario en una canción (JSON)
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Comentar(int musicaId, string texto)
        {
            var uid = UserId;
            if (uid == null) return Unauthorized();

            var limpio = (texto ?? string.Empty).Trim();
            if (limpio.Length == 0 || limpio.Length > 500)
                return BadRequest("El comentario debe tener entre 1 y 500 caracteres.");
            if (!await _db.Musicas.AnyAsync(m => m.Id == musicaId)) return NotFound();

            var comentario = new Comentario
            {
                MusicaId = musicaId,
                UserId = uid,
                Texto = limpio,
                FechaCreacion = DateTime.UtcNow
            };
            _db.Comentarios.Add(comentario);
            await _db.SaveChangesAsync();

            var usuario = (await _userManager.FindByIdAsync(uid))?.UserName ?? "Usuario";
            return Json(new { id = comentario.Id, usuario, texto = comentario.Texto, fecha = comentario.FechaCreacion });
        }

        // ── Helpers ──

        private async Task<List<Musica>> CargarMusicasAsync() =>
            await _db.Musicas
                .Where(m => m.Estado == "Aprobada")
                .Include(m => m.MusicaGeneros).ThenInclude(mg => mg.Genero)
                .Include(m => m.MusicaArtistas).ThenInclude(ma => ma.Artista)
                .Include(m => m.AlbumMusicas)
                .OrderBy(m => m.Nombre)
                .ToListAsync();

        private async Task<HashSet<int>> FavoritosDelUsuarioAsync()
        {
            var uid = UserId;
            if (uid == null) return new HashSet<int>();
            var ids = await _db.Favoritos.Where(f => f.UserId == uid).Select(f => f.MusicaId).ToListAsync();
            return ids.ToHashSet();
        }

        // Busca por nombre de canción, artista o género (sin distinguir mayúsculas).
        private static bool Coincide(Musica m, string q) =>
            m.Nombre.Contains(q, StringComparison.OrdinalIgnoreCase)
            || m.MusicaArtistas.Any(ma => ma.Artista.Nombre.Contains(q, StringComparison.OrdinalIgnoreCase))
            || m.MusicaGeneros.Any(mg => mg.Genero.Nombre.Contains(q, StringComparison.OrdinalIgnoreCase));

        private async Task<MusicaBrowseViewModel> ConstruirAsync(string? q, string? vista)
        {
            q = q?.Trim() ?? string.Empty;

            var musicas = await CargarMusicasAsync();
            if (q.Length > 0)
                musicas = musicas.Where(m => Coincide(m, q)).ToList();

            // Un título por género; debajo solo las canciones (ya filtradas) de ese género.
            var grupos = musicas
                .SelectMany(m => m.MusicaGeneros.Select(mg => mg.Genero))
                .DistinctBy(g => g.Id)
                .OrderBy(g => g.Nombre)
                .Select(g => new GeneroGrupo
                {
                    Nombre = g.Nombre,
                    Canciones = musicas.Where(m => m.MusicaGeneros.Any(mg => mg.GeneroId == g.Id)).ToList()
                })
                .Where(gr => gr.Canciones.Count > 0)
                .ToList();

            return new MusicaBrowseViewModel
            {
                Q = q,
                Vista = vista == "cuadricula" ? "cuadricula" : "genero",
                Grupos = grupos,
                Todas = musicas,
                FavoritosIds = await FavoritosDelUsuarioAsync()
            };
        }
    }
}
