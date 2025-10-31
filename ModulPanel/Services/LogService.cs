using ModulPanel.Data;
using ModulPanel.Entities;

namespace ModulPanel.Services
{
    public class LogService
    {
        private readonly AppDbContext _context;

        public LogService(AppDbContext context)
        {
            _context = context;
        }

        public async Task AddAsync(int? userId, string action, string message, string? module = null, string ipAddress = "")
        {
            var log = new Log
            {
                UserId = userId,
                Action = action,
                Message = message,
                Module = module ?? "System",
                IpAddress = ipAddress,
                CreatedAt = DateTime.UtcNow
            };

            _context.Logs.Add(log);
            await _context.SaveChangesAsync();
        }
    }
}
