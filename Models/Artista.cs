using System.ComponentModel.DataAnnotations;
namespace EcommerceApp.Models
{
    public class Artista
    {
        public int Id { get; set; }
        [Required, MaxLength(120)]
        public string Nombre { get; set; } = string.Empty;
        public ICollection<MusicaArtista> MusicaArtistas { get; set; } = [];
    }
}
