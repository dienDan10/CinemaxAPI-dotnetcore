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
    [Route("api/customers")]
    [ApiController]
    [Authorize(Roles = Constants.Role_Admin)]
    public class CustomerController : ControllerBase
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly IMapper _mapper;

        public CustomerController(IUnitOfWork unitOfWork, UserManager<ApplicationUser> userManager, IMapper mapper)
        {
            _unitOfWork = unitOfWork;
            _userManager = userManager;
            _mapper = mapper;
        }

        [HttpGet]
        public async Task<IActionResult> GetAllCustomers([FromQuery] CustomerFilterRequestDTO filter, [FromQuery] SortRequestDTO sort, [FromQuery] PagedRequestDTO paged)
        {
            var (customers, totalCount) = await _unitOfWork.ApplicationUser.GetAllCustomerAsync(filter, sort, paged);

            if (customers == null || !customers.Any())
            {
                return Ok(new SuccessResponseDTO
                {
                    Data = new List<UserDTO>(),
                    Message = "No customers found."
                });
            }

            var customerDtos = _mapper.Map<List<UserDTO>>(customers);

            return Ok(new SuccessResponseDTO
            {
                Data = new
                {
                    customers = customerDtos,
                    TotalCount = totalCount,
                    TotalResult = customers.Count(),
                    page = paged?.PageNumber > 0 ? paged.PageNumber : 1,
                },
                Message = "Customers retrieved successfully.",
            });
        }

        // lock customer account
        [HttpPut("{id}/lock")]
        public async Task<IActionResult> LockCustomerAccount(string id)
        {
            var customer = await _userManager.FindByIdAsync(id);
            if (customer == null)
            {
                return NotFound(new ErrorResponseDTO
                {
                    Message = "Customer not found."
                });
            }

            customer.LockoutEnabled = true;
            customer.LockoutEnd = DateTime.UtcNow.AddYears(100); // Lock the account indefinitely

            var result = await _userManager.UpdateAsync(customer);
            if (!result.Succeeded)
            {
                return BadRequest(new ErrorResponseDTO
                {
                    Message = "Failed to lock customer account.",
                    Errors = string.Join(", ", result.Errors.Select(e => e.Description)),
                    StatusCode = 400,
                    Status = "Error"
                });
            }

            return Ok(new SuccessResponseDTO
            {
                Message = "Customer account locked successfully."
            });
        }

        // unlock customer account
        [HttpPut("{id}/unlock")]
        public async Task<IActionResult> UnlockCustomerAccount(string id)
        {
            var customer = await _userManager.FindByIdAsync(id);
            if (customer == null)
            {
                return NotFound(new ErrorResponseDTO
                {
                    Message = "Customer not found."
                });
            }

            customer.LockoutEnabled = false;
            customer.LockoutEnd = null; // Unlock the account

            var result = await _userManager.UpdateAsync(customer);
            if (!result.Succeeded)
            {
                return BadRequest(new ErrorResponseDTO
                {
                    Message = "Failed to unlock customer account.",
                    Errors = string.Join(", ", result.Errors.Select(e => e.Description)),
                    StatusCode = 400,
                    Status = "Error"
                });
            }

            return Ok(new SuccessResponseDTO
            {
                Message = "Customer account unlocked successfully."
            });
        }
    }
}
