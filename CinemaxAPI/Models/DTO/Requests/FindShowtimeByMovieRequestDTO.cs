using System.ComponentModel.DataAnnotations;

namespace CinemaxAPI.Models.DTO.Requests
{
    public class FindShowtimeByMovieRequestDTO
    {
        [Required]
        public int MovieId { get; set; }
        [Required]
        public int ProvinceId { get; set; }
        [Required]
        public string Date { get; set; }
    }
}
