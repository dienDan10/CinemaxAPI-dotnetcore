using CinemaxAPI.Data;
using CinemaxAPI.Models.Domain;

namespace CinemaxAPI.Repositories.Impl
{
    public class ApplicationUserRepository : Repository<ApplicationUser>, IApplicationUserRepository
    {
        public ApplicationUserRepository(CinemaxServerDbContext context) : base(context)
        {
        }


    }
}
