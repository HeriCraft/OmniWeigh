using OmniWeigh.Core.Models;

namespace OmniWeigh.Core.Services.DTOs
{
    public class WeighingHistoryItemDto
    {
        public int HistoryId { get; set; }
        public Guid SessionId { get; set; }
        public DateTime Timestamp { get; set; }
        
        public string WeighingReference { get; set; } = string.Empty;
        public string DocumentNumber { get; set; } = string.Empty;
        public string DocumentType { get; set; } = string.Empty;
        
        public string ClientName { get; set; } = string.Empty;
        public string ProductName { get; set; } = string.Empty;
        
        public double GrossWeight { get; set; }
        public double Tare { get; set; }
        public double NetWeight { get; set; }
        public double Quantity { get; set; }
        public UnitType Unit { get; set; }
        
        public string? Observation { get; set; }
    }

    public class PagedResult<T>
    {
        public List<T> Items { get; set; } = new List<T>();
        public int TotalCount { get; set; }
        public int Page { get; set; }
        public int PageSize { get; set; }
        public int TotalPages => (int)Math.Ceiling((double)TotalCount / PageSize);
    }
}
