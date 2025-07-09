namespace CinemaxAPI.Services
{
    public interface IImageService
    {
        Task<string> Upload(IFormFile file);
    }
}
