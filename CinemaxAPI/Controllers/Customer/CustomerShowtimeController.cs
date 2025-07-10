using CinemaxAPI.Models.Domain;
using CinemaxAPI.Models.DTO.Requests;
using CinemaxAPI.Models.DTO.Responses;
using CinemaxAPI.Repositories;
using Microsoft.AspNetCore.Mvc;

namespace CinemaxAPI.Controllers.Customer
{
    [Route("api/showtimes")]
    [ApiController]
    public class CustomerShowtimeController : ControllerBase
    {
        private readonly IUnitOfWork _unitOfWork;

        public CustomerShowtimeController(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        [HttpGet("search")]
        public async Task<IActionResult> SearchShowtimesByMovie([FromQuery] FindShowtimeByMovieRequestDTO request)
        {
            var showtimes = await _unitOfWork.ShowTime.GetAllAsync(
                               s => s.MovieId == request.MovieId
                               && s.Screen.Theater.ProvinceId == request.ProvinceId
                               && s.Date == DateTime.Parse(request.Date), includeProperties: "Screen.Theater");

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

        [HttpGet("{id}/booking")]
        public async Task<IActionResult> GetBookingDetail(int id)
        {
            var showtime = await _unitOfWork.ShowTime.GetOneAsync(s => s.Id == id, includeProperties: "Movie,Screen");

            if (showtime == null)
            {
                return NotFound(new ErrorResponseDTO
                {
                    Message = "Showtime not found.",
                    StatusCode = 404
                });
            }

            showtime.Screen.Theater = await _unitOfWork.Theater.GetOneAsync(t => t.Id == showtime.Screen.TheaterId);

            // get all seats for this screen
            var seats = await _unitOfWork.Seat.GetAllAsync(s => s.ScreenId == showtime.ScreenId && s.IsRemoved == false);

            // get all booked seats for this showtime
            var bookedSeatIds = await _unitOfWork.Booking.GetAllBookingSeatIds(showtime.Id);

            // create a response object
            var response = new
            {
                showtime.Id,
                Movie = new
                {
                    showtime.Movie.Id,
                    showtime.Movie.Title
                },
                Screen = new
                {
                    showtime.Screen.Id,
                    showtime.Screen.Name,
                },
                Theater = new
                {
                    showtime.Screen.Theater.Id,
                    showtime.Screen.Theater.Name,
                    showtime.Screen.Theater.Address
                },
                Date = showtime.Date.ToString("yyyy-MM-dd"),
                StartTime = showtime.StartTime.ToString(@"hh\:mm"),
                EndTime = showtime.EndTime.ToString(@"hh\:mm"),
                showtime.TicketPrice,
                Seats = seats.Select(s => new
                {
                    s.Id,
                    s.SeatRow,
                    s.SeatNumber,
                    IsBooked = bookedSeatIds.Contains(s.Id),
                    s.IsActive
                }).ToList()
            };

            return Ok(new SuccessResponseDTO
            {
                Data = response,
                Message = "Booking details retrieved successfully."
            });
        }
    }
}
