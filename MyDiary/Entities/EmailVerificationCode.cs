using System;

namespace Entities
{
    public class EmailVerificationCode
    {
        public int Id { get; set; }

        public string Email { get; set; } = string.Empty;

        public string Code { get; set; } = string.Empty;

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        // Code valid for 60 seconds
        public DateTime ExpiresAt { get; set; } = DateTime.UtcNow.AddSeconds(60);

        public bool IsUsed { get; set; } = false;
    }
}
