namespace ModulPanel.DTOs
{
    public class ModuleCreateDto
    {
        public string Name { get; set; } = string.Empty;
        public string Key { get; set; } = string.Empty; 
        public string? Description { get; set; }
        public string? Path { get; set; } 
    }
}
