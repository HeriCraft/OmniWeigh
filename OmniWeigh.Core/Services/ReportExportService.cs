using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Threading.Tasks;
using OmniWeigh.Core.Models;
using OmniWeigh.Core.Services.DTOs;
using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;

namespace OmniWeigh.Core.Services
{
    public class ReportExportService : IReportExportService
    {
        private readonly IConfigurationRegistry _configRegistry;
        private readonly IReportQueryService _queryService;

        public ReportExportService(IConfigurationRegistry configRegistry, IReportQueryService queryService)
        {
            _configRegistry = configRegistry;
            _queryService = queryService;
            QuestPDF.Settings.License = LicenseType.Community;
        }

        public async Task<byte[]> GenerateExcelReportAsync(ReportFilter filter)
        {
            var data = await _queryService.GetAggregatedReportAsync(filter);
            
            // Use UTF-8 with BOM for Excel compatibility (CSV representation)
            using var memoryStream = new MemoryStream();
            var encoding = new UTF8Encoding(encoderShouldEmitUTF8Identifier: true);
            using var writer = new StreamWriter(memoryStream, encoding, bufferSize: 8192, leaveOpen: true);

            await writer.WriteLineAsync("Groupe;Unité;Nombre de pesées;Poids Brut Total;Tare Totale;Poids Net Total;Moyenne Poids Net");

            foreach (var item in data)
            {
                var line = $"\"{item.GroupName}\";\"{item.Unit}\";{item.TotalPesees};{item.PoidsBrutTotal:F2};{item.TareTotal:F2};{item.PoidsNetTotal:F2};{item.MoyennePoidsNet:F2}";
                await writer.WriteLineAsync(line);
            }

            await writer.FlushAsync();
            memoryStream.Position = 0;
            return memoryStream.ToArray();
        }

        public async Task<byte[]> GeneratePdfReportAsync(ReportFilter filter)
        {
            var data = await _queryService.GetAggregatedReportAsync(filter);
            var company = _configRegistry.CurrentCompany;
            
            var document = QuestPDF.Fluent.Document.Create(container =>
            {
                container.Page(page =>
                {
                    page.Size(PageSizes.A4);
                    page.Margin(1, Unit.Centimetre);
                    page.PageColor(Colors.White);
                    page.DefaultTextStyle(x => x.FontSize(11));

                    page.Header().Element(c => ComposeHeader(c, company, filter));
                    page.Content().Element(c => ComposeContent(c, data, filter));
                    page.Footer().Element(ComposeFooter);
                });
            });

            using var ms = new MemoryStream();
            document.GeneratePdf(ms);
            return ms.ToArray();
        }

        private void ComposeHeader(IContainer container, Company? company, ReportFilter filter)
        {
            container.Row(row =>
            {
                row.RelativeItem().Column(column =>
                {
                    column.Item().Text("Rapport Exécutif").FontSize(20).SemiBold().FontColor(Colors.Blue.Darken2);
                    column.Item().Text($"Agrégation par : {filter.GroupBy}");
                    
                    var dateStr = (filter.StartDate, filter.EndDate) switch
                    {
                        (null, null) => "Toutes les dates",
                        (var start, null) => $"Depuis le {start:dd/MM/yyyy}",
                        (null, var end) => $"Jusqu'au {end:dd/MM/yyyy}",
                        (var start, var end) => $"Du {start:dd/MM/yyyy} au {end:dd/MM/yyyy}"
                    };
                    column.Item().Text($"Période : {dateStr}");
                    column.Item().Text($"Généré le : {DateTime.Now:dd/MM/yyyy HH:mm}");
                });

                if (company != null)
                {
                    row.ConstantItem(150).Column(c => 
                    {
                        c.Item().AlignRight().Text(company.Name).SemiBold();
                        c.Item().AlignRight().Text(company.Phone);
                    });
                }
            });
        }

        private void ComposeContent(IContainer container, IEnumerable<ReportAggregationDto> data, ReportFilter filter)
        {
            container.PaddingVertical(1, Unit.Centimetre).Column(column =>
            {
                column.Spacing(5);
                
                column.Item().Table(table =>
                {
                    table.ColumnsDefinition(columns =>
                    {
                        columns.RelativeColumn(3);
                        columns.RelativeColumn(1);
                        columns.RelativeColumn(1);
                        columns.RelativeColumn(2);
                        columns.RelativeColumn(2);
                        columns.RelativeColumn(2);
                        columns.RelativeColumn(2);
                    });

                    table.Header(header =>
                    {
                        header.Cell().BorderBottom(1).BorderColor(Colors.Grey.Lighten2).PaddingBottom(5).Text("Groupe").SemiBold();
                        header.Cell().BorderBottom(1).BorderColor(Colors.Grey.Lighten2).PaddingBottom(5).Text("Unité").SemiBold();
                        header.Cell().BorderBottom(1).BorderColor(Colors.Grey.Lighten2).PaddingBottom(5).AlignRight().Text("Nb Pesées").SemiBold();
                        header.Cell().BorderBottom(1).BorderColor(Colors.Grey.Lighten2).PaddingBottom(5).AlignRight().Text("Poids Brut").SemiBold();
                        header.Cell().BorderBottom(1).BorderColor(Colors.Grey.Lighten2).PaddingBottom(5).AlignRight().Text("Tare").SemiBold();
                        header.Cell().BorderBottom(1).BorderColor(Colors.Grey.Lighten2).PaddingBottom(5).AlignRight().Text("Poids Net").SemiBold();
                        header.Cell().BorderBottom(1).BorderColor(Colors.Grey.Lighten2).PaddingBottom(5).AlignRight().Text("Moyenne").SemiBold();
                    });

                    foreach (var item in data)
                    {
                        table.Cell().BorderBottom(1).BorderColor(Colors.Grey.Lighten4).PaddingVertical(5).Text(item.GroupName);
                        table.Cell().BorderBottom(1).BorderColor(Colors.Grey.Lighten4).PaddingVertical(5).Text(item.Unit.ToString());
                        table.Cell().BorderBottom(1).BorderColor(Colors.Grey.Lighten4).PaddingVertical(5).AlignRight().Text(item.TotalPesees.ToString());
                        table.Cell().BorderBottom(1).BorderColor(Colors.Grey.Lighten4).PaddingVertical(5).AlignRight().Text(item.PoidsBrutTotal.ToString("F2"));
                        table.Cell().BorderBottom(1).BorderColor(Colors.Grey.Lighten4).PaddingVertical(5).AlignRight().Text(item.TareTotal.ToString("F2"));
                        table.Cell().BorderBottom(1).BorderColor(Colors.Grey.Lighten4).PaddingVertical(5).AlignRight().Text(item.PoidsNetTotal.ToString("F2"));
                        table.Cell().BorderBottom(1).BorderColor(Colors.Grey.Lighten4).PaddingVertical(5).AlignRight().Text(item.MoyennePoidsNet.ToString("F2"));
                    }
                });
            });
        }

        private void ComposeFooter(IContainer container)
        {
            container.AlignCenter().Text(x =>
            {
                x.Span("Page ");
                x.CurrentPageNumber();
                x.Span(" sur ");
                x.TotalPages();
            });
        }
    }
}
