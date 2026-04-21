using MediatR;
using YourApp.Application.Interfaces.Repositories;
using YourApp.Domain.Exceptions;

namespace YourApp.Application.Features.Auth.VerifyEmail;

public class VerifyEmailCommandHandler : IRequestHandler<VerifyEmailCommand, VerifyEmailResponseDTO>
{
    private readonly IUserRepository _userRepository;

    public VerifyEmailCommandHandler(IUserRepository userRepository)
    {
        _userRepository = userRepository;
    }

    public async Task<VerifyEmailResponseDTO> Handle(VerifyEmailCommand request, CancellationToken cancellationToken)
    {
        var user = await _userRepository.GetByEmailVerificationTokenAsync(request.Token, cancellationToken);

        if (user == null)
            throw new DomainException("Token xác thực không hợp lệ hoặc đã được sử dụng.", "INVALID_TOKEN");

        if (user.IsEmailVerified)
            throw new DomainException("Email này đã được xác thực trước đó.", "EMAIL_ALREADY_VERIFIED");

        if (user.EmailVerificationTokenExpiry < DateTime.UtcNow)
            throw new DomainException("Token xác thực đã hết hạn. Vui lòng yêu cầu gửi lại mã mới.", "TOKEN_EXPIRED");

        // ✅ Xác thực thành công và vô hiệu hóa token
        user.IsEmailVerified = true;
        user.EmailVerificationToken = null;
        user.EmailVerificationTokenExpiry = null;

        await _userRepository.SaveChangesAsync(cancellationToken);

        return new VerifyEmailResponseDTO
        {
            IsEmailVerified = true,
            Message = "Xác thực email thành công."
        };
    }
}
