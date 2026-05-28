using Microsoft.EntityFrameworkCore;
using StreamingAPI.Data;
using StreamingAPI.Models;

namespace StreamingAPI.Repositories
{
    public class CriadorRepository
    {
        private readonly StreamingContext _context;

        public CriadorRepository(StreamingContext context)
        {
            _context = context;
        }

        public async Task<List<Criador>> GetAllAsync()
        {
            return await _context.Criadores.ToListAsync();
        }

        public async Task<Criador?> GetByIdAsync(int id)
        {
            return await _context.Criadores.FindAsync(id);
        }

        public async Task<Criador> CreateAsync(Criador criador)
        {
            _context.Criadores.Add(criador);
            await _context.SaveChangesAsync();
            return criador;
        }

        public async Task UpdateAsync(Criador criador)
        {
            _context.Entry(criador).State = EntityState.Modified;
            await _context.SaveChangesAsync();
        }

        public async Task<bool> DeleteAsync(int id)
        {
            var criador = await _context.Criadores.FindAsync(id);
            if (criador == null)
                return false;

            _context.Criadores.Remove(criador);
            await _context.SaveChangesAsync();
            return true;
        }
    }
}