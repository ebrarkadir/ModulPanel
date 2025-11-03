using Microsoft.AspNetCore.Hosting;
using Microsoft.EntityFrameworkCore;
using ModulPanel.Data;
using ModulPanel.DTOs;
using ModulPanel.Entities;
using System.IO.Compression;

namespace ModulPanel.Services
{
    public class ModuleService
    {
        private readonly AppDbContext _context;
        private readonly LogService _logService;
        private readonly IWebHostEnvironment _env;

        public ModuleService(AppDbContext context, LogService logService, IWebHostEnvironment env)
        {
            _context = context;
            _logService = logService;
            _env = env;
        }

        public async Task<List<ModuleResponseDto>> GetAllAsync()
        {
            return await _context.Modules
                .AsNoTracking()
                .Select(m => new ModuleResponseDto
                {
                    Id = m.Id,
                    Name = m.Name,
                    Key = m.Key,
                    Description = m.Description,
                    Path = m.Path,
                    EntryFile = m.EntryFile,
                    IsActive = m.IsActive,
                    CreatedAt = m.CreatedAt,
                    CreatedBy = _context.Users
                        .Where(u => u.Id == m.CreatedByUserId)
                        .Select(u => u.Username)
                        .FirstOrDefault() ?? "Unknown"
                })
                .ToListAsync();
        }

        public async Task<ModuleResponseDto?> UploadAsync(ModuleUploadDto dto, int adminUserId)
        {
            try
            {
                if (await _context.Modules.AnyAsync(m => m.Key == dto.Key))
                    return null;

                var ext = Path.GetExtension(dto.File.FileName).ToLower();
                if (ext != ".zip" && ext != ".html" && ext != ".rar")
                    return null;

                var wwwRootPath = _env.WebRootPath ?? Path.Combine(_env.ContentRootPath, "wwwroot");
                var baseDir = Path.Combine(wwwRootPath, "Modules", dto.Key);

                if (Directory.Exists(baseDir))
                    Directory.Delete(baseDir, true);
                Directory.CreateDirectory(baseDir);

                var filePath = Path.Combine(baseDir, dto.File.FileName);
                using (var stream = new FileStream(filePath, FileMode.Create))
                    await dto.File.CopyToAsync(stream);

                string? entryFile = null;

                if (ext == ".zip")
                {
                    string tempDir = Path.Combine(baseDir, "temp");
                    Directory.CreateDirectory(tempDir);

                    using (var archive = ZipFile.OpenRead(filePath))
                    {
                        foreach (var entry in archive.Entries)
                        {
                            var destinationPath = Path.Combine(tempDir, entry.FullName);
                            if (entry.FullName.EndsWith("/")) continue;
                            Directory.CreateDirectory(Path.GetDirectoryName(destinationPath)!);
                            entry.ExtractToFile(destinationPath, true);
                        }
                    }

                    File.Delete(filePath);

                    var subDirs = Directory.GetDirectories(tempDir);
                    if (subDirs.Length == 1 && Directory.GetFiles(tempDir).Length == 0)
                    {
                        var innerDir = subDirs[0];
                        foreach (var file in Directory.GetFiles(innerDir, "*", SearchOption.AllDirectories))
                        {
                            var dest = Path.Combine(baseDir, Path.GetRelativePath(innerDir, file));
                            Directory.CreateDirectory(Path.GetDirectoryName(dest)!);
                            File.Copy(file, dest, true);
                        }
                    }
                    else
                    {
                        foreach (var file in Directory.GetFiles(tempDir, "*", SearchOption.AllDirectories))
                        {
                            var dest = Path.Combine(baseDir, Path.GetRelativePath(tempDir, file));
                            Directory.CreateDirectory(Path.GetDirectoryName(dest)!);
                            File.Copy(file, dest, true);
                        }
                    }

                    Directory.Delete(tempDir, true);

                    var htmlFiles = Directory.GetFiles(baseDir, "*.html", SearchOption.AllDirectories);
                    entryFile = htmlFiles
                        .Select(Path.GetFileName)
                        .FirstOrDefault(f => f!.Equals("index.html", StringComparison.OrdinalIgnoreCase))
                        ?? Path.GetFileName(htmlFiles.FirstOrDefault());
                }
                else if (ext == ".html")
                {
                    entryFile = dto.File.FileName;
                }

                var module = new Module
                {
                    Name = dto.Name,
                    Key = dto.Key,
                    Description = dto.Description,
                    Path = $"/Modules/{dto.Key}/",
                    EntryFile = entryFile ?? "index.html",
                    CreatedByUserId = adminUserId,
                    CreatedAt = DateTime.UtcNow,
                    IsActive = true
                };

                _context.Modules.Add(module);
                await _context.SaveChangesAsync();

                await _logService.AddAsync(adminUserId, "Upload", $"Yeni modül yüklendi: {module.Name}", "Module");

                return new ModuleResponseDto
                {
                    Id = module.Id,
                    Name = module.Name,
                    Key = module.Key,
                    Description = module.Description,
                    Path = module.Path,
                    EntryFile = module.EntryFile,
                    IsActive = module.IsActive,
                    CreatedAt = module.CreatedAt,
                    CreatedBy = _context.Users
                        .Where(u => u.Id == module.CreatedByUserId)
                        .Select(u => u.Username)
                        .FirstOrDefault() ?? "Unknown"
                };
            }
            catch (Exception ex)
            {
                await _logService.AddAsync(adminUserId, "Error", $"Modül yükleme hatası: {ex.Message}", "Module");
                throw;
            }
        }

        public async Task<bool> DeleteAsync(int id, int adminUserId)
        {
            var module = await _context.Modules.FindAsync(id);
            if (module == null)
            {
                await _logService.AddAsync(adminUserId, "Error", $"Silinmek istenen modül bulunamadı (ID={id})", "Module");
                return false;
            }

            var webRoot = _env.WebRootPath ?? Path.Combine(_env.ContentRootPath, "wwwroot");
            var folderPath = Path.Combine(webRoot, "Modules", module.Key);
            if (Directory.Exists(folderPath))
                Directory.Delete(folderPath, true);

            _context.Modules.Remove(module);
            await _context.SaveChangesAsync();

            await _logService.AddAsync(adminUserId, "Delete", $"Modül silindi: {module.Name}", "Module");
            return true;
        }
    }
}
