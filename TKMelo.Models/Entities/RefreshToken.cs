namespace TKMelo.Models.Entities
{
    public class RefreshToken
    {
        public Guid Id { get; set; }
        public Guid SessionId { get; set; }
        public string TokenHash { get; set; } = null!;
        public DateTimeOffset IssuedAt { get; set; } = DateTimeOffset.UtcNow;
        public DateTimeOffset ExpiresAt { get; set; }
        public DateTimeOffset? RevokedAt { get; set; }
        public Guid? ReplacedById { get; set; }

        public Session Session { get; set; } = null!;
        public RefreshToken? ReplacedBy { get; set; }
    }
}
