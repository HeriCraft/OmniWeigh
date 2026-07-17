namespace OmniWeigh.Core.Services.DTOs
{
    public class HistoryFilterDto
    {
        public string? ClientName { get; set; }
        public string? ProductName { get; set; }
        public double? MinWeight { get; set; }
        public double? MaxWeight { get; set; }
        public System.DateTime? StartDate { get; set; }
        public System.DateTime? EndDate { get; set; }
    }
}
