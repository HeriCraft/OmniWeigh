using OmniWeigh.Core.Models;

namespace OmniWeigh.Core.Services
{
    public interface IWeighingSessionService
    {
        Task<WeighingSession> CreateSessionAsync(int documentId);
        Task<WeighingSession?> GetSessionAsync(Guid sessionId);
        Task<WeighingHistory> AddWeighingAsync(Guid sessionId, double grossWeight, double tare, double quantity, UnitType unit, int productId, string? observation);
        Task CloseSessionAsync(Guid sessionId);
    }
}
