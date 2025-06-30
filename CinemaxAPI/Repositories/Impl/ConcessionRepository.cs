using CinemaxAPI.Data;
using CinemaxAPI.Models.Domain;
using CinemaxAPI.Repositories;

namespace CinemaxAPI.Repositories.Impl
{
    public class ConcessionRepository : Repository<Concession>, IConcessionRepository
    {
        public ConcessionRepository(CinemaxServerDbContext context) : base(context)
        {

        }
    }
}
