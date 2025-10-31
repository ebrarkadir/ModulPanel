using System.Linq;
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
            // 🔹 Eğer zaten bir Admin kullanıcı varsa, yeni oluşturma
            if (context.Users.Any(u => u.Role == UserRole.Admin))
                return;

            var admin = new User
            {
                Username = "admin",
                PasswordHash = HashPassword("admin123"),
                Role = UserRole.Admin, // int olarak 1 saklanır
                IsActive = true,
                CreatedAt = DateTime.UtcNow
            };

            context.Users.Add(admin);
            context.SaveChanges();

            Console.WriteLine("✅ Default admin user created: username=admin, password=admin123");
        }

        private static string HashPassword(string password)
        {
            using var sha = SHA256.Create();
            var bytes = sha.ComputeHash(Encoding.UTF8.GetBytes(password));
            return Convert.ToHexString(bytes);
        }
    }
}
