using System;
using System.Collections.Generic;
using System.Text;

namespace OmniWeigh.Core.Models
{
    public class Client
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string ContactInfo { get; set; } = string.Empty;

        public virtual ICollection<Weighing> Weighings { get; set; } = new List<Weighing>();
    }
}
