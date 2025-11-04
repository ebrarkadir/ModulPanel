using ModulPanel.Enums;

namespace ModulPanel.DTOs
{
    public class UserUpdateDto
    {
        public string Username { get; set; } = string.Empty;
        public string? Password { get; set; }
        public UserRole Role { get; set; } = UserRole.View;
        public List<string> Permissions { get; set; } = new(); // 🔹 ekledik
    }
}
