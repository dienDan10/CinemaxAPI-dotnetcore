using System.ComponentModel.DataAnnotations;

namespace CinemaxAPI.Models.DTO.Requests
{
    public class UpdateProvinceRequestDTO
    {
        [Required(ErrorMessage = "Province ID is required")]
        public int Id { get; set; }
        [Required(ErrorMessage = "Province name is required")]
        public string Name { get; set; }
    }
}
