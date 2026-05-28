using Microsoft.EntityFrameworkCore;
using StreamingAPI.Data;
using StreamingAPI.Models;

namespace StreamingAPI.Repositories
{
    public class ConteudoRepository
    {
        private readonly StreamingContext _context;

        public ConteudoRepository(StreamingContext context)
        {
            _context = context;
        }

        public async Task<List<Conteudo>> GetAllAsync()
        {
            return await _context.Conteudos
                .Include(c => c.Criador)
                .ToListAsync();
        }

        public async Task<Conteudo?> GetByIdAsync(int id)
        {
            return await _context.Conteudos
                .Include(c => c.Criador)
                .FirstOrDefaultAsync(c => c.Id == id);
        }

        public async Task<Conteudo> CreateAsync(Conteudo conteudo)
        {
            _context.Conteudos.Add(conteudo);
            await _context.SaveChangesAsync();
            return conteudo;
        }

        public async Task UpdateAsync(Conteudo conteudo)
        {
            _context.Entry(conteudo).State = EntityState.Modified;
            await _context.SaveChangesAsync();
        }

        public async Task<bool> DeleteAsync(int id)
        {
            var conteudo = await _context.Conteudos.FindAsync(id);
            if (conteudo == null)
                return false;

            _context.Conteudos.Remove(conteudo);
            await _context.SaveChangesAsync();
            return true;
        }
    }
}