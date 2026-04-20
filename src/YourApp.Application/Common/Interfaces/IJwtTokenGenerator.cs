using YourApp.Domain.Entities;

namespace YourApp.Application.Common.Interfaces
{
    public interface IJwtTokenGenerator
    {
        string GenerateToken(User user);
        string GenerateRefreshToken();
    }
}
