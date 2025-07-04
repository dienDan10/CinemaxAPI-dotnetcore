using AutoMapper;
using CinemaxAPI.CustomActionFilters;
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
    [Route("api/employees")]
    [ApiController]
    [Authorize(Roles = Constants.Role_Admin)]
    public class EmployeeController : ControllerBase
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly IMapper _mapper;

        public EmployeeController(IUnitOfWork unitOfWork, UserManager<ApplicationUser> userManager, IMapper mapper)
        {
            _unitOfWork = unitOfWork;
            _userManager = userManager;
            _mapper = mapper;
        }

        [HttpGet]
        public async Task<IActionResult> GetAllEmployees([FromQuery] EmployeeFilterRequestDTO filter, [FromQuery] SortRequestDTO sort, [FromQuery] PagedRequestDTO paged)
        {
            var (employees, totalCount) = await _unitOfWork.ApplicationUser.GetAllEmployeeAsync(filter, sort, paged, includeProperties: "EmployedTheater");

            if (employees == null || !employees.Any())
            {
                return Ok(new SuccessResponseDTO
                {
                    Data = new List<EmployeeDTO>(),
                    Message = "No employees found."
                });
            }

            var employeeDtos = _mapper.Map<List<EmployeeDTO>>(employees);

            return Ok(new SuccessResponseDTO
            {
                Data = new
                {
                    employees = employeeDtos,
                    TotalCount = totalCount,
                    TotalResult = employees.Count(),
                    page = paged?.PageNumber > 0 ? paged.PageNumber : 1,
                },
                Message = "Employees retrieved successfully.",
            });
        }

        [HttpPost]
        [ValidateModel]
        public async Task<IActionResult> CreateEmployee([FromBody] RegisterEmployeeRequestDTO request)
        {

            var user = new ApplicationUser
            {
                UserName = request.Email,
                Email = request.Email,
                DisplayName = request.Username,
                PhoneNumber = request.PhoneNumber,
                TheaterId = request.TheaterId,
            };

            var result = await _userManager.CreateAsync(user, request.Password);
            if (!result.Succeeded)
            {
                return BadRequest(new ErrorResponseDTO
                {
                    Message = string.Join(", ", result.Errors.Select(e => e.Description)),
                    StatusCode = 400,
                    Status = "Error"
                });
            }

            await _userManager.AddToRoleAsync(user, Constants.Role_Employee);

            // send email confirmation link
            var token = await _userManager.GenerateEmailConfirmationTokenAsync(user);
            await _userManager.ConfirmEmailAsync(user, token);

            return Ok(new SuccessResponseDTO
            {
                Message = "Employee created successfully.",
                Data = new
                {
                    user.Id,
                }
            });
        }

        [HttpPut("{id}")]
        [ValidateModel]
        public async Task<IActionResult> UpdateEmployee(string id, [FromBody] UpdateEmployeeRequestDTO request)
        {
            var user = await _userManager.FindByIdAsync(id);
            if (user == null)
            {
                return NotFound(new ErrorResponseDTO
                {
                    Message = "Employee not found",
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
                    Message = string.Join(", ", result.Errors.Select(e => e.Description)),
                    StatusCode = 400,
                    Status = "Error"
                });
            }

            return Ok(new SuccessResponseDTO
            {
                Message = "Employee updated successfully.",
                Data = user
            });
        }

        [HttpPut("{id}/lock")]
        public async Task<IActionResult> LockEmployee(string id)
        {
            var user = await _userManager.FindByIdAsync(id);
            if (user == null)
            {
                return NotFound(new ErrorResponseDTO
                {
                    Message = "Employee not found",
                    StatusCode = 404,
                    Status = "Error"
                });
            }

            user.LockoutEnabled = true;
            user.LockoutEnd = DateTime.UtcNow.AddYears(100);

            var result = await _userManager.UpdateAsync(user);
            if (!result.Succeeded)
            {
                return BadRequest(new ErrorResponseDTO
                {
                    Message = string.Join(", ", result.Errors.Select(e => e.Description)),
                    StatusCode = 400,
                    Status = "Error"
                });
            }

            return Ok(new SuccessResponseDTO
            {
                Message = "Employee locked successfully.",
                Data = user
            });
        }

        [HttpPut("{id}/unlock")]
        public async Task<IActionResult> UnlockEmployee(string id)
        {
            var user = await _userManager.FindByIdAsync(id);
            if (user == null)
            {
                return NotFound(new ErrorResponseDTO
                {
                    Message = "Employee not found",
                    StatusCode = 404,
                    Status = "Error"
                });
            }

            user.LockoutEnabled = true;
            user.LockoutEnd = DateTimeOffset.UtcNow;

            var result = await _userManager.UpdateAsync(user);
            if (!result.Succeeded)
            {
                return BadRequest(new ErrorResponseDTO
                {
                    Message = string.Join(", ", result.Errors.Select(e => e.Description)),
                    StatusCode = 400,
                    Status = "Error"
                });
            }

            return Ok(new SuccessResponseDTO
            {
                Message = "Employee unlocked successfully.",
                Data = user
            });
        }
    }
}
