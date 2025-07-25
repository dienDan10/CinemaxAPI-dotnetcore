namespace CinemaxAPI.Utils
{
    public static class Constants
    {
        public const string Role_Customer = "Customer";
        public const string Role_Admin = "Admin";
        public const string Role_Employee = "Employee";
        public const string Role_Manager = "Manager";

        public const string PaymentStatus_Pending = "Pending";
        public const string PaymentStatus_Success = "Success";
        public const string PaymentStatus_Failed = "Failed";

        public const string BookingStatus_Pending = "Pending";
        public const string BookingStatus_Success = "Success";
        public const string BookingStatus_Failed = "Failed";
        public const string BookingStatus_CheckedIn = "Checked In";


        public const string PaymentMethod_VnPay = "VnPay";
        public const string PaymentMethod_Atm = "ATM";

        public const string SeatType_Normal = "Normal";
        public const string SeatType_Vip = "Vip";

        public const string PromotionDiscountType_Percentage = "Percentage";
        public const string PromotionDiscountType_Amount = "Amount";

        public const int MaxBookingSeats = 5;
    }
}
