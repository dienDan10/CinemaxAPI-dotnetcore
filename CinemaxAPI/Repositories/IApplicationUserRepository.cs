using CinemaxAPI.Models.Domain;

namespace CinemaxAPI.Repositories
{
    public interface IApplicationUserRepository : IRepository<ApplicationUser>
    {
        Task<List<ApplicationUser>> GetAllCustomers();
        Task<List<ApplicationUser>> GetAllEmployees(int theaterId);
        Task<List<ApplicationUser>> GetAllManagers();

    }
}
