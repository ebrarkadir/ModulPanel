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

        public async Task<List<UserResponseDto>> GetAllAsync()
        {
            var users = await _context.Users
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

            await _logService.AddAsync(null, "GET_USERS", "Kullanıcı listesi görüntülendi.", "User");
            return users;
        }

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

            await _logService.AddAsync(null, "CREATE_USER", $"Yeni kullanıcı oluşturuldu: {user.Username} (Rol: {user.Role})", "User");

            return new UserResponseDto
            {
                Id = user.Id,
                Username = user.Username,
                Role = user.Role,
                CreatedAt = user.CreatedAt,
                IsActive = user.IsActive
            };
        }

        public async Task<bool> UpdateAsync(int id, UserUpdateDto dto)
        {
            var user = await _context.Users.FindAsync(id);
            if (user == null)
            {
                await _logService.AddAsync(null, "UPDATE_USER_FAIL", $"Kullanıcı bulunamadı (ID: {id})", "User");
                return false;
            }

            if (!string.IsNullOrWhiteSpace(dto.Username))
                user.Username = dto.Username;

            if (!string.IsNullOrWhiteSpace(dto.Password))
                user.PasswordHash = HashPassword(dto.Password);

            user.Role = dto.Role;
            await _context.SaveChangesAsync();

            await _logService.AddAsync(null, "UPDATE_USER", $"Kullanıcı güncellendi (ID: {id}, Yeni Username: {user.Username})", "User");
            return true;
        }

        public async Task<bool> DeleteAsync(int id)
        {
            var user = await _context.Users.FindAsync(id);
            if (user == null)
            {
                await _logService.AddAsync(null, "DELETE_USER_FAIL", $"Kullanıcı bulunamadı (ID: {id})", "User");
                return false;
            }

            _context.Users.Remove(user);
            await _context.SaveChangesAsync();

            await _logService.AddAsync(null, "DELETE_USER", $"Kullanıcı silindi (ID: {id}, Username: {user.Username})", "User");
            return true;
        }

        private string HashPassword(string password)
        {
            using var sha = SHA256.Create();
            var bytes = sha.ComputeHash(Encoding.UTF8.GetBytes(password));
            return Convert.ToHexString(bytes);
        }
    }
}
