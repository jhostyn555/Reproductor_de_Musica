using System.ComponentModel.DataAnnotations;
namespace EcommerceApp.Models
{
    public class Musica
    {
        public int Id { get; set; }
        [Required, MaxLength(200)]
        public string Nombre { get; set; } = string.Empty;
        public string? AudioUrl { get; set; }
        public string? AudioPublicId { get; set; }
        public string? ImagenUrl { get; set; }
        public string? ImagenPublicId { get; set; }
        public DateTime CreadoEn { get; set; } = DateTime.UtcNow;
        public int? DuracionSegundos { get; set; }
        public ICollection<MusicaGenero> MusicaGeneros { get; set; } = [];
        public ICollection<MusicaArtista> MusicaArtistas { get; set; } = [];
        public ICollection<AlbumMusica> AlbumMusicas { get; set; } = [];
    }
}
