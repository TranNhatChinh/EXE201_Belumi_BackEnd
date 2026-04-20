using System.Security.Cryptography;
using System.Text;
using MediatR;
using YourApp.Application.Common.Interfaces;
using YourApp.Application.Interfaces.Repositories;
using YourApp.Application.Common.Exceptions;

namespace YourApp.Application.Features.Auth.Refresh;

public record RefreshCommand(string RefreshToken) : IRequest<RefreshResponseDTO>;

public class RefreshCommandHandler : IRequestHandler<RefreshCommand, RefreshResponseDTO>
{
    private readonly IUserRepository _userRepository;
    private readonly IJwtTokenGenerator _jwtTokenGenerator;

    public RefreshCommandHandler(
        IUserRepository userRepository,
        IJwtTokenGenerator jwtTokenGenerator)
    {
        _userRepository = userRepository;
        _jwtTokenGenerator = jwtTokenGenerator;
    }

    public async Task<RefreshResponseDTO> Handle(RefreshCommand request, CancellationToken cancellationToken)
    {
        // 1. Hash Refresh Token nhận được từ FE để so khớp
        var tokenHash = Convert.ToBase64String(SHA256.HashData(Encoding.UTF8.GetBytes(request.RefreshToken)));

        // 2. Tìm User đang sở hữu Token Hash này
        var user = await _userRepository.GetByUserByRefreshTokenHashAsync(tokenHash, cancellationToken);

        if (user == null)
            throw new UnauthorizedException("Invalid refresh token");

        // 3. Tìm chính xác Token entity trong collection của User để kiểm tra trạng thái
        var refreshToken = user.RefreshTokens.FirstOrDefault(t => t.TokenHash == tokenHash);

        if (refreshToken == null || !refreshToken.IsActive)
            throw new UnauthorizedException("Invalid or expired refresh token");

        // 4. Tạo Access Token mới
        var newAccessToken = _jwtTokenGenerator.GenerateToken(user);

        return new RefreshResponseDTO
        {
            AccessToken = newAccessToken,
            RefreshToken = request.RefreshToken // Trả lại chính Token cũ vì không dùng Rotation
        };
    }
}
