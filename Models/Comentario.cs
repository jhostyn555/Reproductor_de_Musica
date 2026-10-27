using System.ComponentModel.DataAnnotations;

namespace EcommerceApp.Models
{
    // Comentario del cliente sobre una canción.
    public class Comentario
    {
        public int Id { get; set; }

        public int MusicaId { get; set; }
        public Musica Musica { get; set; } = null!;

        public string UserId { get; set; } = string.Empty;
        public ApplicationUser User { get; set; } = null!;

        [Required, MaxLength(500)]
        public string Texto { get; set; } = string.Empty;

        public DateTime FechaCreacion { get; set; } = DateTime.UtcNow;
    }
}
