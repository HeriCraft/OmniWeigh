using System;
using System.Collections.Generic;

namespace OmniWeigh.Core.Services.DTOs
{
    public class DashboardKpiDto
    {
        public double TotalVolume { get; set; }
        public int TotalSessions { get; set; }
        public string TopProduct { get; set; } = string.Empty;
        public string TopClient { get; set; } = string.Empty;
        public double AverageWeightPerSession { get; set; }
    }

    public class TimeSeriesDataPoint
    {
        public DateTime Date { get; set; }
        public double Value { get; set; }
    }

    public class CategoricalDataPoint
    {
        public string Category { get; set; } = string.Empty;
        public double Value { get; set; }
    }
}
