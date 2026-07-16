using Microsoft.EntityFrameworkCore;
using OmniWeigh.Core.Data;
using OmniWeigh.Core.Models;

namespace OmniWeigh.Core.Services
{
    public class WeighingSessionService : IWeighingSessionService
    {
        private readonly OmniDbContext _dbContext;

        public WeighingSessionService(OmniDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        public async Task<WeighingSession> CreateSessionAsync(int documentId)
        {
            var session = new WeighingSession
            {
                Id = Guid.NewGuid(),
                DocumentId = documentId,
                StartedAt = DateTime.Now,
                IsClosed = false
            };

            _dbContext.WeighingSessions.Add(session);
            await _dbContext.SaveChangesAsync();

            return session;
        }

        public async Task<WeighingSession?> GetSessionAsync(Guid sessionId)
        {
            return await _dbContext.WeighingSessions
                .Include(s => s.Document)
                .Include(s => s.HistoryRecords)
                .FirstOrDefaultAsync(s => s.Id == sessionId);
        }

        public async Task<WeighingHistory> AddWeighingAsync(Guid sessionId, double grossWeight, double tare, double quantity, UnitType unit, int productId, string? observation)
        {
            var session = await _dbContext.WeighingSessions.FindAsync(sessionId);
            if (session == null)
            {
                throw new ArgumentException("Session non trouvée.", nameof(sessionId));
            }

            if (session.IsClosed)
            {
                throw new InvalidOperationException("Impossible d'ajouter une pesée à une session clôturée.");
            }

            var record = new WeighingHistory
            {
                SessionId = sessionId,
                Timestamp = DateTime.Now,
                WeighingReference = GenerateWeighingReference(),
                GrossWeight = grossWeight,
                Tare = tare,
                Quantity = quantity,
                Unit = unit,
                ProductId = productId,
                Observation = observation
            };

            _dbContext.WeighingHistories.Add(record);
            await _dbContext.SaveChangesAsync();

            return record;
        }

        public async Task CloseSessionAsync(Guid sessionId)
        {
            var session = await _dbContext.WeighingSessions.FindAsync(sessionId);
            if (session != null && !session.IsClosed)
            {
                session.IsClosed = true;
                session.ClosedAt = DateTime.Now;
                await _dbContext.SaveChangesAsync();
            }
        }

        private string GenerateWeighingReference()
        {
            return $"PP-{Guid.NewGuid().ToString("N").Substring(0, 12).ToUpper()}";
        }
    }
}
