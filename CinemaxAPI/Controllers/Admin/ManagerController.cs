using AutoMapper;
using CinemaxAPI.Models.Domain;
using CinemaxAPI.Models.DTO;
using CinemaxAPI.Models.DTO.Requests;
using CinemaxAPI.Models.DTO.Responses;
using CinemaxAPI.Repositories;
using CinemaxAPI.Utils;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;

namespace CinemaxAPI.Controllers.Admin
{
    [Route("api/managers")]
    [ApiController]
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
        [Authorize(Roles = $"{Constants.Role_Admin}")]
        public async Task<IActionResult> GetAllManagers()
        {
            var managers = await _unitOfWork.ApplicationUser.GetAllManagers();

            if (managers == null || managers.Count == 0)
            {
                return Ok(new SuccessResponseDTO
                {
                    Data = new List<UserDTO>(),
                    Message = "No managers found."
                });
            }

            var managerDtos = _mapper.Map<List<UserDTO>>(managers);

            for (int i = 0; i < managerDtos.Count; i++)
            {
                var user = managers[i];
                var roles = await _userManager.GetRolesAsync(user);
                managerDtos[i].Roles = roles.ToArray();
            }

            return Ok(new
            {
                Data = managerDtos,
                Message = "Managers retrieved successfully."
            });
        }

        [HttpPost]
        public async Task<IActionResult> RegisterManager([FromBody] RegisterRequestDTO request)
        {
            var newUser = new ApplicationUser
            {
                UserName = request.Email,
                Email = request.Email,
                DisplayName = request.Username
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
    }
}
