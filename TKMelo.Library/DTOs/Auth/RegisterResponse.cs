namespace TKMelo.Library.DTOs.Auth
{
    public class RegisterResponse
    {
        public Guid UserId { get; set; }
        public string FullName { get; set; } = null!;
        public string Email { get; set; } = null!;
        public string? EmailVerificationToken { get; set; }
    }
}
