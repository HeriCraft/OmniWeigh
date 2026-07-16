namespace OmniWeigh.Core.Models
{
    public class WeighingSession
    {
        public Guid Id { get; set; }
        
        public int DocumentId { get; set; }
        public virtual Document Document { get; set; } = null!;

        public bool IsClosed { get; set; }
        
        public DateTime StartedAt { get; set; } = DateTime.Now;
        public DateTime? ClosedAt { get; set; }

        public virtual ICollection<WeighingHistory> HistoryRecords { get; set; } = new List<WeighingHistory>();
    }
}
