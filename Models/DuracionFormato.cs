namespace EcommerceApp.Models
{
    public static class DuracionFormato
    {
        // 3:09 (o --:-- si no se conoce)
        public static string Mmss(int? segundos) =>
            segundos is null or <= 0 ? "--:--" : $"{segundos / 60}:{segundos % 60:00}";

        // "43 min" o "1 h 12 min"
        public static string Total(int segundos)
        {
            var h = segundos / 3600;
            var m = (segundos % 3600) / 60;
            return h > 0 ? $"{h} h {m} min" : $"{m} min";
        }
    }
}
