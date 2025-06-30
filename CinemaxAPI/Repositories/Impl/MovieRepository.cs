using CinemaxAPI.Data;
using CinemaxAPI.Models.Domain;
using CinemaxAPI.Repositories;

namespace CinemaxAPI.Repositories.Impl
{
    public class MovieRepository : Repository<Movie>, IMovieRepository
    {
        public MovieRepository(CinemaxServerDbContext context) : base(context)
        {

        }
    }
}
