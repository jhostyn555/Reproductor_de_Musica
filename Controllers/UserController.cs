using System.Security.Claims;
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

        public UserController(ApplicationDbContext db)
        {
            _db = db;
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

            if (musica == null) return NotFound();
            return View(musica);
        }

        public IActionResult Historial() => View();
        public IActionResult Novedades() => View();
        public IActionResult Perfil() => View();

        // ── Helpers ──

        private async Task<List<Musica>> CargarMusicasAsync() =>
            await _db.Musicas
                .Include(m => m.MusicaGeneros).ThenInclude(mg => mg.Genero)
                .Include(m => m.MusicaArtistas).ThenInclude(ma => ma.Artista)
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
