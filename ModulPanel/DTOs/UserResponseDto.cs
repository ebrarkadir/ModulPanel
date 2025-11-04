using ModulPanel.Enums;

namespace ModulPanel.DTOs
{
    public class UserResponseDto
    {
        public int Id { get; set; }
        public string Username { get; set; } = string.Empty;
        public UserRole Role { get; set; }
        public bool IsActive { get; set; }
        public DateTime CreatedAt { get; set; }

        // 🔹 kullanıcıya ait modül anahtarları
        public List<string> Permissions { get; set; } = new();
    }
}
