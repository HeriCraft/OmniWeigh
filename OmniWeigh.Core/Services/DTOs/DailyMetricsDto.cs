namespace OmniWeigh.Core.Services.DTOs
{
    public class DailyMetricsDto
    {
        public int TotalWeighingsToday { get; set; }
        public double TotalNetWeightToday { get; set; }
        public string LastDocumentReference { get; set; } = string.Empty;
        public System.Collections.Generic.List<RecentTransactionDto> RecentTransactions { get; set; } = new();
    }
}
