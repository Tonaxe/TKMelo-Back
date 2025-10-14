
using Microsoft.AspNetCore.Authorization;
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

        [HttpPost("login")]
        public async Task<ActionResult<LoginResponse>> Login([FromBody] LoginRequest req, CancellationToken ct)
        {
            try
            {
                var ip = HttpContext.Connection.RemoteIpAddress?.ToString();
                var ua = Request.Headers.UserAgent.ToString();
                var res = await _users.LoginAsync(req, ip, ua, ct);
                return Ok(res);
            }
            catch (ArgumentException ex) { return BadRequest(new { error = ex.Message }); }
            catch (UnauthorizedAccessException ex) { return Unauthorized(new { error = ex.Message }); }
        }

        [HttpPost("verify-email")]
        public async Task<IActionResult> VerifyEmail([FromBody] VerifyEmailRequest req, CancellationToken ct)
        {
            try
            {
                await _users.VerifyEmailAsync(req.Token, ct);
                return NoContent();
            }
            catch (ArgumentException ex) { return BadRequest(new { error = ex.Message }); }
            catch (UnauthorizedAccessException) { return Unauthorized(new { error = "Token inválido o expirado" }); }
        }

        [HttpPost("refresh")]
        public async Task<ActionResult<RefreshResponse>> Refresh([FromBody] RefreshRequest req, CancellationToken ct)
        {
            try
            {
                var res = await _users.RefreshAsync(req, ct);
                return Ok(res);
            }
            catch (ArgumentException ex) { return BadRequest(new { error = ex.Message }); }
            catch (UnauthorizedAccessException ex) { return Unauthorized(new { error = ex.Message }); }
        }

        [Authorize]
        [HttpPost("logout")]
        public async Task<IActionResult> Logout(CancellationToken ct)
        {
            try
            {
                var sid = User.FindFirst("sid")?.Value;
                if (string.IsNullOrWhiteSpace(sid)) return Unauthorized(new { error = "Sesión no válida" });

                if (!Guid.TryParse(sid, out var sessionId))
                    return Unauthorized(new { error = "Sesión no válida" });

                await _users.LogoutAsync(sessionId, ct);
                return NoContent();
            }
            catch (Exception ex) { return BadRequest(new { error = ex.Message }); }
        }
    }
}