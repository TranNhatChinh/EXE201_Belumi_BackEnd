using System;
using MediatR;

namespace YourApp.Application.Features.Auth.Register
{
    public class RegisterCommand : IRequest<RegisterResponseDTO>
    {
        public required string Username { get; set; }
        public required string Email { get; set; }
        public required string Password { get; set; }
        public string? FullName { get; set; }
    }
}
