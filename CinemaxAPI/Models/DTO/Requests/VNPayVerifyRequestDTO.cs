namespace CinemaxAPI.Models.DTO.Requests
{
    public class VNPayVerifyRequestDTO
    {
        public int PaymentId { get; set; }
        public Dictionary<string, string> VnpParams { get; set; }
    }
}
