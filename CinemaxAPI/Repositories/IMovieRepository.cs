using CinemaxAPI.Models.Domain;
using CinemaxAPI.Models.DTO.Requests;

namespace CinemaxAPI.Repositories
{
    public interface IMovieRepository : IRepository<Movie>
    {
        Task<(IEnumerable<Movie> movies, int totalCount)> GetAllAsync(MovieFilterRequestDTO? filter = null, PagedRequestDTO? paged = null);
    }
}
