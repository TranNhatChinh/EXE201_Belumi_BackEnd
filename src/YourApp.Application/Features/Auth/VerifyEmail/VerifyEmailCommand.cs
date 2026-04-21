using MediatR;

namespace YourApp.Application.Features.Auth.VerifyEmail;

public record VerifyEmailCommand(string Token) : IRequest<VerifyEmailResponseDTO>;

public class VerifyEmailResponseDTO
{
    public bool IsEmailVerified { get; set; }
    public string Message { get; set; } = string.Empty;
}
