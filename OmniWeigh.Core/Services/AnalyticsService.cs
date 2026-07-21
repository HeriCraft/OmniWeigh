using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using OmniWeigh.Core.Data;
using OmniWeigh.Core.Models;
using OmniWeigh.Core.Services.DTOs;

namespace OmniWeigh.Core.Services
{
    public class AnalyticsService : IAnalyticsService
    {
        private readonly OmniDbContext _dbContext;

        public AnalyticsService(OmniDbContext dbContext)
        {
            _dbContext = dbContext;
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
