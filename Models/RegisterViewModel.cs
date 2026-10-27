using System.ComponentModel.DataAnnotations;

namespace EcommerceApp.Models
{
    public class RegisterViewModel
    {
        [Display(Name = "Nombre completo")]
        [Required, MaxLength(100)]
        public string FullName { get; set; } = string.Empty;

        [Required, EmailAddress]
        public string Email { get; set; } = string.Empty;

        [Required, DataType(DataType.Password), StringLength(100, MinimumLength = 6)]
        public string Password { get; set; } = string.Empty;

        [Required, DataType(DataType.Password)]
        [Display(Name = "Confirmar contraseña")]
        [Compare(nameof(Password), ErrorMessage = "Las contraseñas no coinciden.")]
        public string ConfirmPassword { get; set; } = string.Empty;

        [Display(Name = "Dirección")]
        public string? Address { get; set; }
    }
}
