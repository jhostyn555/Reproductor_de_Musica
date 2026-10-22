namespace EcommerceApp.Models
{
    public class MusicaArtista
    {
        public int MusicaId { get; set; }
        public Musica Musica { get; set; } = null!;
        public int ArtistaId { get; set; }
        public Artista Artista { get; set; } = null!;
    }
}
