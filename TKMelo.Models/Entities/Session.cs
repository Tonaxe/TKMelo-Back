using System.Net;

namespace TKMelo.Models.Entities
{
    public class Session
    {
        public Guid Id { get; set; }
        public Guid UserId { get; set; }
        public DateTimeOffset CreatedAt { get; set; } = DateTimeOffset.UtcNow;
        public DateTimeOffset ExpiresAt { get; set; }
        public DateTimeOffset? RevokedAt { get; set; }
        public IPAddress? IpAddress { get; set; }
        public string? UserAgent { get; set; }

        public User User { get; set; } = null!;
        public ICollection<RefreshToken> RefreshTokens { get; set; } = new List<RefreshToken>();
    }
}
