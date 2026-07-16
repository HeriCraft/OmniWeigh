using OmniWeigh.Core.Models;
using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;
using CoreDocument = OmniWeigh.Core.Models.Document;

namespace OmniWeigh.Core.Services
{
    public class QuestPdfExportService : IDocumentExportService
    {
        public QuestPdfExportService()
        {
            // Set license type for QuestPDF to Community
            QuestPDF.Settings.License = LicenseType.Community;
        }

        public async Task ExportToPdfAsync(CoreDocument document, string outputPath, PrintLayout layout)
        {
            var pdfDocument = CreateQuestPdfDocument(document, layout);
            
            // Run asynchronously if possible, though QuestPDF GeneratePdf is mostly CPU-bound synchronous work
            await Task.Run(() => pdfDocument.GeneratePdf(outputPath));
        }

        public async Task PrintToLocalPrinterAsync(CoreDocument document, string printerName, PrintLayout layout)
        {
            var pdfDocument = CreateQuestPdfDocument(document, layout);
            
            // Generate PDF into a memory stream
            using var stream = new MemoryStream();
            pdfDocument.GeneratePdf(stream);
            stream.Position = 0;

            // In a real WPF app, printing a PDF stream can be done via PdfiumViewer, RawPrint, or SumatraPDF.
            // For now, this is a placeholder where we would implement the exact printer communication logic.
            await Task.CompletedTask;
            Console.WriteLine($"[PrintService] Sending {document.DocumentNumber} to printer '{printerName}'...");
        }

        private IDocument CreateQuestPdfDocument(CoreDocument doc, PrintLayout layout)
        {
            return QuestPDF.Fluent.Document.Create(container =>
            {
                if (layout == PrintLayout.StandardA4)
                {
                    container.Page(page =>
                    {
                        page.Size(PageSizes.A4);
                        page.Margin(1, Unit.Centimetre);
                        page.PageColor(Colors.White);
                        page.DefaultTextStyle(x => x.FontSize(11));

                        page.Header().Element(c => ComposeHeader(c, doc));
                        page.Content().Element(c => ComposeContent(c, doc));
                        page.Footer().Element(c => ComposeFooter(c, doc));
                    });
                }
                else if (layout == PrintLayout.Thermal80mm)
                {
                    container.Page(page =>
                    {
                        page.ContinuousSize(80, Unit.Millimetre);
                        page.Margin(2, Unit.Millimetre);
                        page.PageColor(Colors.White);
                        page.DefaultTextStyle(x => x.FontSize(9));

                        page.Header().Element(c => ComposeThermalHeader(c, doc));
                        page.Content().Element(c => ComposeThermalContent(c, doc));
                    });
                }
            });
        }

        private void ComposeHeader(IContainer container, CoreDocument doc)
        {
            container.Row(row =>
            {
                row.RelativeItem().Column(column =>
                {
                    column.Item().Text(doc.Type.ToString()).FontSize(20).SemiBold().FontColor(Colors.Blue.Darken2);
                    column.Item().Text($"Ref: {doc.DocumentNumber}");
                    column.Item().Text($"Date: {doc.CreatedAt:dd/MM/yyyy HH:mm}");
                });

                row.ConstantItem(100).Height(50).Placeholder(); // Company Logo
            });
        }

        private void ComposeContent(IContainer container, CoreDocument doc)
        {
            container.PaddingVertical(1, Unit.Centimetre).Column(column =>
            {
                column.Spacing(5);
                
                // Details Info
                column.Item().Text("Client Info").SemiBold();
                column.Item().Text(doc.Client?.Name ?? "N/A");
                
                column.Item().PaddingTop(10).Text("Chauffeur").SemiBold();
                column.Item().Text(doc.DriverName);

                // Table of Weighing Sessions
                column.Item().PaddingTop(15).Table(table =>
                {
                    table.ColumnsDefinition(columns =>
                    {
                        columns.RelativeColumn(3); // Ref
                        columns.RelativeColumn(2); // Gross
                        columns.RelativeColumn(2); // Tare
                        columns.RelativeColumn(2); // Net
                        columns.RelativeColumn(1); // Unit
                    });

                    table.Header(header =>
                    {
                        header.Cell().Text("Réf. Pesée");
                        header.Cell().AlignRight().Text("Brut");
                        header.Cell().AlignRight().Text("Tare");
                        header.Cell().AlignRight().Text("Net");
                        header.Cell().Text("Unité");
                    });

                    foreach (var session in doc.WeighingSessions)
                    {
                        foreach (var record in session.HistoryRecords)
                        {
                            table.Cell().Text(record.WeighingReference);
                            table.Cell().AlignRight().Text(record.GrossWeight.ToString("F2"));
                            table.Cell().AlignRight().Text(record.Tare.ToString("F2"));
                            table.Cell().AlignRight().Text(record.NetWeight.ToString("F2"));
                            table.Cell().Text(record.Unit.ToString());
                        }
                    }
                });
                
                if (!string.IsNullOrEmpty(doc.SignatureBase64))
                {
                    column.Item().PaddingTop(20).Text("Signature:");
                    try 
                    {
                        var imageBytes = Convert.FromBase64String(doc.SignatureBase64);
                        column.Item().Width(150).Image(imageBytes);
                    }
                    catch 
                    {
                        column.Item().Text("[Image de signature invalide]").FontColor(Colors.Red.Medium);
                    }
                }
            });
        }

        private void ComposeFooter(IContainer container, CoreDocument doc)
        {
            container.AlignCenter().Text(x =>
            {
                x.Span("Page ");
                x.CurrentPageNumber();
                x.Span(" sur ");
                x.TotalPages();
            });
        }

        private void ComposeThermalHeader(IContainer container, CoreDocument doc)
        {
            container.Column(column =>
            {
                column.Item().AlignCenter().Text(doc.Type.ToString()).Bold().FontSize(12);
                column.Item().AlignCenter().Text(doc.DocumentNumber);
                column.Item().AlignCenter().Text(doc.CreatedAt.ToString("dd/MM/yy HH:mm"));
                column.Item().LineHorizontal(1).LineColor(Colors.Grey.Medium);
            });
        }

        private void ComposeThermalContent(IContainer container, CoreDocument doc)
        {
            container.PaddingVertical(5).Column(column =>
            {
                column.Item().Text($"Chauffeur: {doc.DriverName}");
                column.Item().Text($"Client: {doc.Client?.Name}");
                column.Item().LineHorizontal(1).LineColor(Colors.Grey.Lighten2);

                foreach (var session in doc.WeighingSessions)
                {
                    foreach (var record in session.HistoryRecords)
                    {
                        column.Item().Text(record.WeighingReference).Bold();
                        column.Item().Text($"  Brut: {record.GrossWeight:F2}");
                        column.Item().Text($"  Tare: {record.Tare:F2}");
                        column.Item().Text($"  Net:  {record.NetWeight:F2} {record.Unit}");
                    }
                }

                if (!string.IsNullOrEmpty(doc.SignatureBase64))
                {
                    column.Item().LineHorizontal(1).LineColor(Colors.Grey.Lighten2);
                    column.Item().PaddingTop(5).Text("Signé");
                }
            });
        }
    }
}
