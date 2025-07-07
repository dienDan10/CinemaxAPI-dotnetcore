using System.ComponentModel.DataAnnotations;

namespace CinemaxAPI.Models.DTO.Requests
{
    public class CreateShowTimeRequestDTO
    {
        [Required]
        public int MovieId { get; set; }
        [Required]
        public int ScreenId { get; set; }
        [Required]
        public DateTime Date { get; set; }
        [Required]
        public TimeSpan StartTime { get; set; }
        [Required]
        public TimeSpan EndTime { get; set; }
        [Required]
        public double TicketPrice { get; set; }
    }
}