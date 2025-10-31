using ModulPanel.Enums;

namespace ModulPanel.Entities
{
    public class User
    {
        public int Id { get; set; }
        public string Username { get; set; } = string.Empty;
        public string PasswordHash { get; set; } = string.Empty;
        public UserRole Role { get; set; } = UserRole.View;
        public bool IsActive { get; set; } = true;
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        // 🔹 Token alanları
        public string? RefreshToken { get; set; }
        public DateTime? RefreshTokenExpiry { get; set; }

        // 🔹 UserPermission ilişkisi
        public ICollection<UserPermission> Permissions { get; set; } = new List<UserPermission>();
    }
}
