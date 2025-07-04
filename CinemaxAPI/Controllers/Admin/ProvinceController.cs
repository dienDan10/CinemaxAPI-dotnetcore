using AutoMapper;
using CinemaxAPI.CustomActionFilters;
using CinemaxAPI.Models.DTO;
using CinemaxAPI.Models.DTO.Requests;
using CinemaxAPI.Models.DTO.Responses;
using CinemaxAPI.Repositories;
using CinemaxAPI.Utils;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace CinemaxAPI.Controllers.Admin
{
    [Route("api/provinces")]
    [ApiController]
    [Authorize(Roles = Constants.Role_Admin)]
    public class ProvinceController : ControllerBase
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;

        public ProvinceController(IUnitOfWork unitOfWork, IMapper mapper)
        {
            _unitOfWork = unitOfWork;
            _mapper = mapper;
        }

        [HttpGet]
        public async Task<IActionResult> GetAllProvinces()
        {
            var provinces = await _unitOfWork.Province.GetAllAsync();
            var provinceDtos = _mapper.Map<List<ProvinceDTO>>(provinces);

            foreach (var province in provinceDtos)
            {
                province.TheaterCount = await _unitOfWork.Theater.CountByProvinceId(province.Id);
            }

            return Ok(new SuccessResponseDTO
            {
                Data = provinces,
                Message = "Provinces retrieved successfully."
            });
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetProvinceById(int id)
        {
            var province = await _unitOfWork.Province.GetOneAsync(p => p.Id == id);
            if (province == null)
            {
                return NotFound(new ErrorResponseDTO
                {
                    Message = "Province not found.",
                    StatusCode = 404,
                    Status = "Error"
                });
            }

            var provinceDto = _mapper.Map<ProvinceDTO>(province);
            provinceDto.TheaterCount = await _unitOfWork.Theater.CountByProvinceId(province.Id);

            return Ok(new SuccessResponseDTO
            {
                Data = provinceDto,
                Message = "Province retrieved successfully."
            });
        }

        [HttpPost]
        [ValidateModel]
        public async Task<IActionResult> CreateProvince([FromBody] CreateProvinceRequestDTO request)
        {

            var province = new Models.Domain.Province
            {
                Name = request.Name,
            };
            await _unitOfWork.Province.AddAsync(province);
            await _unitOfWork.SaveAsync();

            return CreatedAtAction(nameof(GetProvinceById), new { id = province.Id }, new SuccessResponseDTO
            {
                Data = _mapper.Map<ProvinceDTO>(province),
                Message = "Province created successfully."
            });
        }

        [HttpPut("{id}")]
        [ValidateModel]
        public async Task<IActionResult> UpdateProvince(int id, [FromBody] UpdateProvinceRequestDTO request)
        {
            var province = await _unitOfWork.Province.GetOneAsync(p => p.Id == id);
            if (province == null)
            {
                return NotFound(new ErrorResponseDTO
                {
                    Message = "Province not found.",
                    StatusCode = 404,
                    Status = "Error"
                });
            }

            province.Name = request.Name;
            _unitOfWork.Province.Update(province);
            await _unitOfWork.SaveAsync();

            return Ok(new SuccessResponseDTO
            {
                Data = _mapper.Map<ProvinceDTO>(province),
                Message = "Province updated successfully."
            });
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteProvince(int id)
        {
            var province = await _unitOfWork.Province.GetOneAsync(p => p.Id == id);
            if (province == null)
            {
                return NotFound(new ErrorResponseDTO
                {
                    Message = "Province not found.",
                    StatusCode = 404,
                    Status = "Error"
                });
            }

            _unitOfWork.Province.Remove(province);
            await _unitOfWork.SaveAsync();

            return Ok(new SuccessResponseDTO
            {
                Message = "Province deleted successfully."
            });
        }
    }
}
