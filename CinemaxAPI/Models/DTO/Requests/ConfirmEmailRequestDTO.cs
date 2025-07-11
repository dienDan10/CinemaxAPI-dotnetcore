using System.ComponentModel.DataAnnotations;

namespace CinemaxAPI.Models.DTO.Requests
{
    public class ConfirmEmailRequestDTO
    {
        [Required]
        public string UserId { get; set; }
        [Required]
        public string Token { get; set; }
    }
}
