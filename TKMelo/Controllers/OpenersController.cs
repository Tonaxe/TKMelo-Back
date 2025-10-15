using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using TKMelo.Library.DTOs.Openers;
using TKMelo.Library.Interfaces;

namespace TKMelo.Api.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class OpenersController : ControllerBase
    {
        private readonly IOpenersService _service;
        public OpenersController(IOpenersService service) => _service = service;

        [HttpPost]
        public async Task<ActionResult<OpenersResponse>> Generate([FromBody] OpenersRequest request, CancellationToken ct)
        {
            if (request is null) return BadRequest("Body vacío.");
            if (request.Count is < 1 or > 10) return BadRequest("Count debe estar entre 1 y 10.");

            try
            {
                var result = await _service.GenerateAsync(request, ct);
                return Ok(result);
            }
            catch (InvalidOperationException ex)
            {
                return Problem(ex.Message);
            }
        }
    }
}