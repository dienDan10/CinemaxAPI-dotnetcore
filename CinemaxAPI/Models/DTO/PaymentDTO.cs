﻿namespace CinemaxAPI.Models.DTO
{
    public class PaymentDTO
    {
        public int Id { get; set; }
        public string? UserId { get; set; }
        public string? EmployeeId { get; set; }
        public string? Email { get; set; }
        public string? Name { get; set; }
        public string? PhoneNumber { get; set; }
        public decimal TotalAmount { get; set; }
        public decimal? DiscountAmount { get; set; }
        public decimal FinalAmount { get; set; }
        public int PromotionId { get; set; }
        public decimal? BonusPointsUsed { get; set; }
        public string? PaymentMethod { get; set; }
        public DateTime PaymentDate { get; set; }
        public string? PaymentStatus { get; set; }
    }
}
