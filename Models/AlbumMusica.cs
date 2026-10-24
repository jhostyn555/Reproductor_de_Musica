namespace EcommerceApp.Models
{
    // Relación canción ↔ álbum. Orden = posición en el tracklist (1, 2, 3...).
    public class AlbumMusica
    {
        public int Id { get; set; }

        public int AlbumId { get; set; }
        public Album Album { get; set; } = null!;

        public int MusicaId { get; set; }
        public Musica Musica { get; set; } = null!;

        public int Orden { get; set; }
    }
}
