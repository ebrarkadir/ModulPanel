using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using ModulPanel.Data;
using ModulPanel.Enums;

namespace ModulPanel.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize(Roles = nameof(UserRole.Admin))]
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
    }
}
