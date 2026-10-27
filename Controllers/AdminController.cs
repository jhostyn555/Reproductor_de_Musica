using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using EcommerceApp.Data;
using EcommerceApp.Models;
using System;
using System.Linq;
using System.Threading.Tasks;
using System.Collections.Generic;

namespace EcommerceApp.Controllers
{
    [Authorize(Roles = "Admin")]
    public class AdminController : Controller
    {
        private readonly ApplicationDbContext _context;

        public AdminController(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<IActionResult> Dashboard()
        {
            var vm = new DashboardViewModel();

            try
            {
                vm.TotalMusicas = await _context.Musicas.CountAsync();
                vm.TotalArtistas = await _context.Artistas.CountAsync();
                vm.TotalUsuarios = await _context.Users.CountAsync();
                vm.TotalAlbumes = await _context.Albumes.CountAsync();
                vm.TotalReproducciones = await _context.HistorialReproducciones.CountAsync();
                vm.TotalComentarios = await _context.Comentarios.CountAsync();

                // Top 5 últimas canciones agregadas
                var ultimas = await _context.Musicas
                    .Include(m => m.MusicaArtistas).ThenInclude(ma => ma.Artista)
                    .Include(m => m.MusicaGeneros).ThenInclude(mg => mg.Genero)
                    .OrderByDescending(m => m.CreadoEn)
                    .Take(5)
                    .ToListAsync();

                vm.UltimasMusicas = ultimas.Select(m => new DashboardMusicaItem
                {
                    Id = m.Id,
                    Nombre = m.Nombre,
                    ImagenUrl = m.ImagenUrl,
                    Artistas = m.MusicaArtistas.Any() ? string.Join(", ", m.MusicaArtistas.Select(a => a.Artista.Nombre)) : "Sin artista",
                    Generos = m.MusicaGeneros.Any() ? string.Join(", ", m.MusicaGeneros.Select(g => g.Genero.Nombre)) : "Varios",
                    Duracion = DuracionFormato.Mmss(m.DuracionSegundos),
                    CreadoEn = m.CreadoEn
                }).ToList();

                // Top 5 últimos comentarios
                var comentarios = await _context.Comentarios
                    .Include(c => c.Musica)
                    .Include(c => c.User)
                    .OrderByDescending(c => c.FechaCreacion)
                    .Take(5)
                    .ToListAsync();

                vm.UltimosComentarios = comentarios.Select(c => new DashboardComentarioItem
                {
                    Id = c.Id,
                    UsuarioNombre = c.User?.FullName ?? c.User?.UserName ?? "Usuario",
                    UsuarioAvatar = c.User?.AvatarUrl,
                    CancionNombre = c.Musica?.Nombre ?? "Canción",
                    Texto = c.Texto,
                    Fecha = c.FechaCreacion
                }).ToList();

                // Gráfico 1: Reproducciones últimos 7 días
                var hoy = DateTime.UtcNow.Date;
                var dias = new List<string>();
                var reproducidas = new List<int>();

                for (int i = 6; i >= 0; i--)
                {
                    var dia = hoy.AddDays(-i);
                    dias.Add(dia.ToString("ddd d", new System.Globalization.CultureInfo("es-ES")));

                    var diaSiguiente = dia.AddDays(1);
                    int count = await _context.HistorialReproducciones
                        .CountAsync(h => h.FechaReproduccion >= dia && h.FechaReproduccion < diaSiguiente);
                    reproducidas.Add(count);
                }

                // Si aún no hay reproducciones reales en la base de datos, mostramos datos de muestra para el gráfico
                if (reproducidas.All(r => r == 0))
                {
                    reproducidas = new List<int> { 45, 82, 120, 95, 140, 210, 260 };
                }

                vm.DiasLabels = dias;
                vm.ReproduccionesDias = reproducidas;

                // Gráfico 2: Géneros principales con más canciones
                var topGeneros = await _context.Generos
                    .Select(g => new { g.Nombre, Cantidad = g.MusicaGeneros.Count })
                    .OrderByDescending(g => g.Cantidad)
                    .Take(5)
                    .ToListAsync();

                if (topGeneros.Any(g => g.Cantidad > 0))
                {
                    vm.GenerosLabels = topGeneros.Select(g => g.Nombre).ToList();
                    vm.GenerosCantidad = topGeneros.Select(g => g.Cantidad).ToList();
                }
                else
                {
                    vm.GenerosLabels = new List<string> { "Pop", "Rock", "Chicha", "Cumbia", "Trap" };
                    vm.GenerosCantidad = new List<int> { 18, 14, 25, 12, 9 };
                }
            }
            catch
            {
                // En caso de que la bd esté inicializándose
            }

            return View(vm);
        }

        public IActionResult GenerosYArtistas() => RedirectToAction("Index", "Generos");
        public IActionResult Musicas() => RedirectToAction("Index", "Musicas");
        public IActionResult Albumes() => RedirectToAction("Index", "Albumes");
        public IActionResult Playlists() => View();
        public IActionResult Comentarios() => View();
        public IActionResult Estadisticas() => View();
        public IActionResult Publicidad() => RedirectToAction("Index", "Anuncios");
        public IActionResult Notificaciones() => View();
        public IActionResult Usuarios() => View();
    }
}
