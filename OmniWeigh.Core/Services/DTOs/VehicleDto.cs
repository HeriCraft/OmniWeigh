namespace OmniWeigh.Core.Services.DTOs
{
    public class VehicleDto
    {
        public int Id { get; set; }
        public string Registration { get; set; } = string.Empty;
        public string Type { get; set; } = string.Empty;
        public string? MaxLoad { get; set; }
        public string? ImageFileName { get; set; }
    }
}
