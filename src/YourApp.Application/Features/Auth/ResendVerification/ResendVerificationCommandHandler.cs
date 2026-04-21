using System.Security.Cryptography;
using MediatR;
using YourApp.Application.Common.Interfaces;
using YourApp.Application.Interfaces.Repositories;

namespace YourApp.Application.Features.Auth.ResendVerification;

public class ResendVerificationCommandHandler : IRequestHandler<ResendVerificationCommand, ResendVerificationResponseDTO>
{
    private readonly IUserRepository _userRepository;
    private readonly IMailService _mailService;

    public ResendVerificationCommandHandler(IUserRepository userRepository, IMailService mailService)
    {
        _userRepository = userRepository;
        _mailService = mailService;
    }

    public async Task<ResendVerificationResponseDTO> Handle(ResendVerificationCommand request, CancellationToken cancellationToken)
    {
        var user = await _userRepository.GetByEmailAsync(request.Email, cancellationToken);

        // Bảo mật: Không tiết lộ email có tồn tại hay không hoặc đã xác thực chưa
        if (user == null || user.IsEmailVerified)
        {
            return new ResendVerificationResponseDTO
            {
                Message = "Nếu email của bạn tồn tại trong hệ thống và chưa được xác thực, chúng tôi đã gửi lại mã mới."
            };
        }

        // ✅ Ghi đè token mới
        user.EmailVerificationToken = Convert.ToBase64String(RandomNumberGenerator.GetBytes(64));
        user.EmailVerificationTokenExpiry = DateTime.UtcNow.AddHours(24);

        await _userRepository.SaveChangesAsync(cancellationToken);
        await _mailService.SendVerificationAsync(user, user.EmailVerificationToken);

        return new ResendVerificationResponseDTO
        {
            Message = "Nếu email của bạn tồn tại trong hệ thống và chưa được xác thực, chúng tôi đã gửi lại mã mới."
        };
    }
}
