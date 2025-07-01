using System.ComponentModel.DataAnnotations;

namespace CinemaxAPI.Models.DTO.Requests
{
    public class UpdateManagerRequestDTO
    {
        [Required]
        public string Username { get; set; }
        [Required]
        public string Email { get; set; }
        public string PhoneNumber { get; set; }

        [Required]
        public int TheaterId { get; set; }
    }
}
