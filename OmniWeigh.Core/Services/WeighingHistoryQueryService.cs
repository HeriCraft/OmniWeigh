using Microsoft.EntityFrameworkCore;
using OmniWeigh.Core.Data;
using OmniWeigh.Core.Services.DTOs;

namespace OmniWeigh.Core.Services
{
    public class WeighingHistoryQueryService : IWeighingHistoryQueryService
    {
        private readonly OmniDbContext _dbContext;

        public WeighingHistoryQueryService(OmniDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        public async Task<PagedResult<WeighingHistoryItemDto>> GetHistoryAsync(WeighingHistoryFilterDto filter)
        {
            // Base query with AsNoTracking for read-only optimization
            var query = _dbContext.WeighingHistories.AsNoTracking().AsQueryable();

            // 1. Dynamic Predicate Filtering Engine

            // Date filtering
            if (filter.StartDate.HasValue)
            {
                query = query.Where(h => h.Timestamp >= filter.StartDate.Value);
            }
            
            if (filter.EndDate.HasValue)
            {
                query = query.Where(h => h.Timestamp <= filter.EndDate.Value);
            }

            // Interval Processing: NetWeight filtering
            // Note: In SQLite, doing math in the WHERE clause is supported but if performance degrades
            // on huge sets, storing NetWeight as a persisted computed column is recommended.
            if (filter.MinWeight.HasValue)
            {
                query = query.Where(h => (h.GrossWeight - h.Tare) >= filter.MinWeight.Value);
            }

            if (filter.MaxWeight.HasValue)
            {
                query = query.Where(h => (h.GrossWeight - h.Tare) <= filter.MaxWeight.Value);
            }

            // Foreign Key exact matches
            if (filter.ClientId.HasValue)
            {
                query = query.Where(h => h.Session.Document.ClientId == filter.ClientId.Value);
            }

            if (filter.ProductId.HasValue)
            {
                query = query.Where(h => h.ProductId == filter.ProductId.Value);
            }

            // Text Search (LIKE logic)
            if (!string.IsNullOrWhiteSpace(filter.SearchText))
            {
                var searchTerm = filter.SearchText.Trim().ToLower();
                query = query.Where(h => 
                    h.WeighingReference.ToLower().Contains(searchTerm) ||
                    h.Session.Document.DocumentNumber.ToLower().Contains(searchTerm) ||
                    h.Session.Document.Client.Name.ToLower().Contains(searchTerm) ||
                    h.Product.Name.ToLower().Contains(searchTerm));
            }

            // 2. Count before pagination
            var totalCount = await query.CountAsync();

            // 3. Sorting (Default by Newest)
            query = query.OrderByDescending(h => h.Timestamp);

            // 4. Pagination
            var skip = (filter.Page - 1) * filter.PageSize;
            var paginatedQuery = query.Skip(skip).Take(filter.PageSize);

            // 5. Projection
            // By projecting directly into the DTO, EF Core automatically generates optimal SQL 
            // with necessary INNER JOINs for Product, Session, Document, and Client,
            // bypassing the need for explicit .Include() calls.
            var items = await paginatedQuery.Select(h => new WeighingHistoryItemDto
            {
                HistoryId = h.Id,
                SessionId = h.SessionId,
                Timestamp = h.Timestamp,
                WeighingReference = h.WeighingReference,
                DocumentNumber = h.Session.Document.DocumentNumber,
                DocumentType = h.Session.Document.Type.ToString(),
                ClientName = h.Session.Document.Client.Name,
                ProductName = h.Product.Name,
                GrossWeight = h.GrossWeight,
                Tare = h.Tare,
                NetWeight = h.GrossWeight - h.Tare,
                Quantity = h.Quantity,
                Unit = h.Unit,
                Observation = h.Observation
            }).ToListAsync();

            return new PagedResult<WeighingHistoryItemDto>
            {
                Items = items,
                TotalCount = totalCount,
                Page = filter.Page,
                PageSize = filter.PageSize
            };
        }
    }
}
