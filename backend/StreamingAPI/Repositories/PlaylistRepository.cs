using Microsoft.EntityFrameworkCore;
using StreamingAPI.Data;
using StreamingAPI.Models;

namespace StreamingAPI.Repositories
{
    public class PlaylistRepository
    {
        private readonly StreamingContext _context;

        public PlaylistRepository(StreamingContext context)
        {
            _context = context;
        }

        // Create
        public async Task<Playlist> CreateAsync(Playlist playlist)
        {
            _context.Playlists.Add(playlist);
            await _context.SaveChangesAsync();
            return playlist;
        }

        // Read
        public async Task<Playlist?> GetByIdAsync(int id)
        {
            return await _context.Playlists
                .Include(p => p.Usuario)
                .Include(p => p.Items)
                .ThenInclude(ip => ip.Conteudo)
                .FirstOrDefaultAsync(p => p.Id == id);
        }

        public async Task<List<Playlist>> GetAllAsync()
        {
            return await _context.Playlists
                .Include(p => p.Usuario)
                .Include(p => p.Items)
                .ThenInclude(ip => ip.Conteudo)
                .ToListAsync();
        }

        public async Task<List<Playlist>> GetByUsuarioIdAsync(int usuarioId)
        {
            return await _context.Playlists
                .Include(p => p.Items)
                .ThenInclude(ip => ip.Conteudo)
                .Where(p => p.UsuarioId == usuarioId)
                .ToListAsync();
        }

        // Update
        public async Task<Playlist> UpdateAsync(Playlist playlist)
        {
            _context.Playlists.Update(playlist);
            await _context.SaveChangesAsync();
            return playlist;
        }

        // Delete
        public async Task<bool> DeleteAsync(int id)
        {
            var playlist = await _context.Playlists.FindAsync(id);
            if (playlist == null)
                return false;

            _context.Playlists.Remove(playlist);
            await _context.SaveChangesAsync();
            return true;
        }

        // Operações com Items
        public async Task<bool> AddItemAsync(int playlistId, int conteudoId)
        {
            var playlistExiste = await _context.Playlists.AnyAsync(p => p.Id == playlistId);
            if (!playlistExiste)
            {
                return false; // Playlist não encontrada
            }

            var conteudoExiste = await _context.Conteudos.AnyAsync(c => c.Id == conteudoId);
            if (!conteudoExiste)
            {
                return false; // Conteúdo não encontrado
            }
            var item = new ItemPlaylist { PlaylistId = playlistId, ConteudoId = conteudoId };
            _context.ItemsPlaylist.Add(item);
            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<bool> RemoveItemAsync(int itemId)
        {
            var item = await _context.ItemsPlaylist.FindAsync(itemId);
            if (item != null)
            {
                _context.ItemsPlaylist.Remove(item);
                await _context.SaveChangesAsync();
                return true;
            }
            return false;
        }
    }
}
