namespace EcommerceApp.Models
{
    // Datos para la pantalla de músicas del cliente (Inicio y Músicas).
    public class MusicaBrowseViewModel
    {
        public string Q { get; set; } = string.Empty;
        public string Vista { get; set; } = "genero";          // "genero" | "cuadricula"
        public List<GeneroGrupo> Grupos { get; set; } = new();  // vista por género
        public List<Musica> Todas { get; set; } = new();        // vista cuadrícula
        public HashSet<int> FavoritosIds { get; set; } = new();
        public List<Anuncio> Anuncios { get; set; } = new();    // solo en Inicio
    }

    public class GeneroGrupo
    {
        public string Nombre { get; set; } = string.Empty;
        public List<Musica> Canciones { get; set; } = new();
    }

    // Datos del menú de tres puntos de una canción.
    public class CancionMenuVM
    {
        public Musica Musica { get; set; } = null!;
        public bool EsFavorito { get; set; }
        public int? PrimerArtistaId { get; set; }
    }
}
