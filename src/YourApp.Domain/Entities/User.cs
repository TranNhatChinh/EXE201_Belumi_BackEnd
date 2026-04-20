using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace YourApp.Domain.Entities
{
    public class User : BaseEntity
    {
        public required string Username { get; set; }
        public required string PasswordHash { get; set; }
        public required string Email { get; set; }
        public bool IsActive { get; set; } = true;
        public string? FullName { get; set; }

    }
}