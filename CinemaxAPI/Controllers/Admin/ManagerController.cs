using AutoMapper;
using CinemaxAPI.Models.Domain;
using CinemaxAPI.Models.DTO;
using CinemaxAPI.Models.DTO.Responses;
using CinemaxAPI.Repositories;
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
        //[Authorize(Roles = $"{Constants.Role_Admin}")]
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
    }
}
