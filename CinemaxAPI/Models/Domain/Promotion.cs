using System.ComponentModel.DataAnnotations;

namespace CinemaxAPI.Models.Domain
{
    public class Promotion
    {
        [Key]
        public int Id { get; set; }
        public string Description { get; set; }
        public string DiscountType { get; set; }
        public int DiscountValue { get; set; }
        public int Quantity { get; set; }
        public int UsedQuantity { get; set; } = 0;
        public DateOnly StartDate { get; set; }
        public DateOnly EndDate { get; set; }
        public bool IsActive { get; set; } = true;
        public DateTime CreatedAt { get; set; } = DateTime.Now;
        public DateTime LastUpdatedAt { get; set; } = DateTime.Now;
    }
}
