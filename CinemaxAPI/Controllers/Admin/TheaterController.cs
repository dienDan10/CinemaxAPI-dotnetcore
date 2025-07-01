using AutoMapper;
using CinemaxAPI.CustomActionFilters;
using CinemaxAPI.Models.Domain;
using CinemaxAPI.Models.DTO;
using CinemaxAPI.Models.DTO.Requests;
using CinemaxAPI.Models.DTO.Responses;
using CinemaxAPI.Repositories;
using Microsoft.AspNetCore.Mvc;

namespace CinemaxAPI.Controllers.Admin
{
    [Route("api/theaters")]
    [ApiController]
    public class TheaterController : ControllerBase
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;

        public TheaterController(IUnitOfWork unitOfWork, IMapper mapper)
        {
            _unitOfWork = unitOfWork;
            _mapper = mapper;
        }

        [HttpGet]
        public async Task<IActionResult> GetAllTheaters()
        {
            var theaters = await _unitOfWork.Theater.GetAllAsync(includeProperties: "Province");

            if (theaters == null)
            {
                return Ok(new SuccessResponseDTO
                {
                    Data = new List<TheaterDTO>(),
                    Message = "No theaters found."
                });
            }

            return Ok(new
            {
                Data = _mapper.Map<List<TheaterDTO>>(theaters),
                Message = "Theaters retrieved successfully."
            });
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetTheaterById(int id)
        {
            var theater = await _unitOfWork.Theater.GetOneAsync(t => t.Id == id, includeProperties: "Province");

            if (theater == null)
            {
                return NotFound(new ErrorResponseDTO
                {
                    Message = "Theater not found.",
                    StatusCode = 404
                });
            }

            return Ok(new
            {
                Data = _mapper.Map<TheaterDTO>(theater),
                Message = "Theater retrieved successfully."
            });
        }

        [HttpPost]
        [ValidateModel]
        public async Task<IActionResult> CreateTheater([FromBody] CreateTheaterRequestDTO request)
        {
            var theater = new Theater
            {
                Name = request.Name,
                Address = request.Address,
                Phone = request.Phone,
                Email = request.Email,
                Description = request.Description,
                ProvinceId = request.ProvinceId,
                IsActive = true,
                CreatedAt = DateTime.Now,
                LastUpdatedAt = DateTime.Now
            };
            await _unitOfWork.Theater.AddAsync(theater);
            await _unitOfWork.SaveAsync();

            return CreatedAtAction(nameof(GetTheaterById), new { id = theater.Id }, new
            {
                Data = _mapper.Map<TheaterDTO>(theater),
                Message = "Theater created successfully."
            });
        }

        [HttpPut("{id}")]
        [ValidateModel]
        public async Task<IActionResult> UpdateTheater(int id, [FromBody] UpdateTheaterRequestDTO request)
        {
            var theater = await _unitOfWork.Theater.GetOneAsync(t => t.Id == id);

            if (theater == null)
            {
                return NotFound(new ErrorResponseDTO
                {
                    Message = "Theater not found.",
                    StatusCode = 404
                });
            }

            theater.Name = request.Name;
            theater.Address = request.Address;
            theater.Phone = request.Phone;
            theater.Email = request.Email;
            theater.Description = request.Description;
            theater.ProvinceId = request.ProvinceId;
            theater.LastUpdatedAt = DateTime.Now;

            _unitOfWork.Theater.Update(theater);
            await _unitOfWork.SaveAsync();

            return Ok(new
            {
                Data = _mapper.Map<TheaterDTO>(theater),
                Message = "Theater updated successfully."
            });
        }
    }
}
