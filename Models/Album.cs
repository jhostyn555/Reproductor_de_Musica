using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace EcommerceApp.Models
{
    public class Album
    {
        public int Id { get; set; }

        [Required, MaxLength(200)]
        public string Titulo { get; set; } = string.Empty;

        public string? ImagenUrl { get; set; }
        public string? ImagenPublicId { get; set; }

        public int ArtistaId { get; set; }
        public Artista Artista { get; set; } = null!;

        // Fecha que escribe el administrador (no se genera automáticamente)
        public DateTime FechaCreacion { get; set; }

        public ICollection<AlbumMusica> AlbumMusicas { get; set; } = [];

        [NotMapped]
        public int CantidadCanciones => AlbumMusicas.Count;

        [NotMapped]
        public int DuracionTotalSegundos => AlbumMusicas.Sum(am => am.Musica?.DuracionSegundos ?? 0);

        [NotMapped]
        public string Tipo => ClasificacionAlbum.Tipo(CantidadCanciones);
    }

    public static class ClasificacionAlbum
    {
        // 1-3 Single, 4-6 EP, 7 y 8-12 LP (el 7 y los 13+ se asignan a LP; ajustable aquí).
        public static string Tipo(int canciones) => canciones switch
        {
            >= 1 and <= 3 => "Single",
            >= 4 and <= 6 => "EP",
            _ => "LP"
        };
    }
}
