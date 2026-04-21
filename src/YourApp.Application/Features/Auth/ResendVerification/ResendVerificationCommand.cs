using MediatR;

namespace YourApp.Application.Features.Auth.ResendVerification;

public record ResendVerificationCommand(string Email) : IRequest<ResendVerificationResponseDTO>;

public class ResendVerificationResponseDTO
{
    public string Message { get; set; } = string.Empty;
}
