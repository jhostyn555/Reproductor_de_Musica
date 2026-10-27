namespace EcommerceApp.Models
{
    // Comentarios de una misma canción (para el panel de admin).
    public class ComentariosPorCancion
    {
        public Musica Musica { get; set; } = null!;
        public List<Comentario> Comentarios { get; set; } = new();
    }
}
