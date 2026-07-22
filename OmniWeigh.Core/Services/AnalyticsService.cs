using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using OmniWeigh.Core.Data;
using OmniWeigh.Core.Models;
using OmniWeigh.Core.Services.DTOs;
using Microsoft.Extensions.Caching.Memory;

namespace OmniWeigh.Core.Services
{
    public class AnalyticsService : IAnalyticsService
    {
        private readonly OmniDbContext _dbContext;
        private readonly Microsoft.Extensions.Caching.Memory.IMemoryCache _cache;

        public AnalyticsService(OmniDbContext dbContext, Microsoft.Extensions.Caching.Memory.IMemoryCache cache)
        {
            _dbContext = dbContext;
            _cache = cache;
        }

        public async Task<DailyMetricsDto> GetDailyMetricsAsync()
        {
            const string cacheKey = "DailyMetrics_CacheKey";
            if (_cache.TryGetValue(cacheKey, out DailyMetricsDto? cachedMetrics) && cachedMetrics != null)
            {
                return cachedMetrics;
            }

            var today = DateTime.Today;
            var tomorrow = today.AddDays(1);

            var query = _dbContext.WeighingHistories
                .AsNoTracking()
                .Where(w => w.Timestamp >= today && w.Timestamp < tomorrow);

            var totalWeighings = await query.CountAsync();
            var totalNetWeight = await query.SumAsync(w => w.GrossWeight - w.Tare);

            var lastDoc = await _dbContext.Documents
                .AsNoTracking()
                .OrderByDescending(d => d.CreatedAt)
                .Select(d => d.DocumentNumber)
                .FirstOrDefaultAsync();

            var recentTransactions = await query
                .Include(w => w.Product)
                .Include(w => w.Session).ThenInclude(s => s.Document).ThenInclude(d => d.Client)
                .OrderByDescending(w => w.Timestamp)
                .Take(10)
                .Select(w => new RecentTransactionDto
                {
                    Timestamp = w.Timestamp,
                    ClientName = w.Session != null && w.Session.Document != null && w.Session.Document.Client != null ? w.Session.Document.Client.Name : "N/A",
                    ProductName = w.Product != null ? w.Product.Name : "N/A",
                    NetWeight = w.GrossWeight - w.Tare,
                    Unit = w.Unit,
                    DocumentReference = w.Session != null && w.Session.Document != null ? w.Session.Document.DocumentNumber : "-"
                })
                .ToListAsync();

            var metrics = new DailyMetricsDto
            {
                TotalWeighingsToday = totalWeighings,
                TotalNetWeightToday = totalNetWeight,
                LastDocumentReference = lastDoc ?? "-",
                RecentTransactions = recentTransactions
            };

            var cacheOptions = new Microsoft.Extensions.Caching.Memory.MemoryCacheEntryOptions()
                .SetAbsoluteExpiration(TimeSpan.FromMinutes(2));

            _cache.Set(cacheKey, metrics, cacheOptions);

            return metrics;
        }

        private IQueryable<WeighingHistory> BuildBaseQuery(ReportFilter filter)
        {
            var query = _dbContext.WeighingHistories
                .AsNoTracking()
                .Include(w => w.Product)
                .Include(w => w.Session).ThenInclude(s => s.Document).ThenInclude(d => d.Client)
                .AsQueryable();

            if (filter.StartDate.HasValue) query = query.Where(w => w.Timestamp >= filter.StartDate.Value);
            if (filter.EndDate.HasValue) query = query.Where(w => w.Timestamp <= filter.EndDate.Value);
            if (filter.ProductId.HasValue) query = query.Where(w => w.ProductId == filter.ProductId.Value);
            if (filter.ClientId.HasValue) query = query.Where(w => w.Session != null && w.Session.Document != null && w.Session.Document.ClientId == filter.ClientId.Value);
            if (!string.IsNullOrEmpty(filter.DocumentType))
            {
                if (Enum.TryParse<DocumentType>(filter.DocumentType, out var docType))
                {
                    query = query.Where(w => w.Session != null && w.Session.Document != null && w.Session.Document.Type == docType);
                }
            }

            return query;
        }

        public async Task<DashboardKpiDto> GetDashboardKpisAsync(ReportFilter filter)
        {
            var query = BuildBaseQuery(filter);
            
            var totalVolume = await query.SumAsync(h => h.GrossWeight - h.Tare);
            
            var totalSessions = await query.Select(h => h.SessionId).Distinct().CountAsync();

            var topProduct = await query
                .Where(h => h.Product != null)
                .GroupBy(h => h.Product.Name)
                .Select(g => new { Name = g.Key, Volume = g.Sum(h => h.GrossWeight - h.Tare) })
                .OrderByDescending(x => x.Volume)
                .Select(x => x.Name)
                .FirstOrDefaultAsync() ?? "-";

            var topClient = await query
                .Where(h => h.Session != null && h.Session.Document != null && h.Session.Document.Client != null)
                .GroupBy(h => h.Session.Document.Client.Name)
                .Select(g => new { Name = g.Key, Volume = g.Sum(h => h.GrossWeight - h.Tare) })
                .OrderByDescending(x => x.Volume)
                .Select(x => x.Name)
                .FirstOrDefaultAsync() ?? "-";

            return new DashboardKpiDto
            {
                TotalVolume = totalVolume,
                TotalSessions = totalSessions,
                TopProduct = topProduct,
                TopClient = topClient,
                AverageWeightPerSession = totalSessions > 0 ? totalVolume / totalSessions : 0
            };
        }

        public async Task<List<TimeSeriesDataPoint>> GetVolumeTimeSeriesAsync(ReportFilter filter)
        {
            var query = BuildBaseQuery(filter);
            
            var data = await query
                .GroupBy(w => w.Timestamp.Date)
                .Select(g => new TimeSeriesDataPoint
                {
                    Date = g.Key,
                    Value = g.Sum(x => x.GrossWeight - x.Tare)
                })
                .OrderBy(x => x.Date)
                .ToListAsync();

            return data;
        }

        public async Task<List<CategoricalDataPoint>> GetProductDistributionAsync(ReportFilter filter)
        {
            var query = BuildBaseQuery(filter);

            var data = await query
                .Where(w => w.Product != null)
                .GroupBy(w => w.Product.Name)
                .Select(g => new CategoricalDataPoint
                {
                    Category = g.Key,
                    Value = g.Sum(x => x.GrossWeight - x.Tare)
                })
                .OrderByDescending(x => x.Value)
                .ToListAsync();

            return data;
        }

        public async Task<List<CategoricalDataPoint>> GetPeakActivityHoursAsync(ReportFilter filter)
        {
            var query = BuildBaseQuery(filter);

            // Grouping by Hour. EF Core can translate w.Timestamp.Hour in SQLite if configured properly.
            // But to be safe across providers, we might need to fetch data or ensure translation.
            // Since this is SQLite, some date functions might need client-side if not mapped, but EF Core 6+ supports it.
            var data = await query
                .Select(w => new { w.Timestamp, w.GrossWeight, w.Tare })
                .ToListAsync();

            var grouped = data
                .GroupBy(w => w.Timestamp.Hour)
                .Select(g => new CategoricalDataPoint
                {
                    Category = $"{g.Key:00}:00",
                    Value = g.Sum(x => x.GrossWeight - x.Tare)
                })
                .OrderBy(x => x.Category)
                .ToList();

            return grouped;
        }

        public async Task<List<CategoricalDataPoint>> GetDocumentTypeDistributionAsync(ReportFilter filter)
        {
            var query = BuildBaseQuery(filter);

            var data = await query
                .Where(w => w.Session != null && w.Session.Document != null)
                .Select(w => new { w.Session.Document.Type, w.GrossWeight, w.Tare })
                .ToListAsync();

            var grouped = data
                .GroupBy(w => w.Type)
                .Select(g => new CategoricalDataPoint
                {
                    Category = g.Key.ToString(),
                    Value = g.Sum(x => x.GrossWeight - x.Tare)
                })
                .OrderByDescending(x => x.Value)
                .ToList();

            return grouped;
        }

        public async Task<List<CategoricalDataPoint>> GetClientDistributionAsync(ReportFilter filter)
        {
            var query = BuildBaseQuery(filter);

            var data = await query
                .Where(w => w.Session != null && w.Session.Document != null && w.Session.Document.Client != null)
                .Select(w => new { w.Session.Document.Client.Name, w.GrossWeight, w.Tare })
                .ToListAsync();

            var grouped = data
                .GroupBy(w => w.Name)
                .Select(g => new CategoricalDataPoint
                {
                    Category = g.Key,
                    Value = g.Sum(x => x.GrossWeight - x.Tare)
                })
                .OrderByDescending(x => x.Value)
                .Take(10) // Top 10 clients
                .ToList();

            return grouped;
        }
    }
}
