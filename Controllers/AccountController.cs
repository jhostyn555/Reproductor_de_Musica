using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using EcommerceApp.Models;

namespace EcommerceApp.Controllers
{
    public class AccountController(
        UserManager<ApplicationUser> userManager,
        SignInManager<ApplicationUser> signInManager) : Controller
    {
        [HttpGet]
        public IActionResult Login(string? returnUrl = null)
        {
            ViewData["ReturnUrl"] = returnUrl;
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Login(LoginViewModel model, string? returnUrl = null)
        {
            ViewData["ReturnUrl"] = returnUrl;
            if (!ModelState.IsValid) return View(model);

            // El usuario entra con su correo; el nombre de usuario puede cambiar en el perfil.
            var usuario = await userManager.FindByEmailAsync(model.Email);
            var result = usuario == null
                ? Microsoft.AspNetCore.Identity.SignInResult.Failed
                : await signInManager.PasswordSignInAsync(
                    usuario.UserName!, model.Password, model.RememberMe, lockoutOnFailure: false);

            if (result.Succeeded)
            {
                if (!string.IsNullOrEmpty(returnUrl) && Url.IsLocalUrl(returnUrl))
                    return LocalRedirect(returnUrl);

                return RedirectToAction("Inicio", "User");
            }

            ModelState.AddModelError(string.Empty, "Credenciales inválidas");
            return View(model);
        }

        [HttpGet]
        public IActionResult Register() => View();

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Register(RegisterViewModel model)
        {
            if (!ModelState.IsValid) return View(model);

            var user = new ApplicationUser
            {
                UserName = model.Email,
                Email = model.Email,
                FullName = model.FullName,
                Address = model.Address
            };

            var result = await userManager.CreateAsync(user, model.Password);
            if (result.Succeeded)
            {
                // Todo usuario nuevo recibe el rol "User" (creado al iniciar la app)
                await userManager.AddToRoleAsync(user, "User");
                await signInManager.SignInAsync(user, isPersistent: false);
                return RedirectToAction("Inicio", "User");
            }

            foreach (var error in result.Errors)
                ModelState.AddModelError(string.Empty, error.Description);

            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Logout()
        {
            await signInManager.SignOutAsync();
            // Después de cerrar sesión se muestra Inicio (el login aparece al intentar escuchar).
            return RedirectToAction("Inicio", "User");
        }

        [HttpGet]
        public IActionResult AccessDenied() => View();
    }
}
