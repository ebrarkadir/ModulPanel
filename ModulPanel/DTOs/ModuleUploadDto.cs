namespace ModulPanel.DTOs
{
    public class ModuleUploadDto
    {
        public string Name { get; set; } = string.Empty;
        public string Key { get; set; } = string.Empty;
        public string? Description { get; set; }
        public IFormFile File { get; set; } = null!;
    }
}
