namespace EcommerceApp.Models
{
    // Una canción dentro de una playlist, con su posición en la cola.
    public class PlaylistMusica
    {
        public int Id { get; set; }

        public int PlaylistId { get; set; }
        public Playlist Playlist { get; set; } = null!;

        public int MusicaId { get; set; }
        public Musica Musica { get; set; } = null!;

        public int Orden { get; set; }
    }
}
