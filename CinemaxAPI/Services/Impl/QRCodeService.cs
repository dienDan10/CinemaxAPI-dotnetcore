using QRCoder;

namespace CinemaxAPI.Services.Impl
{
    public class QRCodeService : IQRCodeService
    {
        public string GenerateTextQRCode(string text)
        {
            using var qrGenerator = new QRCodeGenerator();
            using var qrCodeData = qrGenerator.CreateQrCode(text, QRCodeGenerator.ECCLevel.Q);
            using var qrCode = new BitmapByteQRCode(qrCodeData);
            string base64String = Convert.ToBase64String(qrCode.GetGraphic(20));
            return base64String;
        }

        public async Task<byte[]> GenerateQRCodeAsync(string text, int width, int height)
        {
            using var qrGenerator = new QRCodeGenerator();
            using var qrCodeData = qrGenerator.CreateQrCode(text, QRCodeGenerator.ECCLevel.Q);
            using var qrCode = new BitmapByteQRCode(qrCodeData);
            return await Task.FromResult(qrCode.GetGraphic(20));
        }
    }
}
