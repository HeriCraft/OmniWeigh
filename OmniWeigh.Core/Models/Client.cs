namespace OmniWeigh.Core.Models
{
    public class Client
    {
        public int Id { get; set; }
        // Reference stored as string in DB, format expected: C-{Id}
        // Nullable to tolerate older rows that may not have this column populated yet.
        public string? Reference { get; set; } = null;
        public string Name { get; set; } = string.Empty;
        public string ContactInfo { get; set; } = string.Empty;

        public virtual ICollection<Document> Documents { get; set; } = new List<Document>();
    }
}
