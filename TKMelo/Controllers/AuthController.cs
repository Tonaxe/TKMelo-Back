
using Microsoft.AspNetCore.Mvc;
using TKMelo.Library.DTOs.Auth;
using TKMelo.Library.Interfaces;

namespace TKMelo.Api.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class AuthController : ControllerBase
    {
        private readonly IUserService _users;

        public AuthController(IUserService users) => _users = users;

        [HttpPost("register")]
        public async Task<ActionResult<RegisterResponse>> Register([FromBody] RegisterRequest req, CancellationToken ct)
        {
            try
            {
                var res = await _users.RegisterAsync(req, ct);
                return CreatedAtAction(nameof(Register), new { id = res.UserId }, res);
            }
            catch (ArgumentException ex) { return BadRequest(new { error = ex.Message }); }
            catch (InvalidOperationException ex) { return Conflict(new { error = ex.Message }); }
        }
    }
}