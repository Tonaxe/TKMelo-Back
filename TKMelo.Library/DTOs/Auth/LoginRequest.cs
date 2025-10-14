namespace TKMelo.Library.DTOs.Auth
{
    public class LoginRequest
    {
        public string Email { get; init; } = default!;
        public string Password { get; init; } = default!;
    }
}
