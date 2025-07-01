using AutoMapper;
using CinemaxAPI.CustomActionFilters;
using CinemaxAPI.Models.Domain;
using CinemaxAPI.Models.DTO;
using CinemaxAPI.Models.DTO.Requests;
using CinemaxAPI.Models.DTO.Responses;
using CinemaxAPI.Repositories;
using CinemaxAPI.Utils;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;

namespace CinemaxAPI.Controllers.Admin
{
    [Route("api/managers")]
    [ApiController]
    //[Authorize(Roles = $"{Constants.Role_Admin}")]
    public class ManagerController : ControllerBase
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly IMapper _mapper;

        public ManagerController(IUnitOfWork unitOfWork, IMapper mapper, UserManager<ApplicationUser> userManager)
        {
            _unitOfWork = unitOfWork;
            _mapper = mapper;
            _userManager = userManager;
        }

        [HttpGet]
        public async Task<IActionResult> GetAllManagers()
        {
            var managers = await _userManager.GetUsersInRoleAsync(Constants.Role_Manager);
            if (managers == null || !managers.Any())
            {
                return Ok(new SuccessResponseDTO
                {
                    Data = new List<UserDTO>(),
                    Message = "No managers found."
                });
            }

            // if manager have managed theaters, include them
            foreach (var manager in managers)
            {
                var managedTheater = await _unitOfWork.Theater.GetOneAsync(t => t.Id == manager.TheaterId);
                if (managedTheater != null)
                {
                    manager.ManagedTheater = managedTheater;
                }
            }

            var managerDtos = _mapper.Map<List<ManagerDTO>>(managers);

            return Ok(new SuccessResponseDTO
            {
                Data = managerDtos,
                Message = "Managers retrieved successfully."
            });
        }

        [HttpPost]
        [ValidateModel]
        public async Task<IActionResult> RegisterManager([FromBody] RegisterManagerRequestDTO request)
        {
            var newUser = new ApplicationUser
            {
                UserName = request.Email,
                Email = request.Email,
                DisplayName = request.Username,
                PhoneNumber = request.PhoneNumber,
                TheaterId = request.TheaterId,
            };

            // save user
            var createUserResult = await _userManager.CreateAsync(newUser, request.Password);
            if (!createUserResult.Succeeded)
            {
                return BadRequest(new ErrorResponseDTO
                {
                    Message = "User creation failed",
                    Errors = string.Join(", ", createUserResult.Errors.Select(e => e.Description)),
                    StatusCode = 400,
                    Status = "Error"
                });
            }

            // assign manager role
            await _userManager.AddToRoleAsync(newUser, Constants.Role_Manager);

            // send email confirmation link
            var token = await _userManager.GenerateEmailConfirmationTokenAsync(newUser);
            await _userManager.ConfirmEmailAsync(newUser, token);

            return Ok(new SuccessResponseDTO
            {
                Message = "Manager registered successfully!",
                Data = newUser.Id,
            });
        }

        [HttpPut("{id}")]
        [ValidateModel]
        public async Task<IActionResult> UpdateManager(string id, [FromBody] UpdateManagerRequestDTO request)
        {
            var user = await _userManager.FindByIdAsync(id);
            if (user == null)
            {
                return NotFound(new ErrorResponseDTO
                {
                    Message = "Manager not found",
                    StatusCode = 404,
                    Status = "Error"
                });
            }

            user.Email = request.Email;
            user.DisplayName = request.Username;
            user.PhoneNumber = request.PhoneNumber;
            user.TheaterId = request.TheaterId;

            var result = await _userManager.UpdateAsync(user);
            if (!result.Succeeded)
            {
                return BadRequest(new ErrorResponseDTO
                {
                    Message = "Failed to update manager",
                    Errors = string.Join(", ", result.Errors.Select(e => e.Description)),
                    StatusCode = 400,
                    Status = "Error"
                });
            }

            return Ok(new SuccessResponseDTO
            {
                Message = "Manager updated successfully",
                Data = user.Id
            });
        }

        [HttpPut("{id}/lock")]
        public async Task<IActionResult> LockManager(string id)
        {
            var user = await _userManager.FindByIdAsync(id);
            if (user == null)
            {
                return NotFound(new ErrorResponseDTO
                {
                    Message = "Manager not found",
                    StatusCode = 404,
                    Status = "Error"
                });
            }

            user.LockoutEnd = DateTime.UtcNow.AddYears(100); // effectively locks the user
            var result = await _userManager.UpdateAsync(user);

            if (!result.Succeeded)
            {
                return BadRequest(new ErrorResponseDTO
                {
                    Message = "Failed to lock manager",
                    Errors = string.Join(", ", result.Errors.Select(e => e.Description)),
                    StatusCode = 400,
                    Status = "Error"
                });
            }

            return Ok(new SuccessResponseDTO
            {
                Message = "Manager locked successfully",
                Data = user.Id
            });
        }

        [HttpPut("{id}/unlock")]
        public async Task<IActionResult> UnlockManager(string id)
        {
            var user = await _userManager.FindByIdAsync(id);
            if (user == null)
            {
                return NotFound(new ErrorResponseDTO
                {
                    Message = "Manager not found",
                    StatusCode = 404,
                    Status = "Error"
                });
            }

            user.LockoutEnd = null; // unlock the user
            var result = await _userManager.UpdateAsync(user);

            if (!result.Succeeded)
            {
                return BadRequest(new ErrorResponseDTO
                {
                    Message = "Failed to unlock manager",
                    Errors = string.Join(", ", result.Errors.Select(e => e.Description)),
                    StatusCode = 400,
                    Status = "Error"
                });
            }

            return Ok(new SuccessResponseDTO
            {
                Message = "Manager unlocked successfully",
                Data = user.Id
            });
        }
    }
}
