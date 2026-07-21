using System;

namespace OmniWeigh.Core.Services.DTOs
{
    public enum GroupByMode
    {
        ByProduct,
        ByClient
    }

    public class ReportFilter
    {
        public DateTime? StartDate { get; set; }
        public DateTime? EndDate { get; set; }
        public int? ClientId { get; set; }
        public int? ProductId { get; set; }
        public string? DocumentType { get; set; }
        public GroupByMode GroupBy { get; set; } = GroupByMode.ByProduct;
    }
}
