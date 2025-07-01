using CinemaxAPI.Data;
using CinemaxAPI.Models.Domain;
using Microsoft.EntityFrameworkCore;

namespace CinemaxAPI.Repositories.Impl
{
    public class TheaterRepository : Repository<Theater>, ITheaterRepository
    {
        public TheaterRepository(CinemaxServerDbContext context) : base(context)
        {

        }

        public async Task<int> CountByProvinceId(int provinceId)
        {
            return await _context.Theaters
                .Where(t => t.ProvinceId == provinceId)
                .CountAsync();
        }

        public int CountTotal()
        {
            return _context.Theaters.Count();
        }
    }
}
