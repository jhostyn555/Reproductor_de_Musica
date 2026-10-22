using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace EcommerceApp.Controllers
{
    [Authorize]
    public class UserController : Controller
    {
        [AllowAnonymous]
        public IActionResult Inicio() => View();
        public IActionResult Generos() => View();
        public IActionResult Historial() => View();
        public IActionResult Novedades() => View();
        public IActionResult Perfil() => View();
    }
}