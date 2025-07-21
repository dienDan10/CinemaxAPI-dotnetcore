using CinemaxAPI.Models.Domain;
using CinemaxAPI.Models.DTO;
using CinemaxAPI.Models.DTO.Requests;
using CinemaxAPI.Models.DTO.Responses;
using CinemaxAPI.Services;
using CinemaxAPI.Utils;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using System.Net;

namespace CinemaxAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AuthController : ControllerBase
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly RoleManager<IdentityRole> _roleManager;
        private readonly SignInManager<ApplicationUser> _signInManager;
        private readonly IConfiguration _configuration;
        private readonly ITokenService _tokenService;
        private readonly IEmailService _emailService;

        public AuthController(UserManager<ApplicationUser> userManager, RoleManager<IdentityRole> roleManager, ITokenService tokenService, IEmailService emailService, SignInManager<ApplicationUser> signInManager, IConfiguration configuration)
        {
            _userManager = userManager;
            _roleManager = roleManager;
            _tokenService = tokenService;
            _emailService = emailService;
            _signInManager = signInManager;
            _configuration = configuration;
        }

        // this api is for customer to register
        [HttpPost("register")]
        public async Task<IActionResult> Register([FromBody] RegisterRequestDTO request)
        {
            // check for existing user
            var existingUser = await _userManager.FindByEmailAsync(request.Email);
            if (existingUser != null)
            {
                return BadRequest(new ErrorResponseDTO
                {
                    Message = "Email already in use",
                    StatusCode = 400,
                    Status = "Error"
                });
            }

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

            // assign customer role
            var addRoleResult = await _userManager.AddToRoleAsync(newUser, Constants.Role_Customer);
            if (!addRoleResult.Succeeded)
            {
                return BadRequest(new ErrorResponseDTO
                {
                    Message = "Failed to assign role",
                    Errors = string.Join(", ", addRoleResult.Errors.Select(e => e.Description)),
                    StatusCode = 400,
                    Status = "Error"
                });
            }

            // send email confirmation link
            var token = await _userManager.GenerateEmailConfirmationTokenAsync(newUser);
            var encodedToken = WebUtility.UrlEncode(token);
            // Generate confirmation link
            var clientUrl = _configuration["CinemaxClients:Website"];

            var confirmationLink = $"{clientUrl}/confirm-email?userId={newUser.Id}&token={encodedToken}";

            await _emailService.SendEmailAsync(request.Email, "Confirm your email", $"Click to confirm: <a href=\"{confirmationLink}\">Confirm email</a>");


            return Ok(new SuccessResponseDTO
            {
                Message = "User registered successfully! Please check your email to confirm registration.",
                Data = newUser.Id,
            });
        }

        [HttpPost("confirm-email")]
        public async Task<IActionResult> ConfirmEmail([FromBody] ConfirmEmailRequestDTO request)
        {

            var user = await _userManager.FindByIdAsync(request.UserId);
            if (user == null)
            {
                return NotFound(new ErrorResponseDTO
                {
                    Message = "User not found",
                    StatusCode = 404,
                    Status = "Error"
                });
            }

            var result = await _userManager.ConfirmEmailAsync(user, request.Token);
            if (!result.Succeeded)
            {
                return BadRequest(new ErrorResponseDTO
                {
                    Message = "Email confirmation failed",
                    Errors = string.Join(", ", result.Errors.Select(e => e.Description)),
                    StatusCode = 400,
                    Status = "Error"
                });
            }

            return Ok(new SuccessResponseDTO
            {
                Message = "Email confirmed successfully",
                Data = user.Id,
            });
        }

        [HttpPost("login")]
        public async Task<IActionResult> Login([FromBody] LoginRequestDTO request)
        {
            var user = await _userManager.FindByEmailAsync(request.Email);
            if (user == null)
            {
                return BadRequest(new ErrorResponseDTO
                {
                    Message = "Invalid email or password",
                    StatusCode = 400,
                    Status = "Error"
                });
            }

            var isEmailConfirmed = await _userManager.IsEmailConfirmedAsync(user);
            if (!isEmailConfirmed)
            {
                return BadRequest(new ErrorResponseDTO
                {
                    Message = "Email not confirmed",
                    StatusCode = 400,
                    Status = "Error"
                });
            }

            var isPasswordValid = await _userManager.CheckPasswordAsync(user, request.Password);
            if (!isPasswordValid)
            {
                return BadRequest(new ErrorResponseDTO
                {
                    Message = "Invalid email or password",
                    StatusCode = 400,
                    Status = "Error"
                });
            }

            // check if user is locked out
            if (await _userManager.IsLockedOutAsync(user))
            {
                return BadRequest(new ErrorResponseDTO
                {
                    Message = "User account is locked",
                    StatusCode = 403,
                    Status = "Error"
                });
            }

            // Generate JWT token here
            var roles = await _userManager.GetRolesAsync(user);
            var token = _tokenService.CreateJwtToken(user, roles.ToList());
            // get user info 
            var userProfile = new UserProfileDTO
            {
                Id = user.Id,
                Email = user.Email,
                DisplayName = user.DisplayName,
                TheaterId = user.TheaterId,
                Role = roles.FirstOrDefault() ?? Constants.Role_Customer,
            };


            return Ok(new SuccessResponseDTO
            {
                Message = "Login successful",
                Data = new
                {
                    accessToken = token,
                    user = userProfile
                }
            });
        }

        [HttpPost("forgot-password")]
        public async Task<IActionResult> ForgotPassword([FromBody] ForgotPasswordRequestDTO request)
        {
            var user = await _userManager.FindByEmailAsync(request.Email);
            if (user == null)
            {
                return BadRequest(new ErrorResponseDTO
                {
                    Message = "Email not found",
                    StatusCode = 400,
                    Status = "Error"
                });
            }
            // Generate password reset token
            var token = await _userManager.GeneratePasswordResetTokenAsync(user);
            var encodedToken = WebUtility.UrlEncode(token);
            // Generate reset link
            var clientUrl = _configuration["CinemaxClients:Website"];
            var resetLink = $"{clientUrl}/reset-password?userId={user.Id}&token={encodedToken}";
            // Send email with reset link
            await _emailService.SendEmailAsync(request.Email, "Reset your password", $"Click to reset: <a href=\"{resetLink}\">Reset password</a>");
            return Ok(new SuccessResponseDTO
            {
                Message = "Password reset link sent to your email",
                Data = user.Id,
            });
        }

        [HttpPost("reset-password")]
        public async Task<IActionResult> ResetPassword([FromBody] ResetPasswordRequestDTO request)
        {
            var user = await _userManager.FindByIdAsync(request.UserId);
            if (user == null)
            {
                return NotFound(new ErrorResponseDTO
                {
                    Message = "User not found",
                    StatusCode = 404,
                    Status = "Error"
                });
            }
            var result = await _userManager.ResetPasswordAsync(user, request.ResetToken, request.Password);
            if (!result.Succeeded)
            {
                return BadRequest(new ErrorResponseDTO
                {
                    Message = "Password reset failed",
                    Errors = string.Join(", ", result.Errors.Select(e => e.Description)),
                    StatusCode = 400,
                    Status = "Error"
                });
            }
            return Ok(new SuccessResponseDTO
            {
                Message = "Password reset successfully",
                Data = user.Id,
            });
        }

        [HttpPost("change-password")]
        public async Task<IActionResult> ChangePassword([FromBody] ChangePasswordRequestDTO request)
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null)
            {
                return Unauthorized(new ErrorResponseDTO
                {
                    Message = "User not found",
                    StatusCode = 401,
                    Status = "Error"
                });
            }
            var result = await _userManager.ChangePasswordAsync(user, request.OldPassword, request.NewPassword);
            if (!result.Succeeded)
            {
                return BadRequest(new ErrorResponseDTO
                {
                    Message = "Password incorrect",
                    Errors = string.Join(", ", result.Errors.Select(e => e.Description)),
                    StatusCode = 400,
                    Status = "Error"
                });
            }
            return Ok(new SuccessResponseDTO
            {
                Message = "Password changed successfully",
                Data = user.Id,
            });
        }
    }
}
