using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using EcommerceApp.Data;
using EcommerceApp.Models;

namespace EcommerceApp.Controllers
{
    // Pantalla "Géneros y Artistas" del panel de admin + CRUD de géneros.
    [Authorize(Roles = "Admin")]
    public class GenerosController : Controller
    {
        private readonly ApplicationDbContext _db;

        public GenerosController(ApplicationDbContext db)
        {
            _db = db;
        }

        // GET: Generos
        public async Task<IActionResult> Index()
        {
            var vm = new GenerosArtistasViewModel
            {
                Generos = await _db.Generos.Include(g => g.MusicaGeneros).OrderBy(g => g.Nombre).ToListAsync(),
                Artistas = await _db.Artistas.Include(a => a.MusicaArtistas).OrderBy(a => a.Nombre).ToListAsync()
            };
            ViewData["ActiveMenu"] = "GenerosYArtistas";
            return View(vm);
        }

        // GET: Generos/Create
        public IActionResult Create()
        {
            ViewData["ActiveMenu"] = "GenerosYArtistas";
            return View(new Genero());
        }

        // POST: Generos/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(Genero genero)
        {
            genero.Nombre = (genero.Nombre ?? string.Empty).Trim();
            if (!await ValidarNombreAsync(genero, 0))
            {
                ViewData["ActiveMenu"] = "GenerosYArtistas";
                return View(genero);
            }

            _db.Generos.Add(genero);
            await _db.SaveChangesAsync();
            TempData["Success"] = "Género creado.";
            return RedirectToAction(nameof(Index));
        }

        // GET: Generos/Edit/5
        public async Task<IActionResult> Edit(int id)
        {
            var genero = await _db.Generos.FindAsync(id);
            if (genero == null) return NotFound();
            ViewData["ActiveMenu"] = "GenerosYArtistas";
            return View(genero);
        }

        // POST: Generos/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, Genero genero)
        {
            genero.Id = id;
            genero.Nombre = (genero.Nombre ?? string.Empty).Trim();
            if (!await ValidarNombreAsync(genero, id))
            {
                ViewData["ActiveMenu"] = "GenerosYArtistas";
                return View(genero);
            }

            var existente = await _db.Generos.FindAsync(id);
            if (existente == null) return NotFound();

            existente.Nombre = genero.Nombre;
            await _db.SaveChangesAsync();
            TempData["Success"] = "Género actualizado.";
            return RedirectToAction(nameof(Index));
        }

        // POST: Generos/Delete/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(int id)
        {
            var genero = await _db.Generos.FindAsync(id);
            if (genero == null) return NotFound();

            _db.Generos.Remove(genero);
            await _db.SaveChangesAsync();
            TempData["Success"] = "Género eliminado.";
            return RedirectToAction(nameof(Index));
        }

        // Nombre obligatorio y sin repetir (sin distinguir mayúsculas).
        private async Task<bool> ValidarNombreAsync(Genero genero, int idActual)
        {
            if (string.IsNullOrEmpty(genero.Nombre))
            {
                ModelState.AddModelError("Nombre", "El nombre es obligatorio.");
                return false;
            }
            var repetido = await _db.Generos.AnyAsync(g => g.Id != idActual && g.Nombre.ToLower() == genero.Nombre.ToLower());
            if (repetido)
            {
                ModelState.AddModelError("Nombre", "Ya existe un género con ese nombre.");
                return false;
            }
            return true;
        }
    }
}
