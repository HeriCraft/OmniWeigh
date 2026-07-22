using System;
using OmniWeigh.Core.Models;

namespace OmniWeigh.Core.Services.DTOs
{
    public class RecentTransactionDto
    {
        public DateTime Timestamp { get; set; }
        public string ClientName { get; set; } = string.Empty;
        public string ProductName { get; set; } = string.Empty;
        public double NetWeight { get; set; }
        public UnitType Unit { get; set; }
        public string DocumentReference { get; set; } = string.Empty;
    }
}
