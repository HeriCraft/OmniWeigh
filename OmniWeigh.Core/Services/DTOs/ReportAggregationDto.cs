using OmniWeigh.Core.Models;

namespace OmniWeigh.Core.Services.DTOs
{
    public class ReportAggregationDto
    {
        public string GroupName { get; set; } = string.Empty;
        public UnitType Unit { get; set; }
        public int TotalPesees { get; set; }
        public double PoidsBrutTotal { get; set; }
        public double TareTotal { get; set; }
        public double PoidsNetTotal { get; set; }
        public double MoyennePoidsNet { get; set; }
    }
}
