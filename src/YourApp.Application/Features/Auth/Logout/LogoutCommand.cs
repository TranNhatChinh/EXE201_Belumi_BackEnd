using System.Security.Cryptography;
using System.Text;
using MediatR;
using YourApp.Application.Common.Interfaces;
using YourApp.Application.Interfaces.Repositories;
using YourApp.Application.Common.Exceptions;

namespace YourApp.Application.Features.Auth.Logout;

public record LogoutCommand(string RefreshToken) : IRequest;

public class LogoutCommandHandler : IRequestHandler<LogoutCommand>
{
    private readonly IUserRepository _userRepository;
    private readonly ICurrentUser _currentUser;

    public LogoutCommandHandler(IUserRepository userRepository, ICurrentUser currentUser)
    {
        _userRepository = userRepository;
        _currentUser = currentUser;
    }

    public async Task Handle(LogoutCommand request, CancellationToken cancellationToken)
    {
        // 1. Hash Refresh Token để so khớp
        var tokenHash = Convert.ToBase64String(SHA256.HashData(Encoding.UTF8.GetBytes(request.RefreshToken)));

        // 2. Tìm User sở hữu Token này
        var user = await _userRepository.GetByUserByRefreshTokenHashAsync(tokenHash, cancellationToken);

        // Security Check: Token phải tồn tại và thuộc về User đang đăng nhập
        if (user == null || user.Id.ToString() != _currentUser.Id)
        {
            throw new UnauthorizedException("Invalid logout request");
        }

        // 3. Tìm Token và revoke nó
        var refreshToken = user.RefreshTokens.FirstOrDefault(t => t.TokenHash == tokenHash);
        if (refreshToken != null)
        {
            refreshToken.IsRevoked = true;
            refreshToken.RevokedAt = DateTime.UtcNow;
            
            await _userRepository.SaveChangesAsync(cancellationToken);
        }
    }
}
