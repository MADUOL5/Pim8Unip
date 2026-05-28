using Microsoft.EntityFrameworkCore;
using StreamingAPI.Data;
using StreamingAPI.Models;

namespace StreamingAPI.Repositories
{
    public class UsuarioRepository
    {
        private readonly StreamingContext _context;

        public UsuarioRepository(StreamingContext context)
        {
            _context = context;
        }

        public async Task<List<Usuario>> GetAllAsync()
        {
            return await _context.Usuarios.ToListAsync();
        }

        public async Task<Usuario?> GetByIdAsync(int id)
        {
            return await _context.Usuarios.FindAsync(id);
        }

        public async Task<Usuario?> GetByEmailAsync(string email)
        {
            var emailNormalizado = email.Trim().ToLower();
            return await _context.Usuarios
                .FirstOrDefaultAsync(u => u.Email.ToLower() == emailNormalizado);
        }

        public async Task<bool> EmailExistsAsync(string email, int? usuarioIdIgnorado = null)
        {
            var emailNormalizado = email.Trim().ToLower();
            return await _context.Usuarios
                .AnyAsync(u => u.Email.ToLower() == emailNormalizado
                    && (!usuarioIdIgnorado.HasValue || u.Id != usuarioIdIgnorado.Value));
        }

        public async Task<Usuario> CreateAsync(Usuario usuario)
        {
            _context.Usuarios.Add(usuario);
            await _context.SaveChangesAsync();
            return usuario;
        }

        public async Task UpdateAsync(Usuario usuario)
        {
            _context.Usuarios.Update(usuario);
            await _context.SaveChangesAsync();
        }

        public async Task<bool> DeleteAsync(int id)
        {
            var usuario = await _context.Usuarios.FindAsync(id);
            if (usuario == null)
                return false;

            _context.Usuarios.Remove(usuario);
            await _context.SaveChangesAsync();
            return true;
        }
    }
}
