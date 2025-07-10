using AutoMapper;
using CinemaxAPI.CustomActionFilters;
using CinemaxAPI.Models.Domain;
using CinemaxAPI.Models.DTO;
using CinemaxAPI.Models.DTO.Requests;
using CinemaxAPI.Models.DTO.Responses;
using CinemaxAPI.Repositories;
using CinemaxAPI.Services;
using CinemaxAPI.Utils;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace CinemaxAPI.Controllers
{
    [Route("api/concessions")]
    [ApiController]
    public class ConcessionController : ControllerBase
    {
        private readonly IConcessionRepository _repo;
        private readonly IImageService _imageService;
        private readonly IMapper _mapper;

        public ConcessionController(IConcessionRepository repo, IImageService imageService, IMapper mapper)
        {
            _repo = repo;
            _imageService = imageService;
            _mapper = mapper;
        }

        [HttpGet]
        public async Task<IActionResult> GetAllConcessions()
        {
            var items = await _repo.GetAllAsync();
            if (items == null || !items.Any())
            {
                return Ok(new SuccessResponseDTO
                {
                    Data = new List<ConcessionDTO>(),
                    Message = "No concessions found."
                });
            }
            return Ok(new SuccessResponseDTO
            {
                Data = _mapper.Map<List<ConcessionDTO>>(items),
                Message = "Concessions retrieved successfully."
            });
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetConcessionById(int id)
        {
            var item = await _repo.GetByIdAsync(id);
            if (item == null)
            {
                return NotFound(new ErrorResponseDTO
                {
                    Message = "Concession not found.",
                    StatusCode = 404
                });
            }
            return Ok(new SuccessResponseDTO
            {
                Data = _mapper.Map<ConcessionDTO>(item),
                Message = "Concession retrieved successfully."
            });
        }

        [HttpPost]
        [ValidateModel]
        //[Authorize(Roles = Constants.Role_Employee)]
        public async Task<IActionResult> CreateConcession([FromForm] CreateConcessionRequestDTO request)
        {
            var newConcession = new Concession
            {
                Name = request.Name,
                Description = request.Description,
                Price = request.Price,
            };

            IFormFile file = request.File;
            if (file != null)
            {
                ValidateImage(file);
                if (!ModelState.IsValid)
                {
                    return BadRequest(new ErrorResponseDTO
                    {
                        Message = "Invalid file",
                        StatusCode = 400,
                        Status = "Error"
                    });
                }
                var newFileName = await _imageService.Upload(file);
                if (string.IsNullOrEmpty(newFileName))
                {
                    return BadRequest(new ErrorResponseDTO
                    {
                        Message = "Failed to upload image.",
                        StatusCode = 500,
                        Status = "Error"
                    });
                }
                newConcession.ImageUrl = newFileName;
            }

            newConcession.CreatedAt = DateTime.Now;
            newConcession.LastUpdatedAt = DateTime.Now;
            newConcession.IsActive = true;
            await _repo.CreateAsync(newConcession);
            var dto = _mapper.Map<ConcessionDTO>(newConcession);
            return CreatedAtAction(nameof(GetConcessionById), new { id = newConcession.Id }, new SuccessResponseDTO
            {
                Data = dto,
                Message = "Concession created successfully."
            });
        }

        [HttpPut("{id}")]
        [ValidateModel]
        //[Authorize(Roles = Constants.Role_Employee)]
        public async Task<IActionResult> UpdateConcession(int id, [FromForm] UpdateConcessionRequestDTO request)
        {
            var concession = await _repo.GetByIdAsync(id);
            if (concession == null)
            {
                return NotFound(new ErrorResponseDTO
                {
                    Message = "Concession not found.",
                    StatusCode = 404
                });
            }
            concession.Name = request.Name;
            concession.Description = request.Description;
            concession.Price = request.Price;

            IFormFile file = request.File;
            if (file != null)
            {
                ValidateImage(file);
                if (!ModelState.IsValid)
                {
                    return BadRequest(new ErrorResponseDTO
                    {
                        Message = "Invalid file",
                        StatusCode = 400,
                        Status = "Error"
                    });
                }
                var newFileName = await _imageService.Upload(file);
                if (string.IsNullOrEmpty(newFileName))
                {
                    return BadRequest(new ErrorResponseDTO
                    {
                        Message = "Failed to upload image.",
                        StatusCode = 500,
                        Status = "Error"
                    });
                }
                concession.ImageUrl = newFileName;
            }
            concession.LastUpdatedAt = DateTime.Now;
            await _repo.UpdateAsync(id, concession);
            var dto = _mapper.Map<ConcessionDTO>(concession);
            return Ok(new SuccessResponseDTO
            {
                Data = dto,
                Message = "Concession updated successfully."
            });
        }

        [HttpPut("{id}/disable")]
        //[Authorize(Roles = Constants.Role_Employee)]
        public async Task<IActionResult> DisableConcession(int id)
        {
            var concession = await _repo.GetByIdAsync(id);
            if (concession == null)
            {
                return NotFound(new ErrorResponseDTO
                {
                    Message = "Concession not found.",
                    StatusCode = 404
                });
            }
            concession.IsActive = false;
            concession.LastUpdatedAt = DateTime.Now;
            await _repo.UpdateAsync(id, concession);
            return Ok(new SuccessResponseDTO
            {
                Data = concession.Id,
                Message = "Concession disabled successfully."
            });
        }

        [HttpPut("{id}/enable")]
        //[Authorize(Roles = Constants.Role_Employee)]
        public async Task<IActionResult> EnableConcession(int id)
        {
            var concession = await _repo.GetByIdAsync(id);
            if (concession == null)
            {
                return NotFound(new ErrorResponseDTO
                {
                    Message = "Concession not found.",
                    StatusCode = 404
                });
            }
            concession.IsActive = true;
            concession.LastUpdatedAt = DateTime.Now;
            await _repo.UpdateAsync(id, concession);
            return Ok(new SuccessResponseDTO
            {
                Data = concession.Id,
                Message = "Concession enabled successfully."
            });
        }

        [HttpDelete("{id}")]
        //[Authorize(Roles = Constants.Role_Employee)]
        public async Task<IActionResult> DeleteConcession(int id)
        {
            var result = await _repo.DeleteAsync(id);
            if (result == null)
            {
                return NotFound(new ErrorResponseDTO
                {
                    Message = "Concession not found.",
                    StatusCode = 404
                });
            }
            return Ok(new SuccessResponseDTO
            {
                Data = result.Id,
                Message = "Concession deleted successfully."
            });
        }

        private void ValidateImage(IFormFile file)
        {
            if (!_imageService.ValidateImage(file, out var errorMsg))
            {
                ModelState.AddModelError("File", errorMsg ?? "Invalid file");
            }
        }
    }
}

