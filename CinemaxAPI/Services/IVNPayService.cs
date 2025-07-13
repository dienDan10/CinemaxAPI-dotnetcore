namespace CinemaxAPI.Services
{
    public interface IVNPayService
    {
        string CreatePaymentUrl(decimal amount, string orderInfo, string ipAddress, string returnUrl, string tmnCode, string hashSecret, string baseUrl);

        string HmacSha512(string key, string inputData);

        bool ValidateResponse(SortedList<string, string> responseData, string hashSecret);
    }
}
