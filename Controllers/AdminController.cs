using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace EcommerceApp.Controllers
{
    [Authorize(Roles = "Admin")]
    public class AdminController : Controller
    {
        public IActionResult Dashboard()        => View();
        public IActionResult GenerosYArtistas() => View();
        public IActionResult Musicas()          => RedirectToAction("Index", "Musicas");
        public IActionResult Albumes()          => View();
        public IActionResult Playlists()        => View();
        public IActionResult Comentarios()      => View();
        public IActionResult Estadisticas()     => View();
        public IActionResult Publicidad()       => View();
        public IActionResult Notificaciones()   => View();
        public IActionResult Usuarios()         => View();
    }
}
