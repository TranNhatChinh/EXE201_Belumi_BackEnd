    using System;
using System.Threading;
using System.Threading.Tasks;
using MediatR;
using Microsoft.AspNetCore.Identity;
using YourApp.Application.Common.Exceptions;
using YourApp.Application.Interfaces.Repositories;
using YourApp.Application.Mappers;
using YourApp.Domain.Entities;

namespace YourApp.Application.Features.Auth.Register
{
    public class RegisterCommandHandler : IRequestHandler<RegisterCommand, RegisterResponseDTO>
    {
        private readonly IUserRepository _userRepository;
        private readonly IPasswordHasher<User> _passwordHasher;
        private readonly AuthMapper _authMapper;            
        public RegisterCommandHandler(IUserRepository userRepository, IPasswordHasher<User> passwordHasher, AuthMapper authMapper)
        {
            _userRepository = userRepository;
            _passwordHasher = passwordHasher;
            _authMapper = authMapper;
        }

        public async Task<RegisterResponseDTO> Handle(RegisterCommand request, CancellationToken cancellationToken)
        {
            // 1. Check if email exists
            var existingByEmail = await _userRepository.GetByEmailAsync(request.Email, cancellationToken);
            if (existingByEmail != null)
            {
                throw new ConflictException("Email này đã được sử dụng bởi một tài khoản khác.");
            }

            // 2. Check if username exists
            var existingByUsername = await _userRepository.GetByUsernameAsync(request.Username, cancellationToken);
            if (existingByUsername != null)
            {
                throw new ConflictException("Tên đăng nhập này đã tồn tại trên hệ thống.");
            }

            // 3. Map to Entity
            var user = _authMapper.MapToUser(request);

            // 4. Hash Password (pass user object for password hashing context)
            user.PasswordHash = _passwordHasher.HashPassword(user, request.Password);

            // 5. Save to database
            await _userRepository.AddAsync(user, cancellationToken);

            return _authMapper.MapToResponse(user); 
        }
    }
}
