using CinemaxAPI.Models.Domain;
using CinemaxAPI.Models.DTO;
using CinemaxAPI.Models.DTO.Requests;
using CinemaxAPI.Models.DTO.Responses;
using CinemaxAPI.Services;
using CinemaxAPI.Utils;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;

namespace CinemaxAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AuthController : ControllerBase
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly RoleManager<IdentityRole> _roleManager;
        private readonly SignInManager<ApplicationUser> _signInManager;
        private readonly ITokenService _tokenService;
        private readonly IEmailService _emailService;

        public AuthController(UserManager<ApplicationUser> userManager, RoleManager<IdentityRole> roleManager, ITokenService tokenService, IEmailService emailService, SignInManager<ApplicationUser> signInManager)
        {
            _userManager = userManager;
            _roleManager = roleManager;
            _tokenService = tokenService;
            _emailService = emailService;
            _signInManager = signInManager;
        }

        [HttpGet("create-roles")]
        public async Task<IActionResult> CreateRoles()
        {
            var roles = new[] { Constants.Role_Employee, Constants.Role_Admin, Constants.Role_Customer, Constants.Role_Manager };
            foreach (var role in roles)
            {
                if (!await _roleManager.RoleExistsAsync(role))
                {
                    var result = await _roleManager.CreateAsync(new IdentityRole(role));
                    if (!result.Succeeded)
                    {
                        return BadRequest(new ErrorResponseDTO
                        {
                            Message = "Role creation failed",
                            Errors = string.Join(", ", result.Errors.Select(e => e.Description)),
                            StatusCode = 400,
                            Status = "Error"
                        });
                    }
                }
            }
            return Ok(new { Message = "Roles created successfully" });
        }

        [HttpPost("register-admin")]
        public async Task<IActionResult> RegisterAdmin([FromBody] RegisterRequestDTO request)
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
            await _userManager.AddToRoleAsync(newUser, Constants.Role_Admin);

            // send email confirmation link
            var token = await _userManager.GenerateEmailConfirmationTokenAsync(newUser);
            await _userManager.ConfirmEmailAsync(newUser, token);

            return Ok(new SuccessResponseDTO
            {
                Message = "Admin registered successfully!",
                Data = newUser.Id,
            });
        }

        [HttpPost("register-employee")]
        public async Task<IActionResult> RegisterEmployee([FromBody] RegisterRequestDTO request)
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

            // assign employee role
            await _userManager.AddToRoleAsync(newUser, Constants.Role_Employee);

            // send email confirmation link
            var token = await _userManager.GenerateEmailConfirmationTokenAsync(newUser);
            await _userManager.ConfirmEmailAsync(newUser, token);

            return Ok(new SuccessResponseDTO
            {
                Message = "Employee registered successfully!",
                Data = newUser.Id,
            });
        }

        // this api is for customer to register
        [HttpPost("register")]
        public async Task<IActionResult> Register([FromBody] RegisterRequestDTO request)
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
            var confirmationLink = Url.Action(nameof(ConfirmEmail), "Auth",
            new { userId = newUser.Id, token }, Request.Scheme);

            await _emailService.SendEmailAsync(request.Email, "Confirm your email", $"Click to confirm: <a href=\"{confirmationLink}\">Confirm email</a>");


            return Ok(new SuccessResponseDTO
            {
                Message = "User registered successfully! Please check your email to confirm registration.",
                Data = newUser.Id,
            });
        }

        [HttpGet("confirm-email")]
        public async Task<IActionResult> ConfirmEmail(string userId, string token)
        {
            if (string.IsNullOrEmpty(userId) || string.IsNullOrEmpty(token))
            {
                return BadRequest(new ErrorResponseDTO
                {
                    Message = "Invalid email confirmation request",
                    StatusCode = 400,
                    Status = "Error"
                });
            }

            var user = await _userManager.FindByIdAsync(userId);
            if (user == null)
            {
                return NotFound(new ErrorResponseDTO
                {
                    Message = "User not found",
                    StatusCode = 404,
                    Status = "Error"
                });
            }

            var result = await _userManager.ConfirmEmailAsync(user, token);
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
    }
}
