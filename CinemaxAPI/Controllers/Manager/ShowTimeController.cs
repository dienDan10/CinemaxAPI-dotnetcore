using AutoMapper;
using CinemaxAPI.CustomActionFilters;
using CinemaxAPI.Models.Domain;
using CinemaxAPI.Models.DTO;
using CinemaxAPI.Models.DTO.Requests;
using CinemaxAPI.Models.DTO.Responses;
using CinemaxAPI.Repositories;
using CinemaxAPI.Utils;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace CinemaxAPI.Controllers.Manager
{
    [Route("api/showtimes")]
    [ApiController]
    //[Authorize(Roles = Constants.Role_Manager)]
    public class ShowTimeController : ControllerBase
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;

        public ShowTimeController(IUnitOfWork unitOfWork, IMapper mapper)
        {
            _unitOfWork = unitOfWork;
            _mapper = mapper;
        }

        [HttpGet]
        public async Task<IActionResult> GetAllShowTimes()
        {
            var showTimes = await _unitOfWork.ShowTime.GetAllAsync();
            return Ok(new SuccessResponseDTO
            {
                Data = _mapper.Map<List<ShowTimeDTO>>(showTimes),
                Message = "ShowTimes retrieved successfully."
            });
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetShowTimeById(int id)
        {
            var showTime = await _unitOfWork.ShowTime.GetOneAsync(s => s.Id == id);
            if (showTime == null)
            {
                return NotFound(new ErrorResponseDTO
                {
                    Message = "ShowTime not found.",
                    StatusCode = 404
                });
            }
            return Ok(new SuccessResponseDTO
            {
                Data = _mapper.Map<ShowTimeDTO>(showTime),
                Message = "ShowTime retrieved successfully."
            });
        }

        [HttpPost]
        [ValidateModel]
        public async Task<IActionResult> CreateShowTime([FromBody] CreateShowTimeRequestDTO request)
        {
            // Get all showtimes with the same movie, screen, and date
            var sameDayShowTimes = (await _unitOfWork.ShowTime.GetAllAsync(
                s => s.MovieId == request.MovieId && s.ScreenId == request.ScreenId && s.Date.Date == request.Date.Date
            )).OrderBy(s => s.StartTime).ToList();

            // Check time constraints
            foreach (var st in sameDayShowTimes)
            {
                // If the new showtime starts before the current one
                if (request.StartTime < st.StartTime)
                {
                    // The new showtime's end time must be at least 10 minutes before the next one's start time
                    if (request.EndTime > st.StartTime.Add(TimeSpan.FromMinutes(-10)))
                    {
                        return BadRequest(new ErrorResponseDTO
                        {
                            Message = "The end time of the new showtime must be at least 10 minutes earlier than the start time of the next showtime.",
                            StatusCode = 400
                        });
                    }
                }
                // If the new showtime starts after or at the same time as the current one
                else
                {
                    // The new showtime's start time must be at least 10 minutes after the previous one's end time
                    if (request.StartTime < st.EndTime.Add(TimeSpan.FromMinutes(10)))
                    {
                        return BadRequest(new ErrorResponseDTO
                        {
                            Message = "The start time of the new showtime must be at least 10 minutes after the end time of the previous showtime.",
                            StatusCode = 400
                        });
                    }
                }
            }

            var showTime = _mapper.Map<ShowTime>(request);
            showTime.CreatedAt = DateTime.Now;
            showTime.LastUpdatedAt = DateTime.Now;
            showTime.IsActive = true;
            await _unitOfWork.ShowTime.AddAsync(showTime);
            await _unitOfWork.SaveAsync();
            return Ok(new SuccessResponseDTO
            {
                Data = _mapper.Map<ShowTimeDTO>(showTime),
                Message = "ShowTime created successfully."
            });
        }

        [HttpPut("{id}")]
        [ValidateModel]
        public async Task<IActionResult> UpdateShowTime(int id, [FromBody] UpdateShowTimeRequestDTO request)
        {
            var showTime = await _unitOfWork.ShowTime.GetOneAsync(s => s.Id == id);
            if (showTime == null)
            {
                return NotFound(new ErrorResponseDTO
                {
                    Message = "ShowTime not found.",
                    StatusCode = 404
                });
            }
            _mapper.Map(request, showTime);
            showTime.LastUpdatedAt = DateTime.Now;
            _unitOfWork.ShowTime.Update(showTime);
            await _unitOfWork.SaveAsync();
            return Ok(new SuccessResponseDTO
            {
                Data = _mapper.Map<ShowTimeDTO>(showTime),
                Message = "ShowTime updated successfully."
            });
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteShowTime(int id)
        {
            var showTime = await _unitOfWork.ShowTime.GetOneAsync(s => s.Id == id);
            if (showTime == null)
            {
                return NotFound(new ErrorResponseDTO
                {
                    Message = "ShowTime not found.",
                    StatusCode = 404
                });
            }
            var bookings = await _unitOfWork.Booking.GetAllAsync(b => b.ShowTimeId == id);
            if (bookings.Any())
            {
                return BadRequest(new ErrorResponseDTO
                {
                    Message = "Cannot delete ShowTime with associated bookings.",
                    StatusCode = 400
                });
            }
            _unitOfWork.ShowTime.Remove(showTime);
            await _unitOfWork.SaveAsync();
            return Ok(new SuccessResponseDTO
            {
                Message = "ShowTime deleted successfully."
            });
        }
    }
}
