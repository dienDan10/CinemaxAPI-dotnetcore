using CinemaxAPI.Data;
using CinemaxAPI.Models.Domain;
using CinemaxAPI.Models.DTO.Requests;
using Microsoft.EntityFrameworkCore;

namespace CinemaxAPI.Repositories.Impl
{
    public class MovieRepository : Repository<Movie>, IMovieRepository
    {
        public MovieRepository(CinemaxServerDbContext context) : base(context)
        {

        }

        public async Task<(IEnumerable<Movie> movies, int totalCount)> GetAllAsync(MovieFilterRequestDTO? filter = null, PagedRequestDTO? paged = null)
        {
            var query = dbSet.AsQueryable();

            // FILTERING
            if (filter != null)
            {
                if (!string.IsNullOrEmpty(filter.Title))
                {
                    query = query.Where(m => m.Title.ToLower().Contains(filter.Title.ToLower()));
                }
            }

            int totalCount = await query.CountAsync();

            // PAGING
            if (paged != null)
            {
                var pageSize = paged.PageSize > 0 ? paged.PageSize : 10; // Default page size
                var pageNumber = paged.PageNumber > 0 ? paged.PageNumber : 1; // Default page number

                query = query.Skip((pageNumber - 1) * pageSize).Take(pageSize);
            }

            var movies = await query.ToListAsync();

            return (movies, totalCount);
        }
    }
}
