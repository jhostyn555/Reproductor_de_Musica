using System;
using System.Collections.Generic;

namespace EcommerceApp.Models
{
    public class DashboardViewModel
    {
        public int TotalMusicas { get; set; }
        public int TotalArtistas { get; set; }
        public int TotalUsuarios { get; set; }
        public int TotalAlbumes { get; set; }
        public int TotalReproducciones { get; set; }
        public int TotalComentarios { get; set; }

        public List<DashboardMusicaItem> UltimasMusicas { get; set; } = new();
        public List<DashboardComentarioItem> UltimosComentarios { get; set; } = new();

        public List<string> DiasLabels { get; set; } = new();
        public List<int> ReproduccionesDias { get; set; } = new();

        public List<string> GenerosLabels { get; set; } = new();
        public List<int> GenerosCantidad { get; set; } = new();
    }

    public class DashboardMusicaItem
    {
        public int Id { get; set; }
        public string Nombre { get; set; } = string.Empty;
        public string? ImagenUrl { get; set; }
        public string Artistas { get; set; } = string.Empty;
        public string Generos { get; set; } = string.Empty;
        public string Duracion { get; set; } = "--:--";
        public DateTime CreadoEn { get; set; }
    }

    public class DashboardComentarioItem
    {
        public int Id { get; set; }
        public string UsuarioNombre { get; set; } = string.Empty;
        public string? UsuarioAvatar { get; set; }
        public string CancionNombre { get; set; } = string.Empty;
        public string Texto { get; set; } = string.Empty;
        public DateTime Fecha { get; set; }
    }
}
