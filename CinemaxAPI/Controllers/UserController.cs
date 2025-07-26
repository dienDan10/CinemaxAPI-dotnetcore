using CinemaxAPI.Models.Domain;
using CinemaxAPI.Models.DTO;
using CinemaxAPI.Models.DTO.Responses;
using CinemaxAPI.Utils;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace CinemaxAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class UserController : ControllerBase
    {
        private readonly UserManager<ApplicationUser> _userManager;

        public UserController(UserManager<ApplicationUser> userManager)
        {
            _userManager = userManager;
        }

        [HttpGet("profile")]
        [Authorize]
        public async Task<IActionResult> GetUserProfile()
        {
            // get user profile
            var email = User.FindFirstValue(ClaimTypes.Email);

            if (email == null)
            {
                return Unauthorized(new ErrorResponseDTO
                {
                    Message = "User not authenticated",
                    StatusCode = 401,
                    Status = "Error"
                });
            }

            var user = await _userManager.FindByEmailAsync(email);
            if (user == null)
            {
                return NotFound(new ErrorResponseDTO
                {
                    Message = "User not found",
                    StatusCode = 404,
                    Status = "Error"
                });
            }

            var roles = await _userManager.GetRolesAsync(user);

            var userProfile = new UserProfileDTO
            {
                Id = user.Id,
                Email = user.Email,
                DisplayName = user.DisplayName,
                TheaterId = user.TheaterId,
                Role = roles.FirstOrDefault() ?? Constants.Role_Customer,
                Point = user.Point
            };

            return Ok(new SuccessResponseDTO
            {
                Message = "",
                Status = "Success",
                StatusCode = 200,
                Data = userProfile
            });

        }

        [HttpGet("profile/{email}")]
        public async Task<IActionResult> GetUserByEamil(string email)
        {
            var user = await _userManager.FindByEmailAsync(email);

            if (user == null)
            {
                return NotFound(new ErrorResponseDTO
                {
                    Message = "User not found",
                    StatusCode = 404,
                    Status = "Error"
                });
            }

            var roles = await _userManager.GetRolesAsync(user);
            var role = roles.FirstOrDefault() ?? Constants.Role_Customer;

            if (role != Constants.Role_Customer)
            {
                return NotFound(new ErrorResponseDTO
                {
                    Message = "User not found",
                    StatusCode = 404,
                    Status = "Error"
                });
            }
            var userProfile = new UserProfileDTO
            {
                Id = user.Id,
                Email = user.Email,
                DisplayName = user.DisplayName,
                TheaterId = user.TheaterId,
                Role = roles.FirstOrDefault() ?? Constants.Role_Customer,
                Point = user.Point
            };


            return Ok(new SuccessResponseDTO
            {
                Message = "",
                Status = "Success",
                StatusCode = 200,
                Data = userProfile
            });
        }
    }
}
