namespace ModulPanel.Entities
{
    public class UserPermission
    {
        public int Id { get; set; }
        public int UserId { get; set; }

        public string PageKey { get; set; } = string.Empty; // örn: "reports", "settings"

        public User User { get; set; } = null!;
    }
}
