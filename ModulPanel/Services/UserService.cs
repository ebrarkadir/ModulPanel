using System.Security.Cryptography;
using System.Text;
using Microsoft.EntityFrameworkCore;
using ModulPanel.Data;
using ModulPanel.DTOs;
using ModulPanel.Entities;
using ModulPanel.Enums;

namespace ModulPanel.Services
{
    public class UserService
    {
        private readonly AppDbContext _context;
        private readonly LogService _logService;

        public UserService(AppDbContext context, LogService logService)
        {
            _context = context;
            _logService = logService;
        }

        // 🔹 Tüm kullanıcıları getir (izinlerle birlikte)
        public async Task<List<UserResponseDto>> GetAllAsync()
        {
            var users = await _context.Users
                .Include(u => u.Permissions)
                .AsNoTracking()
                .Select(u => new UserResponseDto
                {
                    Id = u.Id,
                    Username = u.Username,
                    Role = u.Role,
                    IsActive = u.IsActive,
                    CreatedAt = u.CreatedAt,
                    Permissions = u.Permissions != null
                        ? u.Permissions.Select(p => p.PermissionKey).ToList()
                        : new List<string>()
                })
                .ToListAsync();

            await _logService.AddAsync(null, "GET_USERS", "Kullanıcı listesi görüntülendi.", "User");
            return users;
        }

        // 🔹 Yeni kullanıcı oluştur
        public async Task<UserResponseDto?> CreateAsync(UserCreateDto dto)
        {
            if (await _context.Users.AnyAsync(u => u.Username == dto.Username))
                return null;

            var user = new User
            {
                Username = dto.Username.Trim(),
                PasswordHash = HashPassword(dto.Password),
                Role = dto.Role,
                CreatedAt = DateTime.UtcNow,
                IsActive = true,
            };

            // 🔹 Yetkiler
            if (dto.Role == UserRole.Admin)
            {
                user.Permissions.Add(new UserPermission { PermissionKey = "all", User = user });
            }
            else
            {
                foreach (var perm in dto.Permissions.Distinct())
                {
                    user.Permissions.Add(new UserPermission
                    {
                        PermissionKey = perm.ToLower(),
                        User = user
                    });
                }
            }

            _context.Users.Add(user);
            await _context.SaveChangesAsync();

            await _logService.AddAsync(null, "CREATE_USER", $"Yeni kullanıcı oluşturuldu: {user.Username}", "User");

            return new UserResponseDto
            {
                Id = user.Id,
                Username = user.Username,
                Role = user.Role,
                CreatedAt = user.CreatedAt,
                IsActive = user.IsActive,
                Permissions = user.Permissions.Select(p => p.PermissionKey).ToList()
            };
        }

        // 🔹 Kullanıcı güncelle
        public async Task<bool> UpdateAsync(int id, UserUpdateDto dto)
        {
            var user = await _context.Users
                .Include(u => u.Permissions)
                .FirstOrDefaultAsync(u => u.Id == id);

            if (user == null)
            {
                await _logService.AddAsync(null, "UPDATE_USER_FAIL", $"Kullanıcı bulunamadı (ID: {id})", "User");
                return false;
            }

            if (!string.IsNullOrWhiteSpace(dto.Username))
                user.Username = dto.Username.Trim();

            if (!string.IsNullOrWhiteSpace(dto.Password))
                user.PasswordHash = HashPassword(dto.Password);

            user.Role = dto.Role;

            // 🔹 mevcut izinleri temizle ve yenileri ekle
            user.Permissions.Clear();

            if (dto.Role == UserRole.Admin)
            {
                user.Permissions.Add(new UserPermission { PermissionKey = "all", User = user });
            }
            else
            {
                foreach (var perm in dto.Permissions.Distinct())
                {
                    user.Permissions.Add(new UserPermission
                    {
                        PermissionKey = perm.ToLower(),
                        User = user
                    });
                }
            }

            await _context.SaveChangesAsync();
            await _logService.AddAsync(user.Id, "UPDATE_USER", $"Kullanıcı güncellendi: {user.Username}", "User");
            return true;
        }

        // 🔹 Kullanıcı sil
        public async Task<bool> DeleteAsync(int id)
        {
            var user = await _context.Users.Include(u => u.Permissions).FirstOrDefaultAsync(u => u.Id == id);
            if (user == null)
            {
                await _logService.AddAsync(null, "DELETE_USER_FAIL", $"Kullanıcı bulunamadı (ID: {id})", "User");
                return false;
            }

            _context.Users.Remove(user);
            await _context.SaveChangesAsync();

            await _logService.AddAsync(user.Id, "DELETE_USER", $"Kullanıcı silindi: {user.Username}", "User");
            return true;
        }

        // 🔹 Şifre hash fonksiyonu
        private string HashPassword(string password)
        {
            using var sha = SHA256.Create();
            var bytes = sha.ComputeHash(Encoding.UTF8.GetBytes(password));
            return Convert.ToHexString(bytes);
        }
    }
}
