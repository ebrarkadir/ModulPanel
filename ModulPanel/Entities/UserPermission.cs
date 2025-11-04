namespace ModulPanel.Entities
{
    public class UserPermission
    {
        public int Id { get; set; }

        // 🔹 Yetki tipi: modülün key'i veya özel alan (ör: "logs", "users")
        public string PermissionKey { get; set; } = string.Empty;

        // 🔹 İlişki
        public int UserId { get; set; }
        public User User { get; set; } = null!;
    }
}
