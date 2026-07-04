using System;
using System.Collections.Generic;
using System.Text;

namespace OmniWeigh.Core.Models
{
    public class Weighing
    {
        public int Id { get; set; }
        public DateTime Timestamp { get; set; } = DateTime.Now;

        // Weighing metrics
        public double GrossWeight { get; set; }
        public double Tare { get; set; }

        // Automatically computed property
        public double NetWeight => GrossWeight - Tare;

        // Foreign Keys and Navigation Properties (EF Core)
        public int ClientId { get; set; }
        public virtual Client Client { get; set; } = null!;

        public int ProductId { get; set; }
        public virtual Product Product { get; set; } = null!;
    }
}
