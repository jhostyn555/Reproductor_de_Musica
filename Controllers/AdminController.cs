using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace EcommerceApp.Controllers
{
    [Authorize(Roles = "Admin")]
    public class AdminController : Controller
    {
        public IActionResult Dashboard() => View();
        public IActionResult GenerosYArtistas() => RedirectToAction("Index", "Generos");
        public IActionResult Musicas() => RedirectToAction("Index", "Musicas");
        public IActionResult Albumes() => RedirectToAction("Index", "Albumes");
        public IActionResult Playlists() => RedirectToAction("Index", "Playlists");
        public IActionResult Comentarios() => RedirectToAction("Index", "Comentarios");
        public IActionResult Estadisticas() => View();
        public IActionResult Publicidad() => RedirectToAction("Index", "Anuncios");
        public IActionResult Notificaciones() => RedirectToAction("Index", "Notificaciones");
        public IActionResult Usuarios() => RedirectToAction("Index", "Usuarios");
    }
}
