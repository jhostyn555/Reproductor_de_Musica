using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using EcommerceApp.Data;

namespace EcommerceApp.Controllers
{
    // Panel de admin: nombres de los usuarios registrados. Solo visualización.
    [Authorize(Roles = "Admin")]
    public class UsuariosController : Controller
    {
        private readonly ApplicationDbContext _db;

        public UsuariosController(ApplicationDbContext db)
        {
            _db = db;
        }

        // GET: Usuarios
        public async Task<IActionResult> Index()
        {
            var usuarios = await _db.Users
                .OrderBy(u => u.UserName)
                .ToListAsync();
            ViewData["ActiveMenu"] = "Usuarios";
            return View(usuarios);
        }
    }
}
