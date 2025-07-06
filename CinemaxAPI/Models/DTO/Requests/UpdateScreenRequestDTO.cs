using System.ComponentModel.DataAnnotations;

namespace CinemaxAPI.Models.DTO.Requests
{
    public class UpdateScreenRequestDTO
    {
        [Required(ErrorMessage = "Screen name is required.")]
        public string Name { get; set; }
    }
}
