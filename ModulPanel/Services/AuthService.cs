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
using ModulPanel.Enums;

namespace ModulPanel.Services
{
    public class AuthService
    {
        private readonly AppDbContext _context;
        private readonly IConfiguration _configuration;
        private readonly LogService _logService;

        public AuthService(AppDbContext context, IConfiguration configuration, LogService logService)
        {
            _context = context;
            _configuration = configuration;
            _logService = logService;
        }

        public async Task<AuthResponseDto?> LoginAsync(LoginDto dto)
        {
            var user = await _context.Users.FirstOrDefaultAsync(u => u.Username == dto.Username);
            if (user == null)
            {
                await _logService.AddAsync(null, "LOGIN_FAIL", $"Kullanıcı bulunamadı: {dto.Username}", "Auth");
                return null;
            }

            var hashedPassword = HashPassword(dto.Password);
            if (user.PasswordHash != hashedPassword)
            {
                await _logService.AddAsync(user.Id, "LOGIN_FAIL", $"Hatalı şifre denemesi: {dto.Username}", "Auth");
                return null;
            }

            var accessToken = GenerateJwtToken(user);
            var refreshToken = GenerateRefreshToken();

            user.RefreshToken = refreshToken;
            user.RefreshTokenExpiry = DateTime.UtcNow.AddDays(7);
            await _context.SaveChangesAsync();

            await _logService.AddAsync(user.Id, "LOGIN_SUCCESS", $"{user.Username} sisteme giriş yaptı.", "Auth");

            return new AuthResponseDto
            {
                AccessToken = accessToken,
                RefreshToken = refreshToken,
                Username = user.Username,
                Role = user.Role
            };
        }

        private string HashPassword(string password)
        {
            using var sha = SHA256.Create();
            var bytes = sha.ComputeHash(Encoding.UTF8.GetBytes(password));
            return Convert.ToHexString(bytes);
        }

        private string GenerateJwtToken(User user)
        {
            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_configuration["Jwt:Key"]!));
            var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

            var claims = new[]
            {
                new Claim(JwtRegisteredClaimNames.Sub, user.Username),
                new Claim("uid", user.Id.ToString()),
                new Claim(ClaimTypes.Role, user.Role.ToString()),
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

        private string GenerateRefreshToken()
        {
            var randomNumber = new byte[64];
            using var rng = RandomNumberGenerator.Create();
            rng.GetBytes(randomNumber);
            return Convert.ToBase64String(randomNumber);
        }

        public async Task<bool> LogoutAsync(string username)
        {
            var user = await _context.Users.FirstOrDefaultAsync(u => u.Username == username);
            if (user == null)
            {
                await _logService.AddAsync(null, "LOGOUT_FAIL", $"Çıkış yapılamadı, kullanıcı bulunamadı: {username}", "Auth");
                return false;
            }

            user.RefreshToken = null;
            user.RefreshTokenExpiry = null;
            await _context.SaveChangesAsync();

            await _logService.AddAsync(user.Id, "LOGOUT_SUCCESS", $"{username} sistemden çıkış yaptı.", "Auth");
            return true;
        }

        public async Task<AuthResponseDto?> RefreshTokenAsync(RefreshTokenDto dto)
        {
            var user = await _context.Users.FirstOrDefaultAsync(u =>
                u.RefreshToken == dto.RefreshToken &&
                u.RefreshTokenExpiry > DateTime.UtcNow);

            if (user == null)
            {
                await _logService.AddAsync(null, "REFRESH_FAIL", "Geçersiz refresh token kullanımı tespit edildi.", "Auth");
                return null;
            }

            var newAccessToken = GenerateJwtToken(user);
            var newRefreshToken = GenerateRefreshToken();

            user.RefreshToken = newRefreshToken;
            user.RefreshTokenExpiry = DateTime.UtcNow.AddDays(7);
            await _context.SaveChangesAsync();

            await _logService.AddAsync(user.Id, "REFRESH_SUCCESS", $"{user.Username} için token yenilendi.", "Auth");

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
