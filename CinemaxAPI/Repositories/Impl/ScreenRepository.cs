using CinemaxAPI.Data;
using CinemaxAPI.Models.Domain;

namespace CinemaxAPI.Repositories.Impl
{
    public class ScreenRepository : Repository<Screen>, IScreenRepository
    {
        public ScreenRepository(CinemaxServerDbContext context) : base(context)
        {

        }


    }
}
