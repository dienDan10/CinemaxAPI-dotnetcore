using CinemaxAPI.Data;
using CinemaxAPI.Models.Domain;
using CinemaxAPI.Repositories;

namespace CinemaxAPI.Repositories.Impl
{
    public class SeatRepository : Repository<Seat>, ISeatRepository
    {
        public SeatRepository(CinemaxServerDbContext context) : base(context)
        {

        }

    }
}
