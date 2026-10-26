namespace EcommerceApp.Models
{
    // Canciones y anuncios creados hoy.
    public class NovedadesViewModel
    {
        public List<Musica> Canciones { get; set; } = new();
        public List<Anuncio> Anuncios { get; set; } = new();
    }
}
