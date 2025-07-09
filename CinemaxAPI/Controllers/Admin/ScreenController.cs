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

namespace CinemaxAPI.Controllers.Admin
{
    [Route("api/screens")]
    [ApiController]
    public class ScreenController : ControllerBase
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;

        public ScreenController(IUnitOfWork unitOfWork, IMapper mapper)
        {
            _unitOfWork = unitOfWork;
            _mapper = mapper;
        }

        [HttpGet]
        [Authorize(Roles = $"{Constants.Role_Manager},{Constants.Role_Admin}")]
        public async Task<IActionResult> GetScreensByTheaterId([FromQuery] int theaterId)
        {
            List<Screen> screens;
            if (theaterId <= 0)
            {
                screens = (await _unitOfWork.Screen.GetAllAsync(includeProperties: "Theater")).ToList();
                return Ok(new SuccessResponseDTO
                {
                    Data = _mapper.Map<List<ScreenDTO>>(screens),
                    Message = "All screens retrieved successfully."
                });
            }

            screens = (await _unitOfWork.Screen.GetAllAsync(s => s.TheaterId == theaterId, includeProperties: "Theater")).ToList();

            if (screens == null || !screens.Any())
            {
                return Ok(new SuccessResponseDTO
                {
                    Data = new List<ScreenDTO>(),
                    Message = "No screens found for the specified theater."
                });
            }

            return Ok(new SuccessResponseDTO
            {
                Data = _mapper.Map<List<ScreenDTO>>(screens),
                Message = "Screens retrieved successfully."
            });
        }

        [HttpPost]
        [ValidateModel]
        [Authorize(Roles = Constants.Role_Admin)]
        public async Task<IActionResult> CreateScreen([FromBody] CreateScreenRequestDTO request)
        {

            var screen = new Screen
            {
                Name = request.Name,
                TheaterId = request.TheaterId,
                Rows = request.Rows,
                Columns = request.Columns,
                IsActive = true,
                CreatedAt = DateTime.Now,
                LastUpdatedAt = DateTime.Now
            };
            await _unitOfWork.Screen.AddAsync(screen);
            await _unitOfWork.SaveAsync();

            // generate seats
            await GenerateSeats(screen.Id, request.Rows, request.Columns);

            return Ok(new SuccessResponseDTO
            {
                Data = screen.Id,
                Message = "Screen created successfully."
            });
        }

        private async Task GenerateSeats(int screenId, int rows, int columns)
        {
            List<Seat> seats = new();
            char rowLetter = 'A';

            for (int r = 0; r < rows; r++)
            {
                for (int c = 1; c <= columns; c++)
                {
                    seats.Add(new Seat
                    {
                        SeatRow = rowLetter.ToString(),
                        SeatNumber = c,
                        ScreenId = screenId,
                        CreatedAt = DateTime.Now,
                        LastUpdatedAt = DateTime.Now
                    });
                }
                rowLetter++;
            }

            await _unitOfWork.Seat.AddRangeAsync(seats);
            await _unitOfWork.SaveAsync();
        }

        [HttpPut("{id}")]
        [ValidateModel]
        [Authorize(Roles = $"{Constants.Role_Manager},{Constants.Role_Admin}")]
        public async Task<IActionResult> UpdateScreen(int id, [FromBody] UpdateScreenRequestDTO request)
        {
            var screen = await _unitOfWork.Screen.GetOneAsync(s => s.Id == id);
            if (screen == null)
            {
                return NotFound(new ErrorResponseDTO
                {
                    Message = "Screen not found.",
                    StatusCode = 404,
                });
            }

            screen.Name = request.Name;
            screen.LastUpdatedAt = DateTime.Now;

            _unitOfWork.Screen.Update(screen);
            await _unitOfWork.SaveAsync();

            return Ok(new SuccessResponseDTO
            {
                Data = screen.Id,
                Message = "Screen updated successfully."
            });
        }

        [HttpPut("{id}/enable")]
        [Authorize(Roles = $"{Constants.Role_Manager},{Constants.Role_Admin}")]
        public async Task<IActionResult> EnableScreen(int id)
        {
            var screen = await _unitOfWork.Screen.GetOneAsync(s => s.Id == id);
            if (screen == null)
            {
                return NotFound(new ErrorResponseDTO
                {
                    Message = "Screen not found.",
                    StatusCode = 404,
                });
            }

            screen.IsActive = true;
            screen.LastUpdatedAt = DateTime.Now;

            _unitOfWork.Screen.Update(screen);
            await _unitOfWork.SaveAsync();

            return Ok(new SuccessResponseDTO
            {
                Data = screen.Id,
                Message = "Screen enabled successfully."
            });
        }

        [HttpPut("{id}/disable")]
        [Authorize(Roles = $"{Constants.Role_Manager},{Constants.Role_Admin}")]
        public async Task<IActionResult> DisableScreen(int id)
        {
            var screen = await _unitOfWork.Screen.GetOneAsync(s => s.Id == id);
            if (screen == null)
            {
                return NotFound(new ErrorResponseDTO
                {
                    Message = "Screen not found.",
                    StatusCode = 404,
                });
            }

            screen.IsActive = false;
            screen.LastUpdatedAt = DateTime.Now;

            _unitOfWork.Screen.Update(screen);
            await _unitOfWork.SaveAsync();

            return Ok(new SuccessResponseDTO
            {
                Data = screen.Id,
                Message = "Screen disabled successfully."
            });
        }

    }
}
