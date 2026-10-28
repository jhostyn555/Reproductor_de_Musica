using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.EntityFrameworkCore;
using EcommerceApp.Data;
using EcommerceApp.Models;

namespace EcommerceApp.Services
{
    public static class PlaylistHelpers
    {
        // Degradado "aleatorio" para la portada de una playlist (0-9). Es estable: el mismo id siempre da el mismo color.
        public static int Gradiente(int id) => (int)((((uint)id * 2654435761u) >> 16) % 10u);

        // Canciones aprobadas que se pueden agregar a una playlist.
        public static async Task<List<MusicaOpcion>> DisponiblesAsync(ApplicationDbContext db)
        {
            var musicas = await db.Musicas
                .Where(m => m.Estado == "Aprobada")
                .Include(m => m.MusicaArtistas).ThenInclude(ma => ma.Artista)
                .Include(m => m.MusicaGeneros).ThenInclude(mg => mg.Genero)
                .OrderBy(m => m.Nombre)
                .ToListAsync();

            return musicas.Select(m => new MusicaOpcion
            {
                Id = m.Id,
                Nombre = m.Nombre,
                Artistas = string.Join(", ", m.MusicaArtistas.Select(ma => ma.Artista.Nombre)),
                Genero = m.MusicaGeneros.Select(mg => mg.Genero.Nombre).FirstOrDefault() ?? "Sin género"
            }).ToList();
        }

        // Valida nombre y canciones; devuelve los ids sin repetir, en el orden de la cola.
        public static async Task<List<int>> ValidarAsync(ApplicationDbContext db, PlaylistViewModel vm, ModelStateDictionary ms)
        {
            vm.Nombre = (vm.Nombre ?? string.Empty).Trim();
            if (vm.Nombre.Length == 0) ms.AddModelError("Nombre", "El nombre es obligatorio.");

            var ids = vm.MusicaIds.Distinct().ToList();
            if (ids.Count == 0)
            {
                ms.AddModelError("MusicaIds", "Agrega al menos una canción a la playlist.");
            }
            else
            {
                var validas = await db.Musicas
                    .Where(m => ids.Contains(m.Id) && m.Estado == "Aprobada")
                    .Select(m => m.Id).ToListAsync();
                if (validas.Count != ids.Count)
                    ms.AddModelError("MusicaIds", "Hay canciones que no están disponibles.");
            }
            return ids;
        }

        // Cola: Orden = posición de llegada (FIFO).
        public static void GuardarCola(ApplicationDbContext db, int playlistId, List<int> ids)
        {
            int orden = 1;
            foreach (var id in ids)
                db.PlaylistMusicas.Add(new PlaylistMusica { PlaylistId = playlistId, MusicaId = id, Orden = orden++ });
        }

        public static async Task<Playlist?> CargarConCancionesAsync(ApplicationDbContext db, int id) =>
            await db.Playlists
                .Include(p => p.Canciones).ThenInclude(pm => pm.Musica).ThenInclude(m => m.MusicaGeneros).ThenInclude(mg => mg.Genero)
                .Include(p => p.Canciones).ThenInclude(pm => pm.Musica).ThenInclude(m => m.MusicaArtistas).ThenInclude(ma => ma.Artista)
                .FirstOrDefaultAsync(p => p.Id == id);
    }
}
