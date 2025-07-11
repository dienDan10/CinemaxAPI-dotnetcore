using System.ComponentModel.DataAnnotations;

namespace CinemaxAPI.Models.DTO.Requests
{
    public class ShowTimesFilterRequestDTO
    {
        public int? MovieId { get; set; }
        [Required]
        public int? ScreenId { get; set; }
        public DateTime? StartDate { get; set; } = null;

        public DateTime? EndDate { get; set; } = null;
    }
}
