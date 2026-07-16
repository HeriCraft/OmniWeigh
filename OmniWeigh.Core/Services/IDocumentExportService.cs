using OmniWeigh.Core.Models;

namespace OmniWeigh.Core.Services
{
    public interface IDocumentExportService
    {
        Task ExportToPdfAsync(Document document, string outputPath, PrintLayout layout);
        Task PrintToLocalPrinterAsync(Document document, string printerName, PrintLayout layout);
    }
}
