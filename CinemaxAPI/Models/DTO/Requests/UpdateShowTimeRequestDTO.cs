using System.ComponentModel.DataAnnotations;

namespace CinemaxAPI.Models.DTO.Requests
{
    public class UpdateShowTimeRequestDTO
    {
        [Required]
        public TimeSpan StartTime { get; set; }
        [Required]
        public TimeSpan EndTime { get; set; }
        [Required]
        public decimal TicketPrice { get; set; }
        [Required]
        public decimal VipTicketPrice { get; set; }
    }
}