namespace OmniWeigh.Core.Services.DTOs
{
    public class ProductDto
    {
        public int Id { get; set; }
        public string Reference { get; set; } = string.Empty;
        public string Name { get; set; } = string.Empty;
        public string? ImageFileName { get; set; }
    }
}
