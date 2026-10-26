namespace EcommerceApp.Models
{
    // Cada vez que el cliente empieza a escuchar una canción queda un registro aquí.
    public class HistorialReproduccion
    {
        public int Id { get; set; }

        public string UserId { get; set; } = string.Empty;
        public ApplicationUser User { get; set; } = null!;

        public int MusicaId { get; set; }
        public Musica Musica { get; set; } = null!;

        public DateTime FechaReproduccion { get; set; } = DateTime.UtcNow;
    }

    // Una canción del historial con cuántas veces se escuchó y cuándo fue la última vez.
    public class HistorialItem
    {
        public Musica Musica { get; set; } = null!;
        public int Reproducciones { get; set; }
        public DateTime Ultima { get; set; }
    }
}
