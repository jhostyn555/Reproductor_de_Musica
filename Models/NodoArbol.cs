namespace EcommerceApp.Models
{
    // Árbol de una playlist: raíz = playlist, hijos = géneros, hojas = canciones.
    public class NodoArbol
    {
        public string Nombre { get; set; } = string.Empty;
        public List<NodoArbol> Hijos { get; set; } = new();
        public List<Musica> Canciones { get; set; } = new();

        // Cantidad de canciones bajo este nodo (incluye sus subnodos)
        public int Total => Canciones.Count + Hijos.Sum(h => h.Total);

        public static NodoArbol PorGenero(string raiz, IEnumerable<Musica> canciones)
        {
            var nodoRaiz = new NodoArbol { Nombre = raiz };
            var grupos = canciones
                .GroupBy(m => m.MusicaGeneros.Select(mg => mg.Genero.Nombre).FirstOrDefault() ?? "Sin género")
                .OrderBy(g => g.Key);

            foreach (var grupo in grupos)
            {
                var nodoGenero = new NodoArbol { Nombre = grupo.Key };
                nodoGenero.Canciones.AddRange(grupo);
                nodoRaiz.Hijos.Add(nodoGenero);
            }
            return nodoRaiz;
        }
    }
}
