using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using EcommerceApp.Data;
using EcommerceApp.Models;

namespace EcommerceApp.Controllers
{
    // Solicitudes de canciones enviadas por los clientes: aprobar o rechazar.
    [Authorize(Roles = "Admin")]
    public class NotificacionesController : Controller
    {
        private readonly ApplicationDbContext _db;

        public NotificacionesController(ApplicationDbContext db)
        {
            _db = db;
        }

        // GET: Notificaciones
        public async Task<IActionResult> Index()
        {
            var pendientes = await _db.Musicas
                .Where(m => m.Estado == "Pendiente")
                .Include(m => m.MusicaArtistas).ThenInclude(ma => ma.Artista)
                .Include(m => m.MusicaGeneros).ThenInclude(mg => mg.Genero)
                .OrderBy(m => m.CreadoEn)
                .ToListAsync();

            var ids = pendientes.Where(m => m.CreadoPorId != null).Select(m => m.CreadoPorId!).Distinct().ToList();
            var nombres = await _db.Users.Where(u => ids.Contains(u.Id))
                .ToDictionaryAsync(u => u.Id, u => u.UserName ?? "Usuario");

            ViewData["Nombres"] = nombres;
            ViewData["ActiveMenu"] = "Notificaciones";
            return View(pendientes);
        }

        // POST: Notificaciones/Aprobar/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Aprobar(int id)
        {
            var musica = await _db.Musicas.FindAsync(id);
            if (musica == null) return NotFound();
            musica.Estado = "Aprobada";
            await _db.SaveChangesAsync();
            TempData["Success"] = $"«{musica.Nombre}» quedó pública.";
            return RedirectToAction(nameof(Index));
        }

        // POST: Notificaciones/Rechazar/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Rechazar(int id)
        {
            var musica = await _db.Musicas.FindAsync(id);
            if (musica == null) return NotFound();
            musica.Estado = "Rechazada";
            await _db.SaveChangesAsync();
            TempData["Success"] = $"«{musica.Nombre}» fue rechazada.";
            return RedirectToAction(nameof(Index));
        }
    }
}
