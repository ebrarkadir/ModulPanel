using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using ModulPanel.Data;
using ModulPanel.DTOs;
using ModulPanel.Entities;
using ModulPanel.Enums; // 🔹 Enum erişimi için

namespace ModulPanel.Services
{
    public class AuthService
    {
        private readonly AppDbContext _context;
        private readonly IConfiguration _configuration;

        public AuthService(AppDbContext context, IConfiguration configuration)
        {
            _context = context;
            _configuration = configuration;
        }

        // 🔹 1. Kullanıcı giriş işlemi
        public async Task<AuthResponseDto?> LoginAsync(LoginDto dto)
        {
            var user = await _context.Users.FirstOrDefaultAsync(u => u.Username == dto.Username);
            if (user == null)
                return null;

            // Şifre kontrolü (hash karşılaştırması)
            var hashedPassword = HashPassword(dto.Password);
            if (user.PasswordHash != hashedPassword)
                return null;

            // Token üret
            var accessToken = GenerateJwtToken(user);
            var refreshToken = GenerateRefreshToken();

            // Refresh token’ı kaydet
            user.RefreshToken = refreshToken;
            user.RefreshTokenExpiry = DateTime.UtcNow.AddDays(7);
            await _context.SaveChangesAsync();

            return new AuthResponseDto
            {
                AccessToken = accessToken,
                RefreshToken = refreshToken,
                Username = user.Username,
                Role = user.Role // ✅ Enum DTO’ya doğrudan atanır
            };
        }

        // 🔹 2. Şifre hash fonksiyonu (SHA256)
        private string HashPassword(string password)
        {
            using var sha = SHA256.Create();
            var bytes = sha.ComputeHash(Encoding.UTF8.GetBytes(password));
            return Convert.ToHexString(bytes);
        }

        // 🔹 3. JWT Token oluşturma
        private string GenerateJwtToken(User user)
        {
            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_configuration["Jwt:Key"]!));
            var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

            var claims = new[]
            {
                new Claim(JwtRegisteredClaimNames.Sub, user.Username),
                new Claim("uid", user.Id.ToString()),
                new Claim(ClaimTypes.Role, user.Role.ToString()), // ✅ Enum → string
                new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString())
            };

            var token = new JwtSecurityToken(
                issuer: _configuration["Jwt:Issuer"],
                audience: _configuration["Jwt:Audience"],
                claims: claims,
                expires: DateTime.UtcNow.AddHours(2),
                signingCredentials: creds
            );

            return new JwtSecurityTokenHandler().WriteToken(token);
        }

        // 🔹 4. Refresh token oluşturma
        private string GenerateRefreshToken()
        {
            var randomNumber = new byte[64];
            using var rng = RandomNumberGenerator.Create();
            rng.GetBytes(randomNumber);
            return Convert.ToBase64String(randomNumber);
        }

        // 🔹 5. Çıkış işlemi
        public async Task<bool> LogoutAsync(string username)
        {
            var user = await _context.Users.FirstOrDefaultAsync(u => u.Username == username);
            if (user == null) return false;

            user.RefreshToken = null;
            user.RefreshTokenExpiry = null;
            await _context.SaveChangesAsync();

            return true;
        }

        // 🔹 6. Refresh token yenileme
        public async Task<AuthResponseDto?> RefreshTokenAsync(RefreshTokenDto dto)
        {
            var user = await _context.Users.FirstOrDefaultAsync(u =>
                u.RefreshToken == dto.RefreshToken &&
                u.RefreshTokenExpiry > DateTime.UtcNow);

            if (user == null)
                return null;

            var newAccessToken = GenerateJwtToken(user);
            var newRefreshToken = GenerateRefreshToken();

            user.RefreshToken = newRefreshToken;
            user.RefreshTokenExpiry = DateTime.UtcNow.AddDays(7);
            await _context.SaveChangesAsync();

            return new AuthResponseDto
            {
                AccessToken = newAccessToken,
                RefreshToken = newRefreshToken,
                Username = user.Username,
                Role = user.Role
            };
        }
    }
}
