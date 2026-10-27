using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace EcommerceApp.Models
{
    public class MusicaViewModel
    {
        public int Id { get; set; }

        [Required(ErrorMessage = "El nombre es obligatorio")]
        [MaxLength(200)]
        [Display(Name = "Nombre de la canción")]
        public string Nombre { get; set; } = string.Empty;

        [Display(Name = "Archivo de audio")]
        public IFormFile? ArchivoAudio { get; set; }

        [Display(Name = "Imagen de portada")]
        public IFormFile? ArchivoImagen { get; set; }

        [Display(Name = "Enlace de audio (URL)")]
        public string? UrlAudio { get; set; }

        [Display(Name = "Enlace de imagen (URL)")]
        public string? UrlImagen { get; set; }

        // Se rellena en el navegador a partir del audio (segundos)
        public int? DuracionSegundos { get; set; }

        public string? AudioUrlActual { get; set; }
        public string? ImagenUrlActual { get; set; }

        [Display(Name = "Géneros")]
        public List<int> GenerosSeleccionados { get; set; } = [];

        [Display(Name = "Artistas")]
        public List<int> ArtistasSeleccionados { get; set; } = [];

        public List<SelectListItem> GenerosDisponibles { get; set; } = [];
        public List<SelectListItem> ArtistasDisponibles { get; set; } = [];
    }
}
