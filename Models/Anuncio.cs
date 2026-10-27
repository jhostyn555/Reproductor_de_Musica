using System.ComponentModel.DataAnnotations;

namespace EcommerceApp.Models
{
    public class Anuncio
    {
        public int Id { get; set; }

        [Required, MaxLength(120)]
        public string Titulo { get; set; } = string.Empty;

        // Imagen del banner (URL pública)
        public string? ImagenUrl { get; set; }

        // Enlace al que lleva el anuncio al hacer click (opcional)
        public string? Enlace { get; set; }

        public bool Activo { get; set; } = true;
        public DateTime CreadoEn { get; set; } = DateTime.UtcNow;
    }
}
