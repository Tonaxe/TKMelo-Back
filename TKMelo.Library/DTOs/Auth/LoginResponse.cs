namespace TKMelo.Library.DTOs.Auth
{
    public class LoginResponse
    {
        public Guid UserId { get; init; }
        public string FullName { get; init; } = default!;
        public string Email { get; init; } = default!;

        public string AccessToken { get; init; } = default!;
        public DateTimeOffset AccessTokenExpiresAt { get; init; }

        public string RefreshToken { get; init; } = default!;
        public DateTimeOffset RefreshTokenExpiresAt { get; init; }

        public Guid SessionId { get; init; }
        public DateTimeOffset SessionExpiresAt { get; init; }
        public IEnumerable<string> Roles { get; init; } = Array.Empty<string>();
    }
}
