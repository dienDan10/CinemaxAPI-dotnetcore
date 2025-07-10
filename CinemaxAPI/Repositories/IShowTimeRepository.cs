using CinemaxAPI.Models.Domain;
using CinemaxAPI.Models.DTO.Requests;

namespace CinemaxAPI.Repositories
{
    public interface IShowTimeRepository : IRepository<ShowTime>
    {
        Task<IEnumerable<Movie>> GetCurrentlyAiringMovies();
        Task<IEnumerable<Movie>> GetUpcomingMovies();
        Task<IEnumerable<ShowTime>> GetShowtimes(ShowTimesFilterRequestDTO filter);
    }
}
