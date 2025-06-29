namespace CinemaxAPI.Services
{
    public interface IEmailService
    {
        Task SendEmailAsync(string receiverEmail, string subject, string htmlMessage);

        Task SendEmailWithAttachmentAsync(string receiverEmail, string subject, string htmlMessage, byte[] attachmentBytes, string attachmentName);
    }
}
