namespace OmniWeigh.Core.Models
{
    public class Vehicle
    {
        // Use Registration as the primary semantic identifier (not necessarily DB primary key)
        public int Id { get; set; }
        public string Registration { get; set; } = string.Empty; // immatriculation - used as ID in app
        public string Type { get; set; } = string.Empty;
        public string? MaxLoad { get; set; }
        public string? ImageFile { get; set; }
    }
}
