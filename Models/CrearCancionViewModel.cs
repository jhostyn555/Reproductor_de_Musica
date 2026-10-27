using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace EcommerceApp.Models
{
    public class CrearCancionViewModel
    {
        [Required, StringLength(200)]
        [Display(Name = "Nombre de la canción")]
        public string Nombre { get; set; } = string.Empty;

        public int? ArtistaId { get; set; }
        public int? GeneroId { get; set; }
        public IFormFile? ArchivoAudio { get; set; }
        public IFormFile? ArchivoImagen { get; set; }

        public List<SelectListItem> ArtistasDisponibles { get; set; } = new();
        public List<SelectListItem> GenerosDisponibles { get; set; } = new();
    }
}
