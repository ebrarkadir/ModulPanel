using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using ModulPanel.DTOs;
using ModulPanel.Enums;
using ModulPanel.Services;

namespace ModulPanel.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize(Roles = nameof(UserRole.Admin))]
    public class ModulesController : ControllerBase
    {
        private readonly ModuleService _moduleService;

        public ModulesController(ModuleService moduleService)
        {
            _moduleService = moduleService;
        }

        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            var modules = await _moduleService.GetAllAsync();
            return Ok(modules);
        }

        [HttpPost("upload")]
        public async Task<IActionResult> Upload([FromForm] ModuleUploadDto dto)
        {
            var userId = int.Parse(User.Claims.First(c => c.Type == "uid").Value);
            var result = await _moduleService.UploadAsync(dto, userId);

            if (result == null)
                return BadRequest("Modül yüklenemedi veya aynı key'e sahip bir modül mevcut.");

            return Ok(result);
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(int id)
        {
            var userId = int.Parse(User.Claims.First(c => c.Type == "uid").Value);
            var success = await _moduleService.DeleteAsync(id, userId);

            if (!success)
                return NotFound("Modül bulunamadı.");

            return Ok($"Modül ID {id} başarıyla silindi.");
        }
    }
}
