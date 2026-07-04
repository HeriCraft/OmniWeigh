using System;
using System.Collections.Generic;
using System.Text;

namespace OmniWeigh.Core.Models
{
    public class Product
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string Barcode { get; set; } = string.Empty;

        public virtual ICollection<Weighing> Weighings { get; set; } = new List<Weighing>();
    }
}
