using Microsoft.EntityFrameworkCore;
using StreamingAPI.Models;

namespace StreamingAPI.Data
{
    public class StreamingContext : DbContext
    {
        public StreamingContext(DbContextOptions<StreamingContext> options) : base(options)
        {
        }

        public DbSet<Usuario> Usuarios { get; set; }
        public DbSet<Playlist> Playlists { get; set; }
        public DbSet<Conteudo> Conteudos { get; set; }
        public DbSet<Criador> Criadores { get; set; }
        public DbSet<ItemPlaylist> ItemsPlaylist { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // Configuração de relacionamentos
            modelBuilder.Entity<Playlist>()
                .HasOne(p => p.Usuario)
                .WithMany(u => u.Playlists)
                .HasForeignKey(p => p.UsuarioId);

            modelBuilder.Entity<Conteudo>()
                .HasOne(c => c.Criador)
                .WithMany(cr => cr.Conteudos)
                .HasForeignKey(c => c.CriadorId);

            modelBuilder.Entity<ItemPlaylist>()
                .HasOne(ip => ip.Playlist)
                .WithMany(p => p.Items)
                .HasForeignKey(ip => ip.PlaylistId);

            modelBuilder.Entity<ItemPlaylist>()
                .HasOne(ip => ip.Conteudo)
                .WithMany(c => c.ItemsPlaylist)
                .HasForeignKey(ip => ip.ConteudoId);

            // Índices únicos
            modelBuilder.Entity<Usuario>()
                .HasIndex(u => u.Email)
                .IsUnique();
        }
    }
}
