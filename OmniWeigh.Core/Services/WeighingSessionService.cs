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

        public async Task<(System.Collections.Generic.IEnumerable<OmniWeigh.Core.Services.DTOs.HistoryRecordDto> Records, int TotalCount)> GetHistoryAsync(int pageNumber, int pageSize, OmniWeigh.Core.Services.DTOs.HistoryFilterDto? filter = null)
        {
            var query = _dbContext.WeighingHistories
                .Include(h => h.Session)
                    .ThenInclude(s => s.Document)
                        .ThenInclude(d => d.Client)
                .Include(h => h.Product)
                .AsQueryable();

            if (filter != null)
            {
                if (!string.IsNullOrWhiteSpace(filter.ClientName))
                {
                    query = query.Where(h => h.Session.Document != null && h.Session.Document.Client != null && h.Session.Document.Client.Name.Contains(filter.ClientName));
                }
                if (!string.IsNullOrWhiteSpace(filter.ProductName))
                {
                    query = query.Where(h => h.Product != null && h.Product.Name.Contains(filter.ProductName));
                }
                if (filter.MinWeight.HasValue)
                {
                    query = query.Where(h => (h.GrossWeight - h.Tare) >= filter.MinWeight.Value);
                }
                if (filter.MaxWeight.HasValue)
                {
                    query = query.Where(h => (h.GrossWeight - h.Tare) <= filter.MaxWeight.Value);
                }
                if (filter.StartDate.HasValue)
                {
                    query = query.Where(h => h.Timestamp >= filter.StartDate.Value);
                }
                if (filter.EndDate.HasValue)
                {
                    query = query.Where(h => h.Timestamp <= filter.EndDate.Value);
                }
            }

            query = query.OrderByDescending(h => h.Timestamp);

            var totalCount = await query.CountAsync();

            var records = await query
                .Skip((pageNumber - 1) * pageSize)
                .Take(pageSize)
                .Select(h => new OmniWeigh.Core.Services.DTOs.HistoryRecordDto
                {
                    Id = h.Id,
                    Timestamp = h.Timestamp,
                    DeliveryNoteReference = h.Session.Document != null ? h.Session.Document.DocumentNumber : string.Empty,
                    ClientName = h.Session.Document != null && h.Session.Document.Client != null ? h.Session.Document.Client.Name : string.Empty,
                    ProductName = h.Product != null ? h.Product.Name : string.Empty,
                    Quantity = h.Quantity,
                    Unit = h.Unit.ToString(),
                    NetWeight = h.GrossWeight - h.Tare
                })
                .ToListAsync();

            return (records, totalCount);
        }
    }
}
