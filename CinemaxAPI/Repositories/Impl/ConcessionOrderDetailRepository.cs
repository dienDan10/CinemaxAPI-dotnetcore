using CinemaxAPI.Data;
using CinemaxAPI.Models.Domain;
using CinemaxAPI.Repositories;

namespace CinemaxAPI.Repositories.Impl
{
    public class ConcessionOrderDetailRepository : Repository<ConcessionOrderDetail>, IConcessionOrderDetailRepository
    {
        public ConcessionOrderDetailRepository(CinemaxServerDbContext context) : base(context)
        {

        }
    }
}
