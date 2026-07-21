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
    public class ReportQueryService : IReportQueryService
    {
        private readonly OmniDbContext _dbContext;

        public ReportQueryService(OmniDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        public async Task<List<ReportAggregationDto>> GetAggregatedReportAsync(ReportFilter filter)
        {
            var query = _dbContext.WeighingHistories.AsNoTracking().AsQueryable();

            if (filter.StartDate.HasValue)
                query = query.Where(h => h.Timestamp >= filter.StartDate.Value);
            
            if (filter.EndDate.HasValue)
                query = query.Where(h => h.Timestamp <= filter.EndDate.Value);

            if (filter.ClientId.HasValue)
                query = query.Where(h => h.Session.Document.ClientId == filter.ClientId.Value);

            if (filter.ProductId.HasValue)
                query = query.Where(h => h.ProductId == filter.ProductId.Value);

            if (!string.IsNullOrWhiteSpace(filter.DocumentType))
            {
                if (Enum.TryParse<DocumentType>(filter.DocumentType, out var docType))
                {
                    query = query.Where(h => h.Session.Document.Type == docType);
                }
            }

            if (filter.GroupBy == GroupByMode.ByClient)
            {
                var groupedByClient = await query
                    .GroupBy(h => new { GroupName = h.Session.Document.Client.Name, Unit = h.Unit })
                    .Select(g => new ReportAggregationDto
                    {
                        GroupName = g.Key.GroupName,
                        Unit = g.Key.Unit,
                        TotalPesees = g.Count(),
                        PoidsBrutTotal = g.Sum(x => x.GrossWeight),
                        TareTotal = g.Sum(x => x.Tare),
                        PoidsNetTotal = g.Sum(x => x.GrossWeight - x.Tare),
                        MoyennePoidsNet = g.Average(x => (double?)(x.GrossWeight - x.Tare)) ?? 0
                    })
                    .ToListAsync();
                
                return groupedByClient.OrderBy(x => x.GroupName).ThenBy(x => x.Unit).ToList();
            }
            else
            {
                var groupedByProduct = await query
                    .GroupBy(h => new { GroupName = h.Product.Name, Unit = h.Unit })
                    .Select(g => new ReportAggregationDto
                    {
                        GroupName = g.Key.GroupName,
                        Unit = g.Key.Unit,
                        TotalPesees = g.Count(),
                        PoidsBrutTotal = g.Sum(x => x.GrossWeight),
                        TareTotal = g.Sum(x => x.Tare),
                        PoidsNetTotal = g.Sum(x => x.GrossWeight - x.Tare),
                        MoyennePoidsNet = g.Average(x => (double?)(x.GrossWeight - x.Tare)) ?? 0
                    })
                    .ToListAsync();
                    
                return groupedByProduct.OrderBy(x => x.GroupName).ThenBy(x => x.Unit).ToList();
            }
        }
    }
}
