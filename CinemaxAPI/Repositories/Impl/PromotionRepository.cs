using CinemaxAPI.Data;
using CinemaxAPI.Models.Domain;

namespace CinemaxAPI.Repositories.Impl
{
    public class PromotionRepository : Repository<Promotion>, IPromotionRepository
    {
        public PromotionRepository(CinemaxServerDbContext context) : base(context)
        {
        }

    }
}
