namespace OmniWeigh.Core.Models
{
    public class WeighingHistory
    {
        public int Id { get; set; }
        public DateTime Timestamp { get; set; } = DateTime.Now;

        public Guid SessionId { get; set; }
        public virtual WeighingSession Session { get; set; } = null!;

        /// <summary>
        /// e.g. PP-xxxxxxxxxxxx
        /// </summary>
        public string WeighingReference { get; set; } = string.Empty;

        // Weighing metrics
        public double GrossWeight { get; set; }
        public double Tare { get; set; }
        public double Quantity { get; set; }
        
        public UnitType Unit { get; set; } = UnitType.PCS;

        public string? Observation { get; set; }

        // Automatically computed property
        public double NetWeight => GrossWeight - Tare;

        // Foreign Keys and Navigation Properties (EF Core)
        public int ProductId { get; set; }
        public virtual Product Product { get; set; } = null!;
    }
}
