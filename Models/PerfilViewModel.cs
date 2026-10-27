using System.ComponentModel.DataAnnotations;

namespace EcommerceApp.Models
{
    public class PerfilViewModel
    {
        [Required, StringLength(30, MinimumLength = 3)]
        [Display(Name = "Nombre de usuario")]
        public string UserName { get; set; } = string.Empty;

        public string Email { get; set; } = string.Empty;
        public string? AvatarUrl { get; set; }
    }
}
