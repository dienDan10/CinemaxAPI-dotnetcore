using CinemaxAPI.Models.Domain;

namespace CinemaxAPI.Repositories
{
    public interface IConcessionRepository
    {
        Task<List<Concession>> GetAllAsync();
        Task<Concession?> GetByIdAsync(int id);
        Task<Concession> CreateAsync(Concession concession);
        Task<Concession?> UpdateAsync(int id, Concession concession);
        Task<Concession?> DeleteAsync(int id);
    }
}
