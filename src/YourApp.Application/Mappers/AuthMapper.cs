using Riok.Mapperly.Abstractions;
using YourApp.Application.Features.Auth.Login;
using YourApp.Application.Features.Auth.Register;
using YourApp.Domain.Entities;

namespace YourApp.Application.Mappers
{
    [Mapper]
    public partial class AuthMapper
    {
        [MapperIgnoreTarget(nameof(User.Id))]
        [MapperIgnoreTarget(nameof(User.IsActive))]
        [MapperIgnoreTarget(nameof(User.CreatedAt))]
        [MapperIgnoreTarget(nameof(User.UpdatedAt))]
        [MapperIgnoreTarget(nameof(User.IsDeleted))]
        [MapperIgnoreTarget(nameof(User.PasswordHash))]
        [MapperIgnoreTarget(nameof(User.Role))]
        public partial User MapToUser(RegisterCommand command);

        public partial RegisterResponseDTO MapToResponse(User user);

        [MapperIgnoreTarget(nameof(LoginResponseDTO.AccessToken))]
        [MapperIgnoreTarget(nameof(LoginResponseDTO.RefreshToken))]
        [MapperIgnoreSource(nameof(User.PasswordHash))]
        [MapperIgnoreSource(nameof(User.IsActive))]
        [MapperIgnoreSource(nameof(User.CreatedAt))]
        [MapperIgnoreSource(nameof(User.UpdatedAt))]
        [MapperIgnoreSource(nameof(User.IsDeleted))]
        public partial LoginResponseDTO MapToLoginResponse(User user);
    }


}
