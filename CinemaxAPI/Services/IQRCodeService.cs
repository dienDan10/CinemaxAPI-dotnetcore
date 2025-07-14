namespace CinemaxAPI.Services
{
    public interface IQRCodeService
    {
        string GenerateTextQRCode(string text);
        Task<byte[]> GenerateQRCodeAsync(string text, int width, int height);
    }
}
