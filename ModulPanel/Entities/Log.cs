namespace ModulPanel.Entities
{
    public class Log
    {
        public int Id { get; set; }
        public int? UserId { get; set; }  // null olabilir (örneğin başarısız login)
        public string Action { get; set; } = string.Empty;  // Örn: LOGIN, CREATE_USER
        public string Message { get; set; } = string.Empty;
        public string? Module { get; set; }  // 🔹 Örn: "Auth", "User", "ModuleLoader"
        public string IpAddress { get; set; } = string.Empty;  // 🔹 Request'ten çekilecek
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    }
}
