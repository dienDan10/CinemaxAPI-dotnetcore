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
    [Route("api/promotions")]
    [ApiController]
    public class PromotionController : ControllerBase
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;

        public PromotionController(IUnitOfWork unitOfWork, IMapper mapper)
        {
            _unitOfWork = unitOfWork;
            _mapper = mapper;
        }

        [HttpGet]
        [Authorize(Roles = Constants.Role_Admin)]
        public async Task<IActionResult> GetPromotions()
        {
            var promotions = await _unitOfWork.Promotion.GetAllAsync();
            return Ok(new SuccessResponseDTO
            {
                Data = _mapper.Map<List<PromotionDTO>>(promotions),
                Message = "Promotions retrieved successfully."
            });
        }

        [HttpGet("active")]
        public async Task<IActionResult> GetCurrentlyActivePromotion()
        {
            DateOnly today = DateOnly.FromDateTime(DateTime.Now);
            var promotions = await _unitOfWork.Promotion.GetAllAsync(p => p.StartDate <= today && p.EndDate >= today && p.IsActive && p.Quantity > p.UsedQuantity);

            return Ok(new SuccessResponseDTO
            {
                Data = _mapper.Map<List<PromotionDTO>>(promotions),
                Message = "Promotions retrieved successfully."
            });
        }

        [HttpGet("{id}")]
        [Authorize(Roles = Constants.Role_Admin)]
        public async Task<IActionResult> GetPromotionById(int id)
        {
            var promotion = await _unitOfWork.Promotion.GetOneAsync(p => p.Id == id);
            if (promotion == null)
            {
                return NotFound(new ErrorResponseDTO
                {
                    Message = "Promotion not found."
                });
            }
            return Ok(new SuccessResponseDTO
            {
                Data = _mapper.Map<PromotionDTO>(promotion),
                Message = "Promotion retrieved successfully."
            });
        }

        [HttpPost]
        [ValidateModel]
        [Authorize(Roles = Constants.Role_Admin)]
        public async Task<IActionResult> CreatePromotion([FromBody] CreatePromotionRequestDTO request)
        {
            // check start date and end date
            if (request.StartDate >= request.EndDate)
            {
                return BadRequest(new ErrorResponseDTO
                {
                    Message = "Start date must be before end date.",
                    StatusCode = 400,
                    Status = "Error"
                });
            }

            var promotion = new Promotion
            {
                Description = request.Description,
                DiscountType = request.DiscountType,
                DiscountValue = request.DiscountValue,
                Quantity = request.Quantity,
                StartDate = request.StartDate,
                EndDate = request.EndDate,
                IsActive = true, // Default status is true
            };

            await _unitOfWork.Promotion.AddAsync(promotion);
            await _unitOfWork.SaveAsync();
            return CreatedAtAction(nameof(GetPromotionById), new { id = promotion.Id }, new SuccessResponseDTO
            {
                Data = _mapper.Map<PromotionDTO>(promotion),
                Message = "Promotion created successfully."
            });
        }

        [HttpPut("{id}")]
        [ValidateModel]
        [Authorize(Roles = Constants.Role_Admin)]
        public async Task<IActionResult> UpdatePromotion(int id, [FromBody] UpdatePromotionRequestDTO request)
        {
            // check start date and end date
            if (request.StartDate >= request.EndDate)
            {
                return BadRequest(new ErrorResponseDTO
                {
                    Message = "Start date must be before end date.",
                    StatusCode = 400,
                    Status = "Error"
                });
            }
            var promotion = await _unitOfWork.Promotion.GetOneAsync(p => p.Id == id);
            if (promotion == null)
            {
                return NotFound(new ErrorResponseDTO
                {
                    Message = "Promotion not found."
                });
            }

            // don't allow update if promotion is already used
            if (promotion.UsedQuantity >= 0)
            {
                return BadRequest(new ErrorResponseDTO
                {
                    Message = "Cannot update promotion that has already been used.",
                    StatusCode = 400,
                    Status = "Error"
                });
            }
            // update promotion details
            promotion.Description = request.Description;
            promotion.DiscountType = request.DiscountType;
            promotion.DiscountValue = request.DiscountValue;
            promotion.Quantity = request.Quantity;
            promotion.StartDate = request.StartDate;
            promotion.EndDate = request.EndDate;
            promotion.LastUpdatedAt = DateTime.Now;
            _unitOfWork.Promotion.Update(promotion);
            await _unitOfWork.SaveAsync();
            return Ok(new SuccessResponseDTO
            {
                Data = _mapper.Map<PromotionDTO>(promotion),
                Message = "Promotion updated successfully."
            });
        }

        [HttpDelete("{id}")]
        [Authorize(Roles = Constants.Role_Admin)]
        public async Task<IActionResult> DeletePromotion(int id)
        {
            var promotion = await _unitOfWork.Promotion.GetOneAsync(p => p.Id == id);
            if (promotion == null)
            {
                return NotFound(new ErrorResponseDTO
                {
                    Message = "Promotion not found."
                });
            }
            // don't allow delete if promotion is already used
            if (promotion.UsedQuantity > 0)
            {
                return BadRequest(new ErrorResponseDTO
                {
                    Message = "Cannot delete promotion that has already been used.",
                    StatusCode = 400,
                    Status = "Error"
                });
            }
            _unitOfWork.Promotion.Remove(promotion);
            await _unitOfWork.SaveAsync();
            return Ok(new SuccessResponseDTO
            {
                Message = "Promotion deleted successfully."
            });
        }

        [HttpPut("{id}/enable")]
        [Authorize(Roles = Constants.Role_Admin)]
        public async Task<IActionResult> EnablePromotion(int id)
        {
            var promotion = await _unitOfWork.Promotion.GetOneAsync(p => p.Id == id);
            if (promotion == null)
            {
                return NotFound(new ErrorResponseDTO
                {
                    Message = "Promotion not found."
                });
            }
            promotion.IsActive = true;
            promotion.LastUpdatedAt = DateTime.Now;
            _unitOfWork.Promotion.Update(promotion);
            await _unitOfWork.SaveAsync();
            return Ok(new SuccessResponseDTO
            {
                Data = _mapper.Map<PromotionDTO>(promotion),
                Message = "Promotion enabled successfully."
            });
        }

        [HttpPut("{id}/disable")]
        [Authorize(Roles = Constants.Role_Admin)]
        public async Task<IActionResult> DisablePromotion(int id)
        {
            var promotion = await _unitOfWork.Promotion.GetOneAsync(p => p.Id == id);
            if (promotion == null)
            {
                return NotFound(new ErrorResponseDTO
                {
                    Message = "Promotion not found."
                });
            }
            promotion.IsActive = false;
            promotion.LastUpdatedAt = DateTime.Now;
            _unitOfWork.Promotion.Update(promotion);
            await _unitOfWork.SaveAsync();
            return Ok(new SuccessResponseDTO
            {
                Data = _mapper.Map<PromotionDTO>(promotion),
                Message = "Promotion disabled successfully."
            });
        }
    }
}
