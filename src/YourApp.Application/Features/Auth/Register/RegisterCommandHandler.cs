using System.Security.Cryptography;
using MediatR;
using Microsoft.AspNetCore.Identity;
using YourApp.Application.Common.Exceptions;
using YourApp.Application.Common.Interfaces;
using YourApp.Application.Interfaces.Repositories;
using YourApp.Application.Mappers;
using YourApp.Domain.Entities;
using YourApp.Domain.Enums;

namespace YourApp.Application.Features.Auth.Register
{
    public class RegisterCommandHandler : IRequestHandler<RegisterCommand, RegisterResponseDTO>
    {
        private readonly IUserRepository _userRepository;
        private readonly IPasswordHasher<User> _passwordHasher;
        private readonly AuthMapper _authMapper;
        private readonly IMailService _mailService;

        public RegisterCommandHandler(
            IUserRepository userRepository,
            IPasswordHasher<User> passwordHasher,
            AuthMapper authMapper,
            IMailService mailService)
        {
            _userRepository = userRepository;
            _passwordHasher = passwordHasher;
            _authMapper = authMapper;
            _mailService = mailService;
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

            // 4. Hash Password
            user.PasswordHash = _passwordHasher.HashPassword(user, request.Password);

            // 5. Generate Email Verification Token
            user.EmailVerificationToken = Convert.ToBase64String(RandomNumberGenerator.GetBytes(64));
            user.EmailVerificationTokenExpiry = DateTime.UtcNow.AddHours(24);
            user.IsEmailVerified = false;
            user.Role = Role.User; // Gán quyền mặc định

            // 6. Save to database
            await _userRepository.AddAsync(user, cancellationToken);

            // 7. Send Verification Email
            _ = Task.Run(async () =>
            {
                try
                {
                    await _mailService.SendVerificationAsync(user, user.EmailVerificationToken);
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"[Email Error] {ex.Message}");
                }
            });
            return _authMapper.MapToResponse(user);
        }
    }
}
