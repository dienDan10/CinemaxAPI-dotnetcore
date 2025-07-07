using CinemaxAPI.Models.Domain;
using Microsoft.EntityFrameworkCore;

namespace CinemaxAPI.Data
{
    public static class MovieSeeder
    {
        public static void SeedMovies(CinemaxServerDbContext dbContext)
        {
            if (!dbContext.Movies.Any(m => m.Title == "Inception"))
            {
                dbContext.Movies.Add(new Movie
                {
                    Title = "Inception",
                    Genre = "Sci-Fi",
                    Director = "Christopher Nolan",
                    Cast = "Leonardo DiCaprio",
                    Description = "A mind-bending thriller.",
                    Duration = 148,
                    ReleaseDate = new DateOnly(2010, 7, 16),
                    PosterUrl = null,
                    TrailerUrl = null
                });
            }
            if (!dbContext.Movies.Any(m => m.Title == "Interstellar"))
            {
                dbContext.Movies.Add(new Movie
                {
                    Title = "Interstellar",
                    Genre = "Sci-Fi",
                    Director = "Christopher Nolan",
                    Cast = "Matthew McConaughey",
                    Description = "Journey beyond the stars.",
                    Duration = 169,
                    ReleaseDate = new DateOnly(2014, 11, 7),
                    PosterUrl = null,
                    TrailerUrl = null
                });
            }

            dbContext.SaveChanges();
        }
    }
}
