using CinemaxAPI.Models.Domain;

namespace CinemaxAPI.Repositories
{
    public interface IBookingRepository : IRepository<Booking>
    {
        IEnumerable<int> GetAllBookingSeatIds(int showtimeId);
    }
}
