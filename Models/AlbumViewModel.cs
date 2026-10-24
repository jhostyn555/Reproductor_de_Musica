using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace EcommerceApp.Models
{
    public class AlbumViewModel
    {
        public int Id { get; set; }
        public string Titulo { get; set; } = string.Empty;

        public IFormFile? ArchivoImagen { get; set; }
        public string? UrlImagen { get; set; }
        public string? ImagenUrlActual { get; set; }

        public int? ArtistaId { get; set; }
        public DateTime? FechaCreacion { get; set; }

        // Orden de selección = orden del tracklist (cola FIFO)
        public List<int> CancionesIds { get; set; } = [];

        public List<SelectListItem> ArtistasDisponibles { get; set; } = [];
    }
}
