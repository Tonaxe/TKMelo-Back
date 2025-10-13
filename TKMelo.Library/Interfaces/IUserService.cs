using TKMelo.Library.DTOs.Auth;

namespace TKMelo.Library.Interfaces
{
    public interface IUserService
    {
        Task<RegisterResponse> RegisterAsync(RegisterRequest req, CancellationToken ct = default);
    }
}
