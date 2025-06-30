using AutoMapper;
using CinemaxAPI.Models.Domain;
using CinemaxAPI.Models.DTO;
using CinemaxAPI.Models.DTO.Responses;
using CinemaxAPI.Repositories;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;

namespace CinemaxAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AccountController : ControllerBase
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly IMapper _mapper;

        public AccountController(IUnitOfWork unitOfWork, UserManager<ApplicationUser> userManager, IMapper mapper)
        {
            _unitOfWork = unitOfWork;
            _userManager = userManager;
            _mapper = mapper;
        }

        [HttpGet("customers")]
        public async Task<IActionResult> GetAllCustomers()
        {
            var users = await _unitOfWork.ApplicationUser.GetAllCustomers();

            if (users == null || users.Count == 0)
            {
                return Ok(new SuccessResponseDTO
                {
                    Data = new List<UserDTO>(),
                    Message = "No customers found.",
                });
            }

            var userDtos = _mapper.Map<List<UserDTO>>(users);
            // get roles of each user
            for (int i = 0; i < userDtos.Count; i++)
            {
                var user = users[i];
                var roles = await _userManager.GetRolesAsync(user);
                userDtos[i].Roles = roles.ToArray();
            }

            return Ok(new SuccessResponseDTO
            {
                Data = userDtos,
                Message = "Customers retrieved successfully.",
            });
        }

        [HttpGet("employees")]
        public async Task<IActionResult> GetAllEmployees([FromBody] int theaterId)
        {
            // Validate theaterId
            if (theaterId <= 0)
            {
                return BadRequest(new SuccessResponseDTO
                {
                    Message = "Invalid theater ID.",
                    StatusCode = 400,
                });
            }

            var users = await _unitOfWork.ApplicationUser.GetAllEmployees(theaterId);

            if (users == null || users.Count == 0)
            {
                return Ok(new SuccessResponseDTO
                {
                    Data = new List<UserDTO>(),
                    Message = "No employees found.",
                });
            }

            var userDtos = _mapper.Map<List<UserDTO>>(users);
            // get roles of each user
            for (int i = 0; i < userDtos.Count; i++)
            {
                var user = users[i];
                var roles = await _userManager.GetRolesAsync(user);
                userDtos[i].Roles = roles.ToArray();
            }

            return Ok(new SuccessResponseDTO
            {
                Data = userDtos,
                Message = "Employees retrieved successfully.",
            });
        }
    }
}
