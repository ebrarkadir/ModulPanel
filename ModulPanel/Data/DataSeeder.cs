using System.Security.Cryptography;
using System.Text;
using ModulPanel.Entities;
using ModulPanel.Enums;

namespace ModulPanel.Data
{
    public static class DataSeeder
    {
        public static void SeedAdmin(AppDbContext context)
        {
            // "admin" kullanıcı adı varsa ekleme
            if (context.Users.Any(u => u.Username == "admin"))
                return;

            var admin = new User
            {
                Username = "admin",
                PasswordHash = HashPassword("admin123"),
                Role = UserRole.Admin,
                IsActive = true,
                CreatedAt = DateTime.UtcNow
            };

            context.Users.Add(admin);
            context.SaveChanges();
        }

        private static string HashPassword(string password)
        {
            using var sha = SHA256.Create();
            var bytes = sha.ComputeHash(Encoding.UTF8.GetBytes(password));
            return Convert.ToHexString(bytes);
        }
    }
}
