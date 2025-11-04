using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using ModulPanel.Data;
using ModulPanel.Enums;
using ModulPanel.Entities;


namespace ModulPanel.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class LogsController : ControllerBase
    {
        private readonly AppDbContext _context;

        public LogsController(AppDbContext context)
        {
            _context = context;
        }

        [HttpGet]
        public IActionResult GetAll()
        {
            var logs = _context.Logs
                .OrderByDescending(l => l.CreatedAt)
                .Take(200)
                .ToList();

            return Ok(logs);
        }

        [HttpPost("module-opened")]
        public async Task<IActionResult> LogModuleOpened([FromBody] ModuleLogDto dto)
        {
            var userId = User.Claims.FirstOrDefault(c => c.Type == "uid")?.Value;
            var username = User.Identity?.Name ?? "Unknown";

            await _context.Logs.AddAsync(new Log
            {
                UserId = int.TryParse(userId, out var id) ? id : null,
                Action = "ModuleOpened",
                Message = $"{username} {dto.ModuleName} modülünü açtı.",
                Module = dto.ModuleName,
                CreatedAt = DateTime.UtcNow,
                IpAddress = HttpContext.Connection.RemoteIpAddress?.ToString() ?? "-"
            });

            await _context.SaveChangesAsync();
            return Ok();
        }

        public class ModuleLogDto
        {
            public string ModuleName { get; set; } = "";
        }

    }
}
