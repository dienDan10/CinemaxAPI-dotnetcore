using System.ComponentModel.DataAnnotations;

namespace CinemaxAPI.Models.DTO.Requests
{
    public class BookingRequestDTO
    {
        public string UserId { get; set; }
        [Required]
        public string Username { get; set; }
        [Required]
        public string Phone { get; set; }
        [Required]
        public string Email { get; set; }
        [Required]
        public int TheaterId { get; set; }
        [Required]
        public int ScreenId { get; set; }
        [Required]
        public int ShowtimeId { get; set; }
        [Required]
        public List<BookingRequestConcessionsDTO> Concessions { get; set; } = [];
        public List<BookingRequestSeatsDTO> Seats { get; set; } = [];
    }

    public class BookingRequestSeatsDTO
    {
        public int Id { get; set; }
        public string Name { get; set; }
    }

    public class BookingRequestConcessionsDTO
    {
        public int Id { get; set; }
        public int Quantity { get; set; }
    }
}
