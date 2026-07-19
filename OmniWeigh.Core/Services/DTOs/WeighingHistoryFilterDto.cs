namespace OmniWeigh.Core.Services.DTOs
{
    public class WeighingHistoryFilterDto
    {
        public string? SearchText { get; set; }
        
        public double? MinWeight { get; set; }
        public double? MaxWeight { get; set; }
        
        public DateTime? StartDate { get; set; }
        public DateTime? EndDate { get; set; }
        
        public int? ClientId { get; set; }
        public int? ProductId { get; set; }

        public int Page { get; set; } = 1;
        public int PageSize { get; set; } = 50;
    }
}
