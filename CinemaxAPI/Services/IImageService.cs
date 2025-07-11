using Microsoft.AspNetCore.Http;
using System.Threading.Tasks;

namespace CinemaxAPI.Services
{
    public interface IImageService
    {
        Task<string> Upload(IFormFile file);
        bool ValidateImage(IFormFile file, out string? errorMsg);
    }
}
