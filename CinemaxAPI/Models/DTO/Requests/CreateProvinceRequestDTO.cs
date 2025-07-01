using System.ComponentModel.DataAnnotations;

namespace CinemaxAPI.Models.DTO.Requests
{
    public class CreateProvinceRequestDTO
    {
        [Required(ErrorMessage = "Province name is required.")]
        public string Name { get; set; }
    }
}
