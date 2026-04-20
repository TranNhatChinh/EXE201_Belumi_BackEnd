using System;
using YourApp.Domain.Entities;

namespace YourApp.Domain.Entities
{
    public class RefreshToken : BaseEntity
    {
        public string TokenHash { get; set; } = string.Empty;

        public Guid UserId { get; set; }
        public User User { get; set; } = null!;

        public DateTime ExpiresAt { get; set; }

        public bool IsRevoked { get; set; }
        public DateTime? RevokedAt { get; set; }

        public Guid? ReplacedByTokenId { get; set; }
        public RefreshToken? ReplacedByToken { get; set; }

        public bool IsExpired => DateTime.UtcNow >= ExpiresAt;
        public bool IsActive => !IsRevoked && !IsExpired;
    }
}
