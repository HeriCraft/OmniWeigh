using OmniWeigh.Core.Models;

namespace OmniWeigh.Core.Services
{
    public interface IWeighingSessionService
    {
        Task<WeighingSession> CreateSessionAsync(int documentId);
        Task<WeighingSession?> GetSessionAsync(Guid sessionId);
        Task<WeighingHistory> AddWeighingAsync(Guid sessionId, double grossWeight, double tare, double quantity, UnitType unit, int productId, string? observation);
        Task CloseSessionAsync(Guid sessionId);
        Task<(System.Collections.Generic.IEnumerable<OmniWeigh.Core.Services.DTOs.HistoryRecordDto> Records, int TotalCount)> GetHistoryAsync(int pageNumber, int pageSize, OmniWeigh.Core.Services.DTOs.HistoryFilterDto? filter = null);
    }
}
