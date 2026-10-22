using EcommerceApp.Models;
using Microsoft.AspNetCore.Mvc;
using System.Diagnostics;

namespace EcommerceApp.Controllers
{
    public class HomeController : Controller
    {
        /// <summary>
        /// Punto de entrada principal: redirige según el estado de autenticación.
        /// - Autenticado  → Panel de usuario (User/Inicio)
        /// - No autenticado → Login
        /// </summary>
        public IActionResult Index()
        {
            if (User.Identity?.IsAuthenticated == true)
                return RedirectToAction("Inicio", "User");

            return RedirectToAction("Login", "Account");
        }

        public IActionResult Privacy() => View();

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}
