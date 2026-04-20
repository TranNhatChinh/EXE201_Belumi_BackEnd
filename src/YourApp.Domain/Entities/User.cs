using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using YourApp.Domain.Enums;

namespace YourApp.Domain.Entities
{
    public class User : BaseEntity
    {
        public required string Username { get; set; }
        public required string PasswordHash { get; set; }
        public required string Email { get; set; }
        public bool IsActive { get; set; } = true;
        public string? FullName { get; set; }

        public Role Role { get; set; }
        
        public virtual ICollection<RefreshToken> RefreshTokens { get; set; } = new List<RefreshToken>();
    }
}