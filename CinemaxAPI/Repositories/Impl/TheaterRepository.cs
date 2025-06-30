using CinemaxAPI.Data;
using CinemaxAPI.Models.Domain;
using CinemaxAPI.Repositories;
using Microsoft.EntityFrameworkCore;

namespace CinemaxAPI.Repositories.Impl
{
    public class TheaterRepository : Repository<Theater>, ITheaterRepository
    {
        public TheaterRepository(CinemaxServerDbContext context) : base(context)
        {

        }

        public int CountTotal()
        {
            return _context.Theaters.Count();
        }
    }
}
