using CinemaxAPI.Models.Domain;

namespace CinemaxAPI.Repositories
{
    public interface ITheaterRepository : IRepository<Theater>
    {
        int CountTotal();
        Task<int> CountByProvinceId(int provinceId);
    }
}
