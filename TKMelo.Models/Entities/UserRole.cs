namespace TKMelo.Models.Entities
{
    public class UserRole
    {
        public Guid UserId { get; set; }
        public Guid RoleId { get; set; }
        public DateTimeOffset AssignedAt { get; set; } = DateTimeOffset.UtcNow;

        public User User { get; set; } = null!;
        public Role Role { get; set; } = null!;
    }
}
