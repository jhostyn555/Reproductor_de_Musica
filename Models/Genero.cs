using System.ComponentModel.DataAnnotations;
namespace EcommerceApp.Models
{
    public class Genero
    {
        public int Id { get; set; }
        [Required, MaxLength(80)]
        public string Nombre { get; set; } = string.Empty;
        public ICollection<MusicaGenero> MusicaGeneros { get; set; } = [];
    }
}
