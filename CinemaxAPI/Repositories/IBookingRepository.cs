using CinemaxAPI.Models.Domain;

namespace CinemaxAPI.Repositories
{
    public interface IBookingRepository : IRepository<Booking>
    {
        Task<IEnumerable<int>> GetAllBookingSeatIds(int showtimeId);
    }
}
