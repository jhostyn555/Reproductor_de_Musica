using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using EcommerceApp.Models;

namespace EcommerceApp.Data
{
    public class ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
        : IdentityDbContext<ApplicationUser>(options)
    {
        public DbSet<Product> Products { get; set; }
        public DbSet<Musica> Musicas { get; set; }
        public DbSet<Genero> Generos { get; set; }
        public DbSet<Artista> Artistas { get; set; }
        public DbSet<Anuncio> Anuncios { get; set; }
        public DbSet<Favorito> Favoritos { get; set; }
        public DbSet<HistorialReproduccion> HistorialReproducciones { get; set; }
        public DbSet<Comentario> Comentarios { get; set; }
        public DbSet<Album> Albumes { get; set; }
        public DbSet<AlbumMusica> AlbumMusicas { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            modelBuilder.Entity<Product>()
                .Property(p => p.Price)
                .HasPrecision(18, 2);

            // MusicaGenero composite PK
            modelBuilder.Entity<MusicaGenero>()
                .HasKey(mg => new { mg.MusicaId, mg.GeneroId });
            modelBuilder.Entity<MusicaGenero>()
                .HasOne(mg => mg.Musica).WithMany(m => m.MusicaGeneros).HasForeignKey(mg => mg.MusicaId);
            modelBuilder.Entity<MusicaGenero>()
                .HasOne(mg => mg.Genero).WithMany(g => g.MusicaGeneros).HasForeignKey(mg => mg.GeneroId);

            // MusicaArtista composite PK
            modelBuilder.Entity<MusicaArtista>()
                .HasKey(ma => new { ma.MusicaId, ma.ArtistaId });
            modelBuilder.Entity<MusicaArtista>()
                .HasOne(ma => ma.Musica).WithMany(m => m.MusicaArtistas).HasForeignKey(ma => ma.MusicaId);
            modelBuilder.Entity<MusicaArtista>()
                .HasOne(ma => ma.Artista).WithMany(a => a.MusicaArtistas).HasForeignKey(ma => ma.ArtistaId);

            // Favoritos: un usuario no puede marcar la misma canción dos veces
            modelBuilder.Entity<Favorito>()
                .HasIndex(f => new { f.UserId, f.MusicaId })
                .IsUnique();
            modelBuilder.Entity<Favorito>()
                .HasOne(f => f.Musica).WithMany().HasForeignKey(f => f.MusicaId).OnDelete(DeleteBehavior.Cascade);
            modelBuilder.Entity<Favorito>()
                .HasOne(f => f.User).WithMany().HasForeignKey(f => f.UserId).OnDelete(DeleteBehavior.Cascade);

            // Álbumes: un artista no se puede borrar si tiene álbumes; al borrar un álbum se borra su tracklist
            modelBuilder.Entity<Album>()
                .HasOne(a => a.Artista).WithMany().HasForeignKey(a => a.ArtistaId).OnDelete(DeleteBehavior.Restrict);
            modelBuilder.Entity<AlbumMusica>()
                .HasOne(am => am.Album).WithMany(a => a.AlbumMusicas).HasForeignKey(am => am.AlbumId).OnDelete(DeleteBehavior.Cascade);
            modelBuilder.Entity<AlbumMusica>()
                .HasOne(am => am.Musica).WithMany(m => m.AlbumMusicas).HasForeignKey(am => am.MusicaId).OnDelete(DeleteBehavior.Cascade);
            modelBuilder.Entity<AlbumMusica>()
                .HasIndex(am => new { am.AlbumId, am.MusicaId }).IsUnique();

            // Historial de reproducciones del cliente
            modelBuilder.Entity<HistorialReproduccion>()
                .HasIndex(h => new { h.UserId, h.FechaReproduccion });
            modelBuilder.Entity<HistorialReproduccion>()
                .HasOne(h => h.Musica).WithMany().HasForeignKey(h => h.MusicaId).OnDelete(DeleteBehavior.Cascade);
            modelBuilder.Entity<HistorialReproduccion>()
                .HasOne(h => h.User).WithMany().HasForeignKey(h => h.UserId).OnDelete(DeleteBehavior.Cascade);

            // Comentarios de canciones
            modelBuilder.Entity<Comentario>()
                .HasOne(c => c.Musica).WithMany().HasForeignKey(c => c.MusicaId).OnDelete(DeleteBehavior.Cascade);
            modelBuilder.Entity<Comentario>()
                .HasOne(c => c.User).WithMany().HasForeignKey(c => c.UserId).OnDelete(DeleteBehavior.Cascade);

            // Seed géneros
            modelBuilder.Entity<Genero>().HasData(
                new Genero { Id = 1, Nombre = "Pop" },
                new Genero { Id = 2, Nombre = "Rock" },
                new Genero { Id = 3, Nombre = "Hip-Hop" },
                new Genero { Id = 4, Nombre = "Electrónica" },
                new Genero { Id = 5, Nombre = "Reggaetón" },
                new Genero { Id = 6, Nombre = "Jazz" },
                new Genero { Id = 7, Nombre = "Clásica" },
                new Genero { Id = 8, Nombre = "R&B" },
                new Genero { Id = 9, Nombre = "Metal" },
                new Genero { Id = 10, Nombre = "Salsa" },
                new Genero { Id = 11, Nombre = "Cumbia" },
                new Genero { Id = 12, Nombre = "Bachata" },
                new Genero { Id = 13, Nombre = "Trap" },
                new Genero { Id = 14, Nombre = "Indie" },
                new Genero { Id = 15, Nombre = "Folk" }
            );

            // Seed artistas
            modelBuilder.Entity<Artista>().HasData(
                new Artista { Id = 1, Nombre = "Bad Bunny" },
                new Artista { Id = 2, Nombre = "Taylor Swift" },
                new Artista { Id = 3, Nombre = "Drake" },
                new Artista { Id = 4, Nombre = "Shakira" },
                new Artista { Id = 5, Nombre = "The Weeknd" },
                new Artista { Id = 6, Nombre = "J Balvin" },
                new Artista { Id = 7, Nombre = "Ariana Grande" },
                new Artista { Id = 8, Nombre = "Daddy Yankee" },
                new Artista { Id = 9, Nombre = "Ozuna" },
                new Artista { Id = 10, Nombre = "Billie Eilish" }
            );
        }
    }
}
