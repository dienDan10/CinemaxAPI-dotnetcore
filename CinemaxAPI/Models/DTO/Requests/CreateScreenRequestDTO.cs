using System.ComponentModel.DataAnnotations;

namespace CinemaxAPI.Models.DTO.Requests
{
    public class CreateScreenRequestDTO
    {
        [Required(ErrorMessage = "Screen name is required.")]
        public string Name { get; set; }
        [Required(ErrorMessage = "Theater ID is required.")]
        public int TheaterId { get; set; }
        [Range(1, 100, ErrorMessage = "Rows must be be between 1 and 100.")]
        public int Rows { get; set; }
        [Range(1, 100, ErrorMessage = "Rows must be be between 1 and 100.")]
        public int Columns { get; set; }
    }
}
