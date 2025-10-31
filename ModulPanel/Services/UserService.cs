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

        public UserService(AppDbContext context)
        {
            _context = context;
        }

        // 🔹 1. Tüm kullanıcıları getir
        public async Task<List<UserResponseDto>> GetAllAsync()
        {
            return await _context.Users
                .AsNoTracking()
                .Select(u => new UserResponseDto
                {
                    Id = u.Id,
                    Username = u.Username,
                    Role = u.Role,
                    IsActive = u.IsActive,
                    CreatedAt = u.CreatedAt
                })
                .ToListAsync();
        }

        // 🔹 2. Yeni kullanıcı oluştur
        public async Task<UserResponseDto?> CreateAsync(UserCreateDto dto)
        {
            if (await _context.Users.AnyAsync(u => u.Username == dto.Username))
                return null;

            var user = new User
            {
                Username = dto.Username,
                PasswordHash = HashPassword(dto.Password),
                Role = dto.Role,
                CreatedAt = DateTime.UtcNow,
                IsActive = true
            };

            _context.Users.Add(user);
            await _context.SaveChangesAsync();

            return new UserResponseDto
            {
                Id = user.Id,
                Username = user.Username,
                Role = user.Role,
                CreatedAt = user.CreatedAt,
                IsActive = user.IsActive
            };
        }

        // 🔹 3. Kullanıcıyı güncelle
        public async Task<bool> UpdateAsync(int id, UserUpdateDto dto)
        {
            var user = await _context.Users.FindAsync(id);
            if (user == null) return false;

            if (!string.IsNullOrWhiteSpace(dto.Username))
                user.Username = dto.Username;

            if (!string.IsNullOrWhiteSpace(dto.Password))
                user.PasswordHash = HashPassword(dto.Password);

            user.Role = dto.Role;
            await _context.SaveChangesAsync();

            return true;
        }

        // 🔹 4. Kullanıcıyı sil
        public async Task<bool> DeleteAsync(int id)
        {
            var user = await _context.Users.FindAsync(id);
            if (user == null) return false;

            _context.Users.Remove(user);
            await _context.SaveChangesAsync();
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
