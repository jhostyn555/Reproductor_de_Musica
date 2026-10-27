using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using EcommerceApp.Data;
using EcommerceApp.Models;

namespace EcommerceApp.Controllers
{
    // CRUD de artistas (se gestiona desde la pantalla "Géneros y Artistas").
    [Authorize(Roles = "Admin")]
    public class ArtistasController : Controller
    {
        private readonly ApplicationDbContext _db;

        public ArtistasController(ApplicationDbContext db)
        {
            _db = db;
        }

        // GET: Artistas/Create
        public IActionResult Create()
        {
            ViewData["ActiveMenu"] = "GenerosYArtistas";
            return View(new Artista());
        }

        // POST: Artistas/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(Artista artista)
        {
            artista.Nombre = (artista.Nombre ?? string.Empty).Trim();
            if (!await ValidarNombreAsync(artista, 0))
            {
                ViewData["ActiveMenu"] = "GenerosYArtistas";
                return View(artista);
            }

            _db.Artistas.Add(artista);
            await _db.SaveChangesAsync();
            TempData["Success"] = "Artista creado.";
            return RedirectToAction("Index", "Generos");
        }

        // GET: Artistas/Edit/5
        public async Task<IActionResult> Edit(int id)
        {
            var artista = await _db.Artistas.FindAsync(id);
            if (artista == null) return NotFound();
            ViewData["ActiveMenu"] = "GenerosYArtistas";
            return View(artista);
        }

        // POST: Artistas/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, Artista artista)
        {
            artista.Id = id;
            artista.Nombre = (artista.Nombre ?? string.Empty).Trim();
            if (!await ValidarNombreAsync(artista, id))
            {
                ViewData["ActiveMenu"] = "GenerosYArtistas";
                return View(artista);
            }

            var existente = await _db.Artistas.FindAsync(id);
            if (existente == null) return NotFound();

            existente.Nombre = artista.Nombre;
            await _db.SaveChangesAsync();
            TempData["Success"] = "Artista actualizado.";
            return RedirectToAction("Index", "Generos");
        }

        // POST: Artistas/Delete/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(int id)
        {
            var artista = await _db.Artistas.FindAsync(id);
            if (artista == null) return NotFound();

            try
            {
                _db.Artistas.Remove(artista);
                await _db.SaveChangesAsync();
                TempData["Success"] = "Artista eliminado.";
            }
            catch (DbUpdateException)
            {
                // Un artista con álbumes no se puede borrar (restricción de la base de datos).
                _db.Entry(artista).State = EntityState.Unchanged;
                TempData["Error"] = "No se puede eliminar: el artista tiene álbumes asociados.";
            }
            return RedirectToAction("Index", "Generos");
        }

        private async Task<bool> ValidarNombreAsync(Artista artista, int idActual)
        {
            if (string.IsNullOrEmpty(artista.Nombre))
            {
                ModelState.AddModelError("Nombre", "El nombre es obligatorio.");
                return false;
            }
            var repetido = await _db.Artistas.AnyAsync(a => a.Id != idActual && a.Nombre.ToLower() == artista.Nombre.ToLower());
            if (repetido)
            {
                ModelState.AddModelError("Nombre", "Ya existe un artista con ese nombre.");
                return false;
            }
            return true;
        }
    }
}
