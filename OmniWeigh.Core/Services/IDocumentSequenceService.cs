using OmniWeigh.Core.Models;

namespace OmniWeigh.Core.Services
{
    public interface IDocumentSequenceService
    {
        Task<string> GenerateDocumentNumberAsync(DocumentType type);
    }
}
