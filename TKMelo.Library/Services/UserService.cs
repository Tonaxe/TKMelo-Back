using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Net;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using TKMelo.Library.DTOs.Auth;
using TKMelo.Library.Interfaces;
using TKMelo.Models.Entities;
using TKMelo.Persistance.Data;

namespace TKMelo.Library.Services;

public class UserService : IUserService
{
    private readonly TKMeloDbContext _db;
    private readonly JwtOptions _jwt;
    private readonly IEmailSender _email;
    private readonly IOptions<SmtpOptions> _smtp;

    public UserService(TKMeloDbContext db, IOptions<JwtOptions> jwt, IEmailSender email, IOptions<SmtpOptions> smtp)
    {
        _db = db;
        _jwt = jwt.Value;
        _email = email;
        _smtp = smtp;
    }

    public async Task<RegisterResponse> RegisterAsync(RegisterRequest req, CancellationToken ct = default)
    {
        if (string.IsNullOrWhiteSpace(req.FullName))
            throw new ArgumentException("FullName es requerido");
        if (string.IsNullOrWhiteSpace(req.Email))
            throw new ArgumentException("Email es requerido");
        if (!req.Email.Contains("@"))
            throw new ArgumentException("Email no válido");
        if (string.IsNullOrWhiteSpace(req.Password) || req.Password.Length < 6)
            throw new ArgumentException("Password mínimo 6 caracteres");

        var emailNorm = req.Email.Trim().ToLowerInvariant();
        var exists = await _db.Users.AnyAsync(u => u.Email.ToLower() == emailNorm, ct);
        if (exists)
            throw new InvalidOperationException("El email ya está registrado");

        var passwordHash = BCrypt.Net.BCrypt.HashPassword(req.Password);

        var user = new User
        {
            FullName = req.FullName.Trim(),
            Email = emailNorm,
            PasswordHash = passwordHash,
            IsActive = true,
            CreatedAt = DateTimeOffset.UtcNow,
            UpdatedAt = DateTimeOffset.UtcNow
        };
        await _db.Users.AddAsync(user, ct);

        var role = await _db.Roles.FirstOrDefaultAsync(r => r.Name == "user", ct);
        if (role is null)
        {
            role = new Role { Name = "user", Description = "Default user" };
            await _db.Roles.AddAsync(role, ct);
        }

        await _db.SaveChangesAsync(ct);

        var userRole = new UserRole
        {
            UserId = user.Id,
            RoleId = role.Id,
            AssignedAt = DateTimeOffset.UtcNow
        };
        await _db.UserRoles.AddAsync(userRole, ct);

        var plainToken = GenerateRandomToken(32);
        var tokenHash = Sha256(plainToken);

        var emailToken = new EmailVerificationToken
        {
            UserId = user.Id,
            TokenHash = tokenHash,
            IssuedAt = DateTimeOffset.UtcNow,
            ExpiresAt = DateTimeOffset.UtcNow.AddDays(1)
        };
        await _db.EmailVerificationTokens.AddAsync(emailToken, ct);

        await _db.SaveChangesAsync(ct);

        var baseUrl = _smtp.Value.FrontendBaseUrl.TrimEnd('/');
        var verifyUrl = $"{baseUrl}/verificar-correo?token={plainToken}";

        var html = $@"
                    <!doctype html>
                    <html>
                      <body style=""font-family:system-ui,Segoe UI,Roboto,Arial,sans-serif;background:#f6f7fb;padding:24px;"">
                        <table role=""presentation"" cellpadding=""0"" cellspacing=""0"" width=""100%"" style=""max-width:560px;margin:auto;background:#fff;border-radius:12px;overflow:hidden"">
                          <tr><td style=""padding:24px 24px 0"">
                            <h1 style=""margin:0;font-size:20px;color:#111827;"">Confirma tu email</h1>
                            <p style=""color:#374151;font-size:14px;line-height:1.5;margin-top:8px"">
                              Hola {System.Net.WebUtility.HtmlEncode(user.FullName)},<br/>
                              para activar tu cuenta en <strong>TKMelo</strong> haz clic aquí:
                            </p>
                            <p style=""margin:24px 0"">
                              <a href=""{verifyUrl}"" style=""display:inline-block;background:#4f46e5;color:#fff;text-decoration:none;padding:12px 16px;border-radius:8px;font-weight:600"">
                                Verificar mi email
                              </a>
                            </p>
                            <p style=""color:#6b7280;font-size:12px"">Si no funciona, copia este enlace:<br/>{verifyUrl}</p>
                            <p style=""color:#9ca3af;font-size:12px;margin-top:24px"">El enlace caduca en 24 horas.</p>
                          </td></tr>
                        </table>
                      </body>
                    </html>";

        await _email.SendAsync(user.Email, "Verifica tu cuenta en TKMelo", html, ct);

        return new RegisterResponse
        {
            UserId = user.Id,
            FullName = user.FullName,
            Email = user.Email,
            EmailVerificationToken = plainToken
        };
    }

    public async Task<LoginResponse> LoginAsync(LoginRequest req, string? ipAddress, string? userAgent, CancellationToken ct = default)
    {
        if (string.IsNullOrWhiteSpace(req.Email) || string.IsNullOrWhiteSpace(req.Password))
            throw new ArgumentException("Email y password son requeridos");

        var emailNorm = req.Email.Trim().ToLowerInvariant();

        var user = await _db.Users
            .Include(u => u.UserRoles).ThenInclude(ur => ur.Role)
            .FirstOrDefaultAsync(u => u.Email.ToLower() == emailNorm, ct);

        if (user is null || !user.IsActive || !BCrypt.Net.BCrypt.Verify(req.Password, user.PasswordHash))
            throw new UnauthorizedAccessException("Credenciales inválidas");

        user.LastLoginAt = DateTimeOffset.UtcNow;

        IPAddress? ipParsed = null;
        if (!string.IsNullOrWhiteSpace(ipAddress) && IPAddress.TryParse(ipAddress, out var parsed))
            ipParsed = parsed;

        var session = new Session
        {
            UserId = user.Id,
            CreatedAt = DateTimeOffset.UtcNow,
            ExpiresAt = DateTimeOffset.UtcNow.AddDays(_jwt.SessionDays),
            IpAddress = ipParsed,
            UserAgent = userAgent
        };
        await _db.Sessions.AddAsync(session, ct);

        await _db.SaveChangesAsync(ct);

        var roles = user.UserRoles.Select(ur => ur.Role.Name).ToArray();
        var (accessToken, accessExp) = CreateJwt(user, roles, session.Id);

        var plainRefresh = GenerateRandomToken(64);
        var refresh = new RefreshToken
        {
            SessionId = session.Id,
            TokenHash = Sha256(plainRefresh),
            IssuedAt = DateTimeOffset.UtcNow,
            ExpiresAt = DateTimeOffset.UtcNow.AddDays(_jwt.RefreshTokenDays)
        };
        await _db.RefreshTokens.AddAsync(refresh, ct);

        user.LastLoginAt = DateTimeOffset.UtcNow;
        await _db.SaveChangesAsync(ct);

        return new LoginResponse
        {
            UserId = user.Id,
            FullName = user.FullName,
            Email = user.Email,
            Roles = roles,

            AccessToken = accessToken,
            AccessTokenExpiresAt = accessExp,

            RefreshToken = plainRefresh,
            RefreshTokenExpiresAt = refresh.ExpiresAt,

            SessionId = session.Id,
            SessionExpiresAt = session.ExpiresAt
        };
    }

    public async Task VerifyEmailAsync(string token, CancellationToken ct = default)
    {
        if (string.IsNullOrWhiteSpace(token))
            throw new ArgumentException("Token requerido");

        var hash = Sha256(token);

        var row = await _db.EmailVerificationTokens
            .Include(t => t.User)
            .FirstOrDefaultAsync(t =>
                t.TokenHash == hash &&
                t.UsedAt == null &&
                t.ExpiresAt > DateTimeOffset.UtcNow, ct);

        if (row is null)
            throw new UnauthorizedAccessException("Token inválido o expirado");

        row.UsedAt = DateTimeOffset.UtcNow;
        row.User.EmailVerifiedAt = DateTimeOffset.UtcNow;

        await _db.SaveChangesAsync(ct);
    }

    public async Task<RefreshResponse> RefreshAsync(RefreshRequest req, CancellationToken ct = default)
    {
        if (string.IsNullOrWhiteSpace(req.RefreshToken))
            throw new ArgumentException("Refresh token requerido");

        var hash = Sha256(req.RefreshToken);

        var token = await _db.RefreshTokens
            .Include(rt => rt.Session)
                .ThenInclude(s => s!.User)
                    .ThenInclude(u => u.UserRoles)
                        .ThenInclude(ur => ur.Role)
            .FirstOrDefaultAsync(rt => rt.TokenHash == hash, ct);

        if (token is null)
            throw new UnauthorizedAccessException("Token inválido");

        if (token.RevokedAt != null || token.ExpiresAt <= DateTimeOffset.UtcNow)
            throw new UnauthorizedAccessException("Token revocado o expirado");

        var session = token.Session!;
        if (session.RevokedAt != null || session.ExpiresAt <= DateTimeOffset.UtcNow)
            throw new UnauthorizedAccessException("Sesión revocada o expirada");

        if (token.ReplacedById != null)
        {
            session.RevokedAt = DateTimeOffset.UtcNow;

            var sessionTokens = await _db.RefreshTokens
                .Where(x => x.SessionId == session.Id && x.RevokedAt == null)
                .ToListAsync(ct);

            var now = DateTimeOffset.UtcNow;
            foreach (var t in sessionTokens) t.RevokedAt = now;

            await _db.SaveChangesAsync(ct);
            throw new UnauthorizedAccessException("Refresh token reutilizado; sesión revocada");
        }

        var user = session.User!;
        var roles = user.UserRoles.Select(r => r.Role.Name).ToArray();

        var (accessToken, accessExp) = CreateJwt(user, roles, session.Id);

        var newPlainRefresh = GenerateRandomToken(64);
        var newRefresh = new RefreshToken
        {
            SessionId = session.Id,
            TokenHash = Sha256(newPlainRefresh),
            IssuedAt = DateTimeOffset.UtcNow,
            ExpiresAt = DateTimeOffset.UtcNow.AddDays(_jwt.RefreshTokenDays)
        };
        await _db.RefreshTokens.AddAsync(newRefresh, ct);

        token.ReplacedById = newRefresh.Id;
        token.RevokedAt = DateTimeOffset.UtcNow;

        await _db.SaveChangesAsync(ct);

        return new RefreshResponse
        {
            AccessToken = accessToken,
            AccessTokenExpiresAt = accessExp,
            RefreshToken = newPlainRefresh,
            RefreshTokenExpiresAt = newRefresh.ExpiresAt
        };
    }

    public async Task LogoutAsync(Guid sessionId, CancellationToken ct = default)
    {
        var session = await _db.Sessions
            .Include(s => s.RefreshTokens)
            .FirstOrDefaultAsync(s => s.Id == sessionId, ct);

        if (session is null) return;

        var now = DateTimeOffset.UtcNow;
        session.RevokedAt = now;

        foreach (var rt in session.RefreshTokens.Where(r => r.RevokedAt == null))
            rt.RevokedAt = now;

        await _db.SaveChangesAsync(ct);
    }

    private (string token, DateTimeOffset exp) CreateJwt(User user, IEnumerable<string> roles, Guid sessionId)
    {
        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_jwt.Key));
        var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

        var now = DateTimeOffset.UtcNow;
        var exp = now.AddMinutes(_jwt.AccessTokenMinutes);

        var claims = new List<Claim>
            {
                new(JwtRegisteredClaimNames.Sub, user.Id.ToString()),
                new(JwtRegisteredClaimNames.Email, user.Email),
                new(JwtRegisteredClaimNames.Name, user.FullName),
                new(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
                new("sid", sessionId.ToString())
            };
        claims.AddRange(roles.Select(r => new Claim(ClaimTypes.Role, r)));

        var jwt = new JwtSecurityToken(
            issuer: _jwt.Issuer,
            audience: _jwt.Audience,
            claims: claims,
            notBefore: now.UtcDateTime,
            expires: exp.UtcDateTime,
            signingCredentials: creds
        );

        var token = new JwtSecurityTokenHandler().WriteToken(jwt);
        return (token, exp);
    }

    private static string GenerateRandomToken(int bytesLen)
    {
        var bytes = RandomNumberGenerator.GetBytes(bytesLen);
        return Convert.ToBase64String(bytes)
            .Replace("+", "-").Replace("/", "_").TrimEnd('=');
    }

    private static string Sha256(string input)
    {
        var bytes = SHA256.HashData(Encoding.UTF8.GetBytes(input));
        return Convert.ToHexString(bytes).ToLowerInvariant();
    }
}
