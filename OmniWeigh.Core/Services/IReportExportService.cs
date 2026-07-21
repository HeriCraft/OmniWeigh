using System.Threading.Tasks;
using OmniWeigh.Core.Services.DTOs;

namespace OmniWeigh.Core.Services
{
    public interface IReportExportService
    {
        Task<byte[]> GenerateExcelReportAsync(ReportFilter filter);
        Task<byte[]> GeneratePdfReportAsync(ReportFilter filter);
    }
}
