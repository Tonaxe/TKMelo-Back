using Microsoft.EntityFrameworkCore;
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

    public UserService(TKMeloDbContext db) => _db = db;

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

        return new RegisterResponse
        {
            UserId = user.Id,
            FullName = user.FullName,
            Email = user.Email,
            EmailVerificationToken = plainToken
        };
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
