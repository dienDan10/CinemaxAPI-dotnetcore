using CinemaxAPI.Data;
using CinemaxAPI.Models.Domain;
using CinemaxAPI.Repositories.Impl;

namespace CinemaxAPI.Repositories
{
    public class ConcessionRepository : Repository<Concession>, IConcessionRepository
    {

        public ConcessionRepository(CinemaxServerDbContext context) : base(context)
        {
        }


    }
}
