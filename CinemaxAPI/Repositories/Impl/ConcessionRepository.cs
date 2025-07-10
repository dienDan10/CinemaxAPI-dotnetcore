using CinemaxAPI.Data;
using CinemaxAPI.Models.Domain;
using Microsoft.EntityFrameworkCore;

namespace CinemaxAPI.Repositories
{
    public class ConcessionRepository : IConcessionRepository
    {
        private readonly CinemaxServerDbContext _context;

        public ConcessionRepository(CinemaxServerDbContext context)
        {
            _context = context;
        }

        public async Task<Concession> CreateAsync(Concession concession)
        {
            await _context.Concessions.AddAsync(concession);
            await _context.SaveChangesAsync();
            return concession;
        }

        public async Task<List<Concession>> GetAllAsync()
        {
            return await _context.Concessions.ToListAsync();
        }

        public async Task<Concession?> GetByIdAsync(int id)
        {
            return await _context.Concessions.FindAsync(id);
        }

        public async Task<Concession?> UpdateAsync(int id, Concession updated)
        {
            var existing = await _context.Concessions.FindAsync(id);
            if (existing == null) return null;

            existing.Name = updated.Name;
            existing.Description = updated.Description;
            existing.Price = updated.Price;
            existing.ImageUrl = updated.ImageUrl;
            existing.LastUpdatedAt = DateTime.Now;

            await _context.SaveChangesAsync();
            return existing;
        }

        public async Task<Concession?> DeleteAsync(int id)
        {
            var concession = await _context.Concessions.FindAsync(id);
            if (concession == null) return null;

            _context.Concessions.Remove(concession);
            await _context.SaveChangesAsync();
            return concession;
        }
    }
}
