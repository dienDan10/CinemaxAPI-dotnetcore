using AutoMapper;
using CinemaxAPI.Models.Domain;
using CinemaxAPI.Models.DTO;
using CinemaxAPI.Models.DTO.Responses;
using CinemaxAPI.Repositories;
using Microsoft.AspNetCore.Mvc;

namespace CinemaxAPI.Controllers.Customer
{
    [Route("api/bookings")]
    [ApiController]
    public class BookingController : ControllerBase
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;

        public BookingController(IUnitOfWork unitOfWork, IMapper mapper)
        {
            _unitOfWork = unitOfWork;
            _mapper = mapper;
        }

        [HttpGet("showtimes/{id}")]
        public async Task<IActionResult> GetShowtimeDetail(int id)
        {
            // get showtimes
            var showtime = await _unitOfWork.ShowTime.GetOneAsync(st => st.Id == id && st.Date >= DateTime.Now.Date, includeProperties: "Screen,Movie,Screen.Theater");

            if (showtime == null)
            {
                return NotFound(new ErrorResponseDTO
                {
                    Message = "Showtime not found.",
                    StatusCode = 404
                });
            }

            // get all active concessions
            var concessions = await _unitOfWork.Concession.GetAllAsync(c => c.IsActive && !c.IsRemoved);

            // get the all active seats for the showtime
            var seats = await _unitOfWork.Seat.GetAllAsync(s => s.ScreenId == showtime.ScreenId && s.IsActive && !s.IsRemoved);

            // get all booked seats for this movie
            var bookedSeats = await _unitOfWork.Seat.GetBookedSeatsByShowtimeId(showtime.Id);

            var response = new
            {
                ShowTime = _mapper.Map<ShowTimeDTO>(showtime),
                Movie = _mapper.Map<MovieDTO>(showtime.Movie),
                Theater = _mapper.Map<TheaterDTO>(showtime.Screen.Theater),
                Screen = _mapper.Map<ScreenDTO>(showtime.Screen),
                Concessions = _mapper.Map<List<ConcessionDTO>>(concessions),
                Seats = CreateShowtimeSeat(seats, bookedSeats)
            };

            return Ok(new SuccessResponseDTO
            {
                Data = response,
                Message = "Showtime details retrieved successfully.",
            });
        }

        private List<ShowtimeSeatDTO> CreateShowtimeSeat(IEnumerable<Seat> seats, IEnumerable<Seat> bookedSeats)
        {
            var showtimeSeats = new List<ShowtimeSeatDTO>();
            foreach (var seat in seats)
            {
                var isBooked = bookedSeats.Any(bs => bs.Id == seat.Id);
                showtimeSeats.Add(new ShowtimeSeatDTO
                {
                    Id = seat.Id,
                    SeatRow = seat.SeatRow,
                    SeatNumber = seat.SeatNumber,
                    IsBooked = isBooked
                });
            }

            return showtimeSeats;
        }
    }
}
