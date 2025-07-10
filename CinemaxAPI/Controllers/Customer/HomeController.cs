using AutoMapper;
using CinemaxAPI.Models.Domain;
using CinemaxAPI.Models.DTO;
using CinemaxAPI.Models.DTO.Responses;
using CinemaxAPI.Repositories;
using Microsoft.AspNetCore.Mvc;

namespace CinemaxAPI.Controllers.Customer
{
    [Route("api/home")]
    [ApiController]
    public class HomeController : ControllerBase
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;

        public HomeController(IUnitOfWork unitOfWork, IMapper mapper)
        {
            _unitOfWork = unitOfWork;
            _mapper = mapper;
        }

        [HttpGet("movies/airing")]
        public async Task<IActionResult> GetAiringMovies()
        {
            var movies = await _unitOfWork.ShowTime.GetCurrentlyAiringMovies();

            if (movies == null || !movies.Any())
            {
                return Ok(new
                {
                    Message = "No movies are currently airing.",
                    Data = new List<object>()
                });
            }

            var airingMovies = _mapper.Map<List<MovieDTO>>(movies);

            return Ok(new
            {
                Data = airingMovies,
                Message = "Airing movies retrieved successfully."
            });
        }

        [HttpGet("movies/upcoming")]
        public async Task<IActionResult> GetUpcomingMovies()
        {
            var movies = await _unitOfWork.ShowTime.GetUpcomingMovies();

            if (movies == null || !movies.Any())
            {
                return Ok(new
                {
                    Message = "No upcoming movies found.",
                    Data = new List<object>()
                });
            }

            var upcomingMovies = _mapper.Map<List<MovieDTO>>(movies);

            return Ok(new
            {
                Data = upcomingMovies,
                Message = "Upcoming movies retrieved successfully."
            });
        }

        [HttpGet("movies/{id}/showtimes")]
        public async Task<IActionResult> GetShowtimesByMovieId(int id, [FromQuery] int provinceId, [FromQuery] string date)
        {
            var showtimes = await _unitOfWork.ShowTime.GetAllAsync(
                                s => s.MovieId == id
                                && s.Screen.Theater.ProvinceId == provinceId
                                && s.Date == DateTime.Parse(date), includeProperties: "Screen.Theater");

            if (showtimes == null || !showtimes.Any())
            {
                return Ok(new SuccessResponseDTO
                {
                    Message = "No showtimes found for the specified movie and date.",
                    Data = new List<ShowTime>()
                });
            }

            // group showtimes by theater
            var groupedShowtimes = showtimes.GroupBy(s => s.Screen.TheaterId)
                                             .Select(g => new
                                             {
                                                 TheaterId = g.Key,
                                                 TheaterName = g.First().Screen.Theater.Name,
                                                 TheaterAddress = g.First().Screen.Theater.Address,
                                                 ShowTimes = g.Select(s => new
                                                 {
                                                     s.Id,
                                                     s.StartTime,
                                                     s.TicketPrice
                                                 }).ToList()
                                             })
                                             .ToList();


            return Ok(new SuccessResponseDTO
            {
                Data = groupedShowtimes,
                Message = "Showtimes retrieved successfully."
            });
        }

    }
}
