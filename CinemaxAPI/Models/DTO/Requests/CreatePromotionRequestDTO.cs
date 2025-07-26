using System.ComponentModel.DataAnnotations;

namespace CinemaxAPI.Models.DTO.Requests
{
    public class CreatePromotionRequestDTO
    {
        [Required]
        public string Description { get; set; }
        [Required]
        public string DiscountType { get; set; }
        [Required]
        [Range(1, int.MaxValue, ErrorMessage = "Discount value must be a positive integer.")]
        public int DiscountValue { get; set; }
        [Required]
        [Range(1, int.MaxValue, ErrorMessage = "Quantity must be a positive integer.")]
        public int Quantity { get; set; }
        [Required]
        public DateOnly StartDate { get; set; }
        [Required]
        public DateOnly EndDate { get; set; }
    }
}
