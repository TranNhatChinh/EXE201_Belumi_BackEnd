using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using YourApp.Domain.Enums;

namespace YourApp.Domain.Entities
{
    /// <summary>
    /// User entity - đã loại bỏ toàn bộ auth fields (PasswordHash, EmailVerification, RefreshTokens).
    /// Firebase xử lý auth. FirebaseUid là key liên kết với Firebase.
    /// </summary>
    public class User : BaseEntity
    {
        public required string FirebaseUid { get; set; }
        public required string Email { get; set; }
        public string? FullName { get; set; }
        public bool IsActive { get; set; } = true;
        public Role Role { get; set; } = Role.User;
    }
}