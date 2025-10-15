using Microsoft.AspNetCore.Mvc;
using System.Text;
using TKMelo.Library.DTOs.Openers;
using TKMelo.Library.Interfaces;

namespace TKMelo.Api.Controllers
{
    [ApiController]
    [Route("api/conversations")]
    public class ConversationsController : ControllerBase
    {
        private readonly IOpenersService _openers;
        private readonly IReplyFromImageService _replyFromImage;

        public ConversationsController(
            IOpenersService openers,
            IReplyFromImageService replyFromImage)
        {
            _openers = openers;
            _replyFromImage = replyFromImage;
        }

        [HttpPost("openers")]
        [ProducesResponseType(typeof(OpenersResponse), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<ActionResult<OpenersResponse>> GenerateOpeners(
            [FromBody] OpenersRequest request,
            CancellationToken ct)
        {
            if (request is null) return BadRequest("Body vacío.");
            if (request.Count is < 1 or > 10) return BadRequest("Count debe estar entre 1 y 10.");

            try
            {
                var result = await _openers.GenerateAsync(request, ct);
                return Ok(result);
            }
            catch (InvalidOperationException ex)
            {
                return Problem(title: "OpenAI error/config", detail: ex.Message);
            }
        }

        [HttpPost("reply-from-image")]
        [Consumes("multipart/form-data")]
        [Produces("application/json")]
        [RequestSizeLimit(20_000_000)]
        [ProducesResponseType(typeof(ReplyFromImageResponse), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<ActionResult<ReplyFromImageResponse>> ReplyFromImage([FromForm] ReplyFromImageForm form, CancellationToken ct = default)
        {
            if (!ModelState.IsValid) return ValidationProblem(ModelState);

            var image = form.Image;
            if (image is null || image.Length == 0)
                return BadRequest("Sube una imagen.");
            if (!image.ContentType.StartsWith("image/", StringComparison.OrdinalIgnoreCase))
                return BadRequest("El archivo debe ser una imagen.");

            try
            {
                await using var ms = new MemoryStream((int)image.Length);
                await image.CopyToAsync(ms, ct);
                var b64 = Convert.ToBase64String(ms.ToArray());
                var dataUrl = $"data:{image.ContentType};base64,{b64}";

                var req = new ReplyFromImageRequest
                {
                    ImageBase64 = dataUrl,
                    Language = form.Language.Trim().ToLowerInvariant(),
                    Tone = form.Tone.Trim().ToLowerInvariant(),
                    Count = form.Count
                };

                var result = await _replyFromImage.GenerateAsync(req, ct);
                return Content(result.BestReply ?? string.Empty, "text/plain", Encoding.UTF8);
            }
            catch (InvalidOperationException ex)
            {
                return Problem(title: "OpenAI error/config", detail: ex.Message);
            }
        }
    }
}
