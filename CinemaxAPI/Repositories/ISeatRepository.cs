using CinemaxAPI.Models.Domain;

namespace CinemaxAPI.Repositories
{
    public interface ISeatRepository : IRepository<Seat>
    {

        Task<IEnumerable<Seat>> GetBookedSeatsByShowtimeId(int showtimeId);

    }
}
