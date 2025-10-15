using TKMelo.Library.DTOs.Openers;

namespace TKMelo.Library.Interfaces
{
    public interface IOpenersService
    {
        Task<OpenersResponse> GenerateAsync(OpenersRequest request, CancellationToken ct = default);
    }
}
