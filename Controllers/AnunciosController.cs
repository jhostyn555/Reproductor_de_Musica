using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using EcommerceApp.Data;
using EcommerceApp.Models;

namespace EcommerceApp.Controllers
{
    // Gestión de anuncios (sección Publicidad del panel de admin).
    [Authorize(Roles = "Admin")]
    public class AnunciosController : Controller
    {
        private readonly ApplicationDbContext _db;

        public AnunciosController(ApplicationDbContext db)
        {
            _db = db;
        }

        // GET: Anuncios
        public async Task<IActionResult> Index()
        {
            var anuncios = await _db.Anuncios.OrderByDescending(a => a.CreadoEn).ToListAsync();
            ViewData["ActiveMenu"] = "Publicidad";
            return View(anuncios);
        }

        // GET: Anuncios/Create
        public IActionResult Create()
        {
            ViewData["ActiveMenu"] = "Publicidad";
            return View(new Anuncio());
        }

        // POST: Anuncios/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(Anuncio anuncio)
        {
            anuncio.ImagenUrl = anuncio.ImagenUrl?.Trim();
            anuncio.Enlace = anuncio.Enlace?.Trim();

            if (string.IsNullOrEmpty(anuncio.ImagenUrl) || !EsUrlHttp(anuncio.ImagenUrl))
                ModelState.AddModelError("ImagenUrl", "Indica la URL de la imagen del anuncio (http:// o https://).");
            if (!string.IsNullOrEmpty(anuncio.Enlace) && !EsUrlHttp(anuncio.Enlace))
                ModelState.AddModelError("Enlace", "El enlace debe empezar por http:// o https://");

            if (!ModelState.IsValid)
            {
                ViewData["ActiveMenu"] = "Publicidad";
                return View(anuncio);
            }

            _db.Anuncios.Add(anuncio);
            await _db.SaveChangesAsync();
            TempData["Success"] = "Anuncio creado.";
            return RedirectToAction(nameof(Index));
        }

        // POST: Anuncios/ToggleActivo/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ToggleActivo(int id)
        {
            var anuncio = await _db.Anuncios.FindAsync(id);
            if (anuncio == null) return NotFound();

            anuncio.Activo = !anuncio.Activo;
            await _db.SaveChangesAsync();
            TempData["Success"] = anuncio.Activo ? "Anuncio activado." : "Anuncio desactivado.";
            return RedirectToAction(nameof(Index));
        }

        // POST: Anuncios/Delete/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(int id)
        {
            var anuncio = await _db.Anuncios.FindAsync(id);
            if (anuncio == null) return NotFound();

            _db.Anuncios.Remove(anuncio);
            await _db.SaveChangesAsync();
            TempData["Success"] = "Anuncio eliminado.";
            return RedirectToAction(nameof(Index));
        }

        private static bool EsUrlHttp(string? url) =>
            Uri.TryCreate(url, UriKind.Absolute, out var uri) &&
            (uri.Scheme == Uri.UriSchemeHttp || uri.Scheme == Uri.UriSchemeHttps);
    }
}
