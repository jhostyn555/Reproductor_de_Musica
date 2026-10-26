using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using EcommerceApp.Data;
using EcommerceApp.Models;

namespace EcommerceApp.Controllers
{
    // Panel de admin: comentarios agrupados por canción.
    [Authorize(Roles = "Admin")]
    public class ComentariosController : Controller
    {
        private readonly ApplicationDbContext _db;

        public ComentariosController(ApplicationDbContext db)
        {
            _db = db;
        }

        // GET: Comentarios
        public async Task<IActionResult> Index()
        {
            var comentarios = await _db.Comentarios
                .Include(c => c.User)
                .Include(c => c.Musica).ThenInclude(m => m.MusicaArtistas).ThenInclude(ma => ma.Artista)
                .OrderByDescending(c => c.FechaCreacion)
                .ToListAsync();

            var grupos = comentarios
                .GroupBy(c => c.MusicaId)
                .Select(g => new ComentariosPorCancion
                {
                    Musica = g.First().Musica,
                    Comentarios = g.ToList()
                })
                .OrderBy(x => x.Musica.Nombre)
                .ToList();

            ViewData["ActiveMenu"] = "Comentarios";
            return View(grupos);
        }
    }
}
