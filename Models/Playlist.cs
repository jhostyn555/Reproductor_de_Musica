using System.ComponentModel.DataAnnotations;

namespace EcommerceApp.Models
{
    // Playlist: pública (la crea el admin) o privada (la crea un cliente).
    public class Playlist
    {
        public int Id { get; set; }

        [Required, MaxLength(120)]
        public string Nombre { get; set; } = string.Empty;

        public bool EsPublica { get; set; }

        // Dueño de la playlist privada (null = pública del admin)
        public string? CreadoPorId { get; set; }

        public DateTime CreadoEn { get; set; } = DateTime.UtcNow;

        // Cola: canciones en orden de reproducción (Orden 1, 2, 3...)
        public ICollection<PlaylistMusica> Canciones { get; set; } = [];
    }
}
