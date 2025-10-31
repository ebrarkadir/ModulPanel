using ModulPanel.Enums;

namespace ModulPanel.DTOs
{
    public class UserResponseDto
    {
        public int Id { get; set; }
        public string Username { get; set; } = string.Empty;
        public UserRole Role { get; set; } // ✅ Enum olacak, string değil
        public bool IsActive { get; set; }
        public DateTime CreatedAt { get; set; }
    }
}
