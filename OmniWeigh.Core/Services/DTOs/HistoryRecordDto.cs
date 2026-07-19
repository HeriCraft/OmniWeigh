namespace OmniWeigh.Core.Services.DTOs
{
    public class HistoryRecordDto
    {
        public int Id { get; set; }
        public System.DateTime Timestamp { get; set; }
        public string DeliveryNoteReference { get; set; } = string.Empty;
        public string ClientName { get; set; } = string.Empty;
        public string ProductName { get; set; } = string.Empty;
        public double Quantity { get; set; }
        public string Unit { get; set; } = string.Empty;
        public double NetWeight { get; set; }
    }
}
