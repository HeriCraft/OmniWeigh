using System.Collections.Generic;
using System.Threading.Tasks;
using OmniWeigh.Core.Services.DTOs;

namespace OmniWeigh.Core.Services
{
    public interface IReportQueryService
    {
        Task<List<ReportAggregationDto>> GetAggregatedReportAsync(ReportFilter filter);
    }
}
