namespace EcommerceApp.Models
{
    public class Favorito
    {
        public int Id { get; set; }

        public string UserId { get; set; } = string.Empty;
        public ApplicationUser User { get; set; } = null!;

        public int MusicaId { get; set; }
        public Musica Musica { get; set; } = null!;

        public DateTime AgregadoEn { get; set; } = DateTime.UtcNow;
    }
}
