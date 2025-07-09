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
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace CinemaxAPI.Controllers.Manager
{
    [Route("api/showtimes")]
    [ApiController]
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
        public async Task<IActionResult> GetAllShowTimes(int? movieId, int? screenId, DateTime? startDate, DateTime? endDate)
        {
            IEnumerable<ShowTime> showTimes;

            if (startDate.HasValue && endDate.HasValue)
            {
                var from = startDate.Value.Date;
                var to = endDate.Value.Date;
                showTimes = await _unitOfWork.ShowTime.GetAllAsync(
                    s =>
                        (!movieId.HasValue || s.MovieId == movieId.Value) &&
                        (!screenId.HasValue || s.ScreenId == screenId.Value) &&
                        s.Date.Date >= from && s.Date.Date <= to
                );
            }
            else
            {
                showTimes = await _unitOfWork.ShowTime.GetAllAsync(
                    s =>
                        (!movieId.HasValue || s.MovieId == movieId.Value) &&
                        (!screenId.HasValue || s.ScreenId == screenId.Value)
                );
            }

            var ordered = showTimes.OrderBy(s => s.Date).ThenBy(s => s.StartTime).ToList();
            return Ok(new SuccessResponseDTO
            {
                Data = _mapper.Map<List<ShowTimeDTO>>(ordered),
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
        //[Authorize(Roles = Constants.Role_Manager)]
        public async Task<IActionResult> CreateShowTime([FromBody] CreateShowTimeRequestDTO request)
        {
            var results = new List<object>();
            var showTimesToAdd = new List<ShowTime>();

            foreach (var showTimeData in request.ShowTimes)
            {
                for (int i = 0; i < showTimeData.StartTimes.Count; i++)
                {
                    // Convert start and end times from string to TimeSpan
                    var startTime = TimeSpan.Parse(showTimeData.StartTimes[i]);
                    var endTime = showTimeData.EndTimes.Count > i ? TimeSpan.Parse(showTimeData.EndTimes[i]) : startTime;
                    // Lấy các suất chiếu cùng screen, cùng ngày
                    var sameDayShowTimes = (await _unitOfWork.ShowTime.GetAllAsync(
                        s => s.ScreenId == request.ScreenId && s.Date.Date == showTimeData.Date.Date
                    )).OrderBy(s => s.StartTime).ToList();


                    var showTime = new ShowTime
                    {
                        MovieId = request.MovieId,
                        ScreenId = request.ScreenId,
                        Date = showTimeData.Date,
                        StartTime = startTime,
                        EndTime = endTime,
                        TicketPrice = request.TicketPrice,
                        CreatedAt = DateTime.Now,
                        LastUpdatedAt = DateTime.Now,
                        IsActive = true
                    };
                    showTimesToAdd.Add(showTime);
                    results.Add(new { Success = true, ShowTime = _mapper.Map<ShowTimeDTO>(showTime) });

                }
            }

            if (showTimesToAdd.Any())
            {
                await _unitOfWork.ShowTime.AddRangeAsync(showTimesToAdd);
                await _unitOfWork.SaveAsync();
            }

            return Ok(new SuccessResponseDTO
            {
                Data = results,
                Message = "ShowTime(s) created."
            });
        }

        [HttpPut("{id}")]
        [ValidateModel]
        [Authorize(Roles = Constants.Role_Manager)]
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

            // Lấy các showtime khác cùng movie, screen, date (trừ chính nó)
            var sameDayShowTimes = (await _unitOfWork.ShowTime.GetAllAsync(
                s => s.Id != id && s.MovieId == request.MovieId && s.ScreenId == request.ScreenId && s.Date.Date == request.Date.Date
            )).OrderBy(s => s.StartTime).ToList();


            // kiểm tra conflict showtime
            foreach (var st in sameDayShowTimes)
            {
                // Nếu hai suất chiếu giao nhau trong khoảng 10 phút thì conflict
                if (request.StartTime < st.EndTime.Add(TimeSpan.FromMinutes(10)) &&
                    request.EndTime > st.StartTime.Add(TimeSpan.FromMinutes(-10)))
                {
                    return BadRequest(new ErrorResponseDTO
                    {
                        Message = "Showtime update conflict with another showtime.",
                        StatusCode = 400
                    });
                }
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
        [Authorize(Roles = Constants.Role_Manager)]
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
