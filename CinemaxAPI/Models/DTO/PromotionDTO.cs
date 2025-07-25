namespace CinemaxAPI.Models.DTO
{
    public class PromotionDTO
    {
        public int Id { get; set; }
        public string Description { get; set; }
        public string DiscountType { get; set; }
        public int DiscountValue { get; set; }
        public int Quantity { get; set; }
        public int UsedQuantity { get; set; }
        public DateOnly StartDate { get; set; }
        public DateOnly EndDate { get; set; }
        public bool IsActive { get; set; } = true;
    }
}
