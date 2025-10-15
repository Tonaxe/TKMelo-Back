using TKMelo.Library.DTOs.Openers;

namespace TKMelo.Library.Interfaces
{
    public interface IReplyFromImageService
    {
        Task<ReplyFromImageResponse> GenerateAsync(ReplyFromImageRequest request, CancellationToken ct = default);
    }
}
