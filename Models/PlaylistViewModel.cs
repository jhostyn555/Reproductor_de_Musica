using System.ComponentModel.DataAnnotations;

namespace EcommerceApp.Models
{
    public class PlaylistViewModel
    {
        public int Id { get; set; }

        [Required, StringLength(120)]
        public string Nombre { get; set; } = string.Empty;

        // Orden de la cola (ids de canciones, en el orden en que se agregaron)
        public List<int> MusicaIds { get; set; } = new();

        // Canciones que se pueden agregar (aprobadas)
        public List<MusicaOpcion> Disponibles { get; set; } = new();
    }

    public class MusicaOpcion
    {
        public int Id { get; set; }
        public string Nombre { get; set; } = string.Empty;
        public string Artistas { get; set; } = string.Empty;
        public string Genero { get; set; } = string.Empty;
    }
}
