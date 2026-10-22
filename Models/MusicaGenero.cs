namespace EcommerceApp.Models
{
    public class MusicaGenero
    {
        public int MusicaId { get; set; }
        public Musica Musica { get; set; } = null!;
        public int GeneroId { get; set; }
        public Genero Genero { get; set; } = null!;
    }
}
