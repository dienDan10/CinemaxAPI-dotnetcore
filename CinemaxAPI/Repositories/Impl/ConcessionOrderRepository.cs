using CinemaxAPI.Data;
using CinemaxAPI.Models.Domain;
using CinemaxAPI.Repositories;

namespace CinemaxAPI.Repositories.Impl
{
    public class ConcessionOrderRepository : Repository<ConcessionOrder>, IConcessionOrderRepository
    {
        public ConcessionOrderRepository(CinemaxServerDbContext context) : base(context)
        {

        }
    }
}
