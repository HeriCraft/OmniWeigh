namespace OmniWeigh.Core.Models
{
    public class Document
    {
        public int Id { get; set; }
        
        /// <summary>
        /// e.g. BL-000000000001
        /// </summary>
        public string DocumentNumber { get; set; } = string.Empty;
        
        public DocumentType Type { get; set; }
        
        public int ClientId { get; set; }
        public virtual Client Client { get; set; } = null!;

        public string DriverName { get; set; } = string.Empty;
        
        public string? Observations { get; set; }
        
        /// <summary>
        /// Base64 encoded string of the signature image
        /// </summary>
        public string? SignatureBase64 { get; set; }
        
        public DateTime CreatedAt { get; set; } = DateTime.Now;

        public virtual ICollection<WeighingSession> WeighingSessions { get; set; } = new List<WeighingSession>();
    }
}
