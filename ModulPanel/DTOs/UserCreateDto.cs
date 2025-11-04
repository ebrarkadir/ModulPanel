using ModulPanel.Enums;

namespace ModulPanel.DTOs
{
    public class UserCreateDto
    {
        public string Username { get; set; } = string.Empty;
        public string Password { get; set; } = string.Empty;
        public UserRole Role { get; set; } = UserRole.View;
        public List<string> Permissions { get; set; } = new(); // 🔹 ekledik
    }
}
