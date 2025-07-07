using AutoMapper;
using CinemaxAPI.Models.Domain;
using CinemaxAPI.Models.DTO;
using CinemaxAPI.Models.DTO.Responses;
using CinemaxAPI.Repositories;
using CinemaxAPI.Utils;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace CinemaxAPI.Controllers.Admin
{
    [Route("api/seats")]
    [ApiController]
    public class SeatController : ControllerBase
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;

        public SeatController(IUnitOfWork unitOfWork, IMapper mapper)
        {
            _unitOfWork = unitOfWork;
            _mapper = mapper;
        }

        [HttpGet]
        [Authorize(Roles = $"{Constants.Role_Manager},{Constants.Role_Admin}")]
        public async Task<IActionResult> GetSeatsInScreen([FromQuery] int screenId)
        {
            if (screenId <= 0)
            {
                return BadRequest(new ErrorResponseDTO
                {
                    Message = "Invalid screen ID."
                });
            }

            var seats = await _unitOfWork.Seat.GetAllAsync(s => s.ScreenId == screenId && s.IsRemoved == false);
            return Ok(new SuccessResponseDTO
            {
                Data = _mapper.Map<List<SeatDTO>>(seats),
                Message = "Seats retrieved successfully."
            });
        }

        [HttpPut("add-row")]
        [Authorize(Roles = Constants.Role_Admin)]
        public async Task<IActionResult> AddOneRow([FromQuery] int screenId)
        {
            // get the screen
            var screen = await _unitOfWork.Screen.GetOneAsync(s => s.Id == screenId);
            if (screen == null)
            {
                return NotFound(new ErrorResponseDTO
                {
                    Message = "Screen not found."
                });
            }

            // update the screen's row count
            screen.Rows++;
            screen.LastUpdatedAt = DateTime.Now;
            _unitOfWork.Screen.Update(screen);
            await _unitOfWork.SaveAsync();

            // create new row
            var newRow = (char)('A' + (screen.Rows - 1));

            // create new seats for the new row
            var newSeats = new List<Seat>();
            for (int i = 1; i <= screen.Columns; i++)
            {
                newSeats.Add(new Seat
                {
                    SeatRow = newRow.ToString(),
                    SeatNumber = i,
                    ScreenId = screenId,
                    CreatedAt = DateTime.Now,
                    LastUpdatedAt = DateTime.Now
                });
            }

            // add new seats to the database
            await _unitOfWork.Seat.AddRangeAsync(newSeats);
            await _unitOfWork.SaveAsync();

            return Ok(new SuccessResponseDTO
            {
                Message = "New row added successfully."
            });
        }

        [HttpPut("add-column")]
        [Authorize(Roles = Constants.Role_Admin)]
        public async Task<IActionResult> AddOneColumn([FromQuery] int screenId)
        {
            // get the screen
            var screen = await _unitOfWork.Screen.GetOneAsync(s => s.Id == screenId);
            if (screen == null)
            {
                return NotFound(new ErrorResponseDTO
                {
                    Message = "Screen not found."
                });
            }

            // update the screen's column count
            screen.Columns++;
            screen.LastUpdatedAt = DateTime.Now;
            _unitOfWork.Screen.Update(screen);
            await _unitOfWork.SaveAsync();

            // create new seats for the new column
            List<Seat> newSeats = new();
            for (int i = 1; i <= screen.Rows; i++)
            {
                var rowLetter = (char)('A' + i - 1); // Convert row index to letter
                newSeats.Add(new Seat
                {
                    SeatRow = rowLetter.ToString(),
                    SeatNumber = screen.Columns, // New column number
                    ScreenId = screenId,
                    CreatedAt = DateTime.Now,
                    LastUpdatedAt = DateTime.Now
                });
            }

            // add new seats to the database
            await _unitOfWork.Seat.AddRangeAsync(newSeats);
            await _unitOfWork.SaveAsync();

            return Ok(new SuccessResponseDTO
            {
                Message = "New column added successfully."
            });
        }

        [HttpPut("remove-row")]
        [Authorize(Roles = Constants.Role_Admin)]
        public async Task<IActionResult> RemoveOneRow([FromQuery] int screenId)
        {
            // get the screen
            var screen = await _unitOfWork.Screen.GetOneAsync(s => s.Id == screenId);
            if (screen == null)
            {
                return NotFound(new ErrorResponseDTO
                {
                    Message = "Screen not found."
                });
            }

            // check if screen has no rows left
            if (screen.Rows <= 0)
            {
                return BadRequest(new ErrorResponseDTO
                {
                    Message = "No rows left to remove."
                });
            }

            // Find the last row seats
            var lastRowSeats = await _unitOfWork.Seat.GetAllAsync(s => s.ScreenId == screenId
            && s.SeatRow == ((char)('A' + screen.Rows - 1)).ToString()
            && s.IsRemoved == false);

            if (lastRowSeats == null || lastRowSeats.ToList().Count == 0)
            {
                return BadRequest(new ErrorResponseDTO
                {
                    Message = "No seats found in the last row."
                });
            }

            // mark the last row seats as removed
            foreach (var seat in lastRowSeats)
            {
                seat.IsRemoved = true;
                seat.LastUpdatedAt = DateTime.Now;
                _unitOfWork.Seat.Update(seat);
            }

            // update the screen's row count
            screen.Rows--;

            // check if this is the last row, then update the column to 0
            if (screen.Rows == 0)
            {
                screen.Columns = 0; // No columns left if no rows left
            }

            screen.LastUpdatedAt = DateTime.Now;
            _unitOfWork.Screen.Update(screen);
            await _unitOfWork.SaveAsync();

            return Ok(new SuccessResponseDTO
            {
                Message = "Last row removed successfully."
            });
        }

        [HttpPut("remove-column")]
        [Authorize(Roles = Constants.Role_Admin)]
        public async Task<IActionResult> RemoveOneColumn([FromQuery] int screenId)
        {
            // get the screen
            var screen = await _unitOfWork.Screen.GetOneAsync(s => s.Id == screenId);
            if (screen == null)
            {
                return NotFound(new ErrorResponseDTO
                {
                    Message = "Screen not found."
                });
            }

            // check if screen has no columns left
            if (screen.Columns <= 0)
            {
                return BadRequest(new ErrorResponseDTO
                {
                    Message = "No columns left to remove."
                });
            }

            // Find all seats in the last column
            var lastColumnSeats = await _unitOfWork.Seat.GetAllAsync(s => s.ScreenId == screenId
                       && s.SeatNumber == screen.Columns && s.IsRemoved == false);

            if (lastColumnSeats == null || lastColumnSeats.ToList().Count == 0)
            {
                return BadRequest(new ErrorResponseDTO
                {
                    Message = "No seats found in the last column."
                });
            }

            // mark the last column seats as removed
            foreach (var seat in lastColumnSeats)
            {
                seat.IsRemoved = true;
                seat.LastUpdatedAt = DateTime.Now;
                _unitOfWork.Seat.Update(seat);
            }

            // update the screen's column count
            screen.Columns--;

            // check if this is the last column, then update the row to 0
            if (screen.Columns == 0)
            {
                screen.Rows = 0; // No rows left if no columns left
            }
            screen.LastUpdatedAt = DateTime.Now;
            _unitOfWork.Screen.Update(screen);
            await _unitOfWork.SaveAsync();

            return Ok(new SuccessResponseDTO
            {
                Message = "Last column removed successfully."
            });
        }

        [HttpPut("{id}/disable")]
        [Authorize(Roles = $"{Constants.Role_Manager},{Constants.Role_Admin}")]
        public async Task<IActionResult> DisableSeat(int id)
        {
            var seat = await _unitOfWork.Seat.GetOneAsync(s => s.Id == id && s.IsRemoved == false);
            if (seat == null)
            {
                return NotFound(new ErrorResponseDTO
                {
                    Message = "Seat not found."
                });
            }

            seat.IsActive = false;
            seat.LastUpdatedAt = DateTime.Now;
            _unitOfWork.Seat.Update(seat);
            await _unitOfWork.SaveAsync();

            return Ok(new SuccessResponseDTO
            {
                Message = "Seat disabled successfully."
            });
        }

        [HttpPut("{id}/enable")]
        [Authorize(Roles = $"{Constants.Role_Manager},{Constants.Role_Admin}")]
        public async Task<IActionResult> EnableSeat(int id)
        {
            var seat = await _unitOfWork.Seat.GetOneAsync(s => s.Id == id && s.IsRemoved == false);
            if (seat == null)
            {
                return NotFound(new ErrorResponseDTO
                {
                    Message = "Seat not found."
                });
            }

            seat.IsActive = true;
            seat.LastUpdatedAt = DateTime.Now;
            _unitOfWork.Seat.Update(seat);
            await _unitOfWork.SaveAsync();

            return Ok(new SuccessResponseDTO
            {
                Message = "Seat enabled successfully."
            });
        }

    }
}
