using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using ModulPanel.DTOs;
using ModulPanel.Services;
using ModulPanel.Enums;

namespace ModulPanel.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize] // sadece Admin erişebilir
    public class UsersController : ControllerBase
    {
        private readonly UserService _userService;

        public UsersController(UserService userService)
        {
            _userService = userService;
        }

        // 🔹 GET: api/users
        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            var users = await _userService.GetAllAsync();
            return Ok(users);
        }

        // 🔹 POST: api/users
        [HttpPost]
        public async Task<IActionResult> Create([FromBody] UserCreateDto dto)
        {
            var result = await _userService.CreateAsync(dto);
            if (result == null)
                return BadRequest("Bu kullanıcı adı zaten kullanılıyor.");

            return Ok(result);
        }

        // 🔹 PUT: api/users/{id}
        [HttpPut("{id}")]
        public async Task<IActionResult> Update(int id, [FromBody] UserUpdateDto dto)
        {
            var success = await _userService.UpdateAsync(id, dto);
            if (!success) return NotFound("Kullanıcı bulunamadı.");

            return Ok($"Kullanıcı ID {id} başarıyla güncellendi.");
        }

        // 🔹 DELETE: api/users/{id}
        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(int id)
        {
            var success = await _userService.DeleteAsync(id);
            if (!success) return NotFound("Kullanıcı bulunamadı.");

            return Ok($"Kullanıcı ID {id} başarıyla silindi.");
        }
    }
}
