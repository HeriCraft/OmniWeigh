using System.Collections.Generic;
using System.Threading.Tasks;
using OmniWeigh.Core.Services.DTOs;

namespace OmniWeigh.Core.Services
{
    public interface IAnalyticsService
    {
        Task<DailyMetricsDto> GetDailyMetricsAsync();
        Task<DashboardKpiDto> GetDashboardKpisAsync(ReportFilter filter);
        Task<List<TimeSeriesDataPoint>> GetVolumeTimeSeriesAsync(ReportFilter filter);
        Task<List<CategoricalDataPoint>> GetProductDistributionAsync(ReportFilter filter);
        Task<List<CategoricalDataPoint>> GetPeakActivityHoursAsync(ReportFilter filter);
        Task<List<CategoricalDataPoint>> GetDocumentTypeDistributionAsync(ReportFilter filter);
        Task<List<CategoricalDataPoint>> GetClientDistributionAsync(ReportFilter filter);
    }
}
