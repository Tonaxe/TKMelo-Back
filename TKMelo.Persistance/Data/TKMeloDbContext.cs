using Microsoft.EntityFrameworkCore;
using TKMelo.Models.Entities;

namespace TKMelo.Persistance.Data
{
    public class TKMeloDbContext : DbContext
    {
        public TKMeloDbContext(DbContextOptions<TKMeloDbContext> options) : base(options) { }

        public DbSet<User> Users => Set<User>();
        public DbSet<Role> Roles => Set<Role>();
        public DbSet<UserRole> UserRoles => Set<UserRole>();
        public DbSet<Session> Sessions => Set<Session>();
        public DbSet<RefreshToken> RefreshTokens => Set<RefreshToken>();
        public DbSet<EmailVerificationToken> EmailVerificationTokens => Set<EmailVerificationToken>();
        public DbSet<PasswordResetToken> PasswordResetTokens => Set<PasswordResetToken>();

        protected override void OnModelCreating(ModelBuilder b)
        {
            b.Entity<User>(e =>
            {
                e.ToTable("users");
                e.HasKey(x => x.Id);
                e.Property(x => x.Id).HasDefaultValueSql("gen_random_uuid()");
                e.Property(x => x.FullName).IsRequired().HasMaxLength(200);
                e.Property(x => x.Email).IsRequired().HasMaxLength(200);
                e.HasIndex(x => x.Email).IsUnique();
                e.Property(x => x.PasswordHash).IsRequired();
                e.Property(x => x.IsActive).HasDefaultValue(true);
                e.Property(x => x.CreatedAt).HasDefaultValueSql("now()");
                e.Property(x => x.UpdatedAt).HasDefaultValueSql("now()");
            });

            b.Entity<Role>(e =>
            {
                e.ToTable("roles");
                e.HasKey(x => x.Id);
                e.Property(x => x.Id).HasDefaultValueSql("gen_random_uuid()");
                e.Property(x => x.Name).IsRequired().HasMaxLength(100);
                e.HasIndex(x => x.Name).IsUnique();
            });

            b.Entity<UserRole>(e =>
            {
                e.ToTable("user_roles");
                e.HasKey(x => new { x.UserId, x.RoleId });
                e.Property(x => x.AssignedAt).HasDefaultValueSql("now()");
                e.HasOne(x => x.User).WithMany(u => u.UserRoles).HasForeignKey(x => x.UserId).OnDelete(DeleteBehavior.Cascade);
                e.HasOne(x => x.Role).WithMany(r => r.UserRoles).HasForeignKey(x => x.RoleId).OnDelete(DeleteBehavior.Cascade);
            });

            b.Entity<Session>(e =>
            {
                e.ToTable("sessions");
                e.HasKey(x => x.Id);
                e.Property(x => x.Id).HasDefaultValueSql("gen_random_uuid()");
                e.Property(x => x.CreatedAt).HasDefaultValueSql("now()");
                e.HasIndex(x => x.UserId);
                e.Property(x => x.IpAddress).HasColumnType("inet").IsRequired(false);
                e.HasOne(x => x.User).WithMany(u => u.Sessions).HasForeignKey(x => x.UserId).OnDelete(DeleteBehavior.Cascade);
            });

            b.Entity<RefreshToken>(e =>
            {
                e.ToTable("refresh_tokens");
                e.HasKey(x => x.Id);
                e.Property(x => x.Id).HasDefaultValueSql("gen_random_uuid()");
                e.Property(x => x.IssuedAt).HasDefaultValueSql("now()");
                e.Property(x => x.TokenHash).IsRequired();
                e.HasIndex(x => x.TokenHash).IsUnique();
                e.HasOne(x => x.Session).WithMany(s => s.RefreshTokens).HasForeignKey(x => x.SessionId).OnDelete(DeleteBehavior.Cascade);
                e.HasOne(x => x.ReplacedBy).WithMany().HasForeignKey(x => x.ReplacedById).OnDelete(DeleteBehavior.SetNull);
                e.HasIndex(x => x.SessionId);
            });

            b.Entity<EmailVerificationToken>(e =>
            {
                e.ToTable("email_verification_tokens");
                e.HasKey(x => x.Id);
                e.Property(x => x.Id).HasDefaultValueSql("gen_random_uuid()");
                e.Property(x => x.IssuedAt).HasDefaultValueSql("now()");
                e.Property(x => x.TokenHash).IsRequired();
                e.HasIndex(x => x.TokenHash).IsUnique();
                e.HasOne(x => x.User).WithMany(u => u.EmailVerificationTokens).HasForeignKey(x => x.UserId).OnDelete(DeleteBehavior.Cascade);
            });

            b.Entity<PasswordResetToken>(e =>
            {
                e.ToTable("password_reset_tokens");
                e.HasKey(x => x.Id);
                e.Property(x => x.Id).HasDefaultValueSql("gen_random_uuid()");
                e.Property(x => x.IssuedAt).HasDefaultValueSql("now()");
                e.Property(x => x.TokenHash).IsRequired();
                e.HasIndex(x => x.TokenHash).IsUnique();
                e.HasOne(x => x.User).WithMany(u => u.PasswordResetTokens).HasForeignKey(x => x.UserId).OnDelete(DeleteBehavior.Cascade);
            });
        }
    }
}
