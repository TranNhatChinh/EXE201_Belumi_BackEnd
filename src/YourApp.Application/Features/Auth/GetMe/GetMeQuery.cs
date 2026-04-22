using MediatR;
using YourApp.Application.Interfaces.Repositories;
using YourApp.Application.Common.Exceptions;

namespace YourApp.Application.Features.Auth.GetMe;

public record GetMeQuery(string UserId) : IRequest<GetMeResponseDTO>;

public class GetMeQueryHandler : IRequestHandler<GetMeQuery, GetMeResponseDTO>
{
    private readonly IUserRepository _userRepository;

    public GetMeQueryHandler(IUserRepository userRepository)
    {
        _userRepository = userRepository;
    }

    public async Task<GetMeResponseDTO> Handle(GetMeQuery request, CancellationToken cancellationToken)
    {
        if (!Guid.TryParse(request.UserId, out var userId))
            throw new UnauthorizedException("Token không hợp lệ.");

        var user = await _userRepository.GetByIdAsync(userId, cancellationToken);

        if (user == null || user.IsDeleted)
            throw new UnauthorizedException("Tài khoản không tồn tại.");

        if (!user.IsActive)
            throw new UnauthorizedException("Tài khoản đã bị khóa.");

        return new GetMeResponseDTO
        {
            Id = user.Id,
            Username = user.Username,
            Email = user.Email,
            FullName = user.FullName,
            Role = user.Role.ToString(),
            IsEmailVerified = user.IsEmailVerified
        };
    }
}
