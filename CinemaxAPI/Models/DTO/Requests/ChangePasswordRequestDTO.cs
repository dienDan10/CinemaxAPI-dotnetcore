using System.ComponentModel.DataAnnotations;

namespace CinemaxAPI.Models.DTO.Requests
{
    public class ChangePasswordRequestDTO
    {
        [Required(ErrorMessage = "Old password is required.")]
        [StringLength(100, ErrorMessage = "Old password must be between 6 and 100 characters.", MinimumLength = 6)]
        public string OldPassword { get; set; }
        [Required(ErrorMessage = "New password is required.")]
        [StringLength(100, ErrorMessage = "New password must be between 6 and 100 characters.", MinimumLength = 6)]
        public string NewPassword { get; set; }
    }
}
