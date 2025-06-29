using Microsoft.AspNetCore.Identity;

namespace CinemaxAPI.Services
{
    public interface ITokenService
    {
        string CreateJwtToken(IdentityUser user, List<string> roles);
    }
}
