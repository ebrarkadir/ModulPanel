namespace ModulPanel.DTOs
{
    public class AuthResponseDto
    {
        public string AccessToken { get; set; } = string.Empty;
        public string RefreshToken { get; set; } = string.Empty;
        public string Username { get; set; } = string.Empty;
        public string Role { get; set; } = string.Empty;

        // 🔹 yeni alan
        public List<string> Permissions { get; set; } = new();
    }
}
