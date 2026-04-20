using System.Security.Cryptography;
using System.Text;
using MediatR;
using Microsoft.AspNetCore.Identity;
using YourApp.Application.Common.Interfaces;
using YourApp.Application.Interfaces.Repositories;
using YourApp.Domain.Entities;
using YourApp.Application.Mappers;
using YourApp.Application.Common.Exceptions;

namespace YourApp.Application.Features.Auth.Login;

public record LoginCommand(string Username, string Password) : IRequest<LoginResponseDTO>;

public class LoginCommandHandler : IRequestHandler<LoginCommand, LoginResponseDTO>
{
    private readonly IUserRepository _userRepository;
    private readonly IJwtTokenGenerator _jwtTokenGenerator;
    private readonly IPasswordHasher<User> _passwordHasher;
    private readonly AuthMapper _authMapper;

    public LoginCommandHandler(
        IUserRepository userRepository,
        IJwtTokenGenerator jwtTokenGenerator,
        IPasswordHasher<User> passwordHasher,
        AuthMapper authMapper)
    {
        _userRepository = userRepository;
        _jwtTokenGenerator = jwtTokenGenerator;
        _passwordHasher = passwordHasher;
        _authMapper = authMapper;
    }

    public async Task<LoginResponseDTO> Handle(LoginCommand request, CancellationToken cancellationToken)
    {
        var user = await _userRepository.GetByUsernameAsync(request.Username, cancellationToken);
        
        if (user == null)
            throw new UnauthorizedException("Invalid credentials");

        var result = _passwordHasher.VerifyHashedPassword(user, user.PasswordHash, request.Password);
        
        if (result == PasswordVerificationResult.Failed)
            throw new UnauthorizedException("Invalid credentials");

        var accessToken = _jwtTokenGenerator.GenerateToken(user);
        var refreshToken = _jwtTokenGenerator.GenerateRefreshToken();

        // Hash Refresh Token trước khi lưu
        var tokenHash = Convert.ToBase64String(SHA256.HashData(Encoding.UTF8.GetBytes(refreshToken)));

        var refreshTokenEntity = new RefreshToken
        {
            Id = Guid.NewGuid(),
            UserId = user.Id,
            TokenHash = tokenHash,
            ExpiresAt = DateTime.UtcNow.AddDays(7),
            CreatedAt = DateTime.UtcNow
        };

        await _userRepository.AddRefreshTokenAsync(refreshTokenEntity, cancellationToken);
        await _userRepository.SaveChangesAsync(cancellationToken);

        // Sử dụng Mapperly để map thông tin User
        var response = _authMapper.MapToLoginResponse(user);
        response.AccessToken = accessToken;
        response.RefreshToken = refreshToken;

        return response;
    }
}
