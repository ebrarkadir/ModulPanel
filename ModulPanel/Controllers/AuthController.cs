using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using ModulPanel.DTOs;
using ModulPanel.Services;

namespace ModulPanel.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class AuthController : ControllerBase
    {
        private readonly AuthService _authService;

        public AuthController(AuthService authService)
        {
            _authService = authService;
        }

       
        [HttpPost("login")]
        public async Task<IActionResult> Login([FromBody] LoginDto dto)
        {
            if (string.IsNullOrWhiteSpace(dto.Username) || string.IsNullOrWhiteSpace(dto.Password))
                return BadRequest("Kullanıcı adı ve şifre gereklidir.");

            var result = await _authService.LoginAsync(dto);

            if (result == null)
                return Unauthorized("Kullanıcı adı veya şifre hatalı.");

            return Ok(result); // result → AuthResponseDto (AccessToken, RefreshToken, Username, Role)
        }

        // 🔹 POST: api/auth/logout
        [HttpPost("logout")]
        [Authorize]
        public async Task<IActionResult> Logout()
        {
            var username = User.Identity?.Name ?? User.Claims.FirstOrDefault(c => c.Type == "sub")?.Value;
            if (username == null)
                return Unauthorized("Kullanıcı kimliği doğrulanamadı.");

            var result = await _authService.LogoutAsync(username);
            if (!result)
                return BadRequest("Çıkış işlemi başarısız oldu.");

            return Ok("Kullanıcı başarıyla çıkış yaptı.");
        }

        // 🔹 POST: api/auth/refresh
        [HttpPost("refresh")]
        public async Task<IActionResult> Refresh([FromBody] RefreshTokenDto dto)
        {
            var result = await _authService.RefreshTokenAsync(dto);
            if (result == null)
                return Unauthorized("Refresh token geçersiz veya süresi dolmuş.");

            return Ok(result);
        }

    }
}
