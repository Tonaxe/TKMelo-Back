using TKMelo.Library.DTOs.Auth;

namespace TKMelo.Library.Interfaces
{
    public interface IUserService
    {
        Task<RegisterResponse> RegisterAsync(RegisterRequest req, CancellationToken ct = default);
        Task<LoginResponse> LoginAsync(LoginRequest req, string? ipAddress, string? userAgent, CancellationToken ct = default);
        Task VerifyEmailAsync(string token, CancellationToken ct = default);
        Task<RefreshResponse> RefreshAsync(RefreshRequest req, CancellationToken ct = default);
        Task LogoutAsync(Guid sessionId, CancellationToken ct = default);
    }
}
