namespace OmniWeigh.Core.Models
{
    public class SequenceTracker
    {
        public int Id { get; set; }
        public string EntityType { get; set; } = string.Empty;
        public int NextValue { get; set; } = 1;
        public string Prefix { get; set; } = string.Empty;
    }
}
