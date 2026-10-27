using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using EcommerceApp.Data;
using EcommerceApp.Models;

namespace EcommerceApp.Controllers
{
    // Panel de admin: canciones con sus comentarios. Solo visualización.
    [Authorize(Roles = "Admin")]
    public class ComentariosController : Controller
    {
        private readonly ApplicationDbContext _db;

        public ComentariosController(ApplicationDbContext db)
        {
            _db = db;
        }

        // GET: Comentarios — todas las canciones con su cantidad de comentarios
        public async Task<IActionResult> Index()
        {
            var canciones = await _db.Musicas
                .Include(m => m.MusicaArtistas).ThenInclude(ma => ma.Artista)
                .Include(m => m.MusicaGeneros).ThenInclude(mg => mg.Genero)
                .OrderBy(m => m.Nombre)
                .ToListAsync();

            var conteos = await _db.Comentarios
                .GroupBy(c => c.MusicaId)
                .Select(g => new { MusicaId = g.Key, Total = g.Count() })
                .ToDictionaryAsync(x => x.MusicaId, x => x.Total);

            ViewData["Conteos"] = conteos;
            ViewData["ActiveMenu"] = "Comentarios";
            return View(canciones);
        }

        // GET: Comentarios/Details/5 — comentarios de una canción (solo lectura)
        public async Task<IActionResult> Details(int id)
        {
            var musica = await _db.Musicas
                .Include(m => m.MusicaArtistas).ThenInclude(ma => ma.Artista)
                .Include(m => m.MusicaGeneros).ThenInclude(mg => mg.Genero)
                .FirstOrDefaultAsync(m => m.Id == id);
            if (musica == null) return NotFound();

            var comentarios = await _db.Comentarios
                .Where(c => c.MusicaId == id)
                .Include(c => c.User)
                .OrderByDescending(c => c.FechaCreacion)
                .ToListAsync();

            ViewData["ActiveMenu"] = "Comentarios";
            return View(new ComentariosPorCancion { Musica = musica, Comentarios = comentarios });
        }
    }
}
