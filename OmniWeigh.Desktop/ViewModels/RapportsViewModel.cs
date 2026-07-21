using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using LiveChartsCore;
using LiveChartsCore.SkiaSharpView;
using LiveChartsCore.SkiaSharpView.Painting;
using OmniWeigh.Core.Services;
using SkiaSharp;
using ClosedXML.Excel;
using Microsoft.Win32;
using OmniWeigh.Core.Data;
using Microsoft.EntityFrameworkCore;
using System.Windows;

namespace OmniWeigh.Desktop.ViewModels
{
    public partial class RapportsViewModel : ObservableObject
    {
        private readonly IAnalyticsService _analyticsService;
        private readonly IReportExportService _reportExportService;

        public RapportsViewModel(IAnalyticsService analyticsService, IReportExportService reportExportService)
        {
            _analyticsService = analyticsService;
            _reportExportService = reportExportService;
            
            StartDate = DateTime.Now.AddDays(-30);
            EndDate = DateTime.Now;
        }

        [ObservableProperty]
        private DateTime _startDate;

        [ObservableProperty]
        private DateTime _endDate;

        [ObservableProperty]
        private double _totalVolume;

        [ObservableProperty]
        private int _totalSessions;

        [ObservableProperty]
        private string _topProduct = "-";

        [ObservableProperty]
        private string _topClient = "-";

        [ObservableProperty]
        private double _averageWeightPerSession;

        [ObservableProperty]
        private ISeries[] _volumeSeries = Array.Empty<ISeries>();

        public SolidColorPaint LegendTextPaint { get; set; } = new SolidColorPaint(new SKColor(255, 255, 255));
        public SolidColorPaint TooltipTextPaint { get; set; } = new SolidColorPaint(new SKColor(255, 255, 255));

        [ObservableProperty]
        private ISeries[] _productDistributionSeries = Array.Empty<ISeries>();

        [ObservableProperty]
        private ISeries[] _peakActivitySeries = Array.Empty<ISeries>();

        [ObservableProperty]
        private ISeries[] _documentTypeSeries = Array.Empty<ISeries>();

        [ObservableProperty]
        private ISeries[] _clientDistributionSeries = Array.Empty<ISeries>();

        [ObservableProperty]
        private Axis[] _xAxes = new[] { new Axis { 
            Labeler = value => (value < DateTime.MinValue.Ticks || value > DateTime.MaxValue.Ticks) 
                ? string.Empty 
                : new DateTime((long)value).ToString("dd/MM"),
            LabelsPaint = new SolidColorPaint(new SKColor(160, 178, 198)),
            SeparatorsPaint = new SolidColorPaint(new SKColor(27, 46, 74))
        } };

        [ObservableProperty]
        private Axis[] _yAxes = new[] { new Axis { 
            LabelsPaint = new SolidColorPaint(new SKColor(160, 178, 198)),
            SeparatorsPaint = new SolidColorPaint(new SKColor(27, 46, 74))
        } };

        [ObservableProperty]
        private Axis[] _peakActivityXAxes = new[] { new Axis { 
            LabelsRotation = 0,
            LabelsPaint = new SolidColorPaint(new SKColor(160, 178, 198)),
            SeparatorsPaint = new SolidColorPaint(new SKColor(27, 46, 74))
        } };

        [ObservableProperty]
        private Axis[] _peakActivityYAxes = new[] { new Axis { 
            LabelsPaint = new SolidColorPaint(new SKColor(160, 178, 198)),
            SeparatorsPaint = new SolidColorPaint(new SKColor(27, 46, 74))
        } };

        [ObservableProperty]
        private Axis[] _clientDistributionXAxes = new[] { new Axis { 
            LabelsPaint = new SolidColorPaint(new SKColor(160, 178, 198)),
            SeparatorsPaint = new SolidColorPaint(new SKColor(27, 46, 74))
        } };

        [ObservableProperty]
        private Axis[] _clientDistributionYAxes = new[] { new Axis { 
            LabelsRotation = 0,
            LabelsPaint = new SolidColorPaint(new SKColor(160, 178, 198)),
            SeparatorsPaint = new SolidColorPaint(new SKColor(27, 46, 74))
        } };



        public async Task InitializeAsync()
        {
            await RefreshDataAsync();
        }

        [RelayCommand]
        private async Task RefreshDataAsync()
        {
            var filter = new OmniWeigh.Core.Services.DTOs.ReportFilter { StartDate = StartDate, EndDate = EndDate };

            // Load KPIs
            var kpis = await _analyticsService.GetDashboardKpisAsync(filter);
            TotalVolume = kpis.TotalVolume;
            TotalSessions = kpis.TotalSessions;
            TopProduct = kpis.TopProduct;
            TopClient = kpis.TopClient;
            AverageWeightPerSession = kpis.AverageWeightPerSession;

            // Load Line Chart (Volume)
            var timeSeries = await _analyticsService.GetVolumeTimeSeriesAsync(filter);
            
            var values = timeSeries.Select(x => new LiveChartsCore.Defaults.DateTimePoint(x.Date, x.Value)).ToList();
            VolumeSeries = new ISeries[]
            {
                new LineSeries<LiveChartsCore.Defaults.DateTimePoint>
                {
                    Values = values,
                    Fill = null,
                    Name = "Volume (kg)",
                    GeometrySize = 10,
                    LineSmoothness = 0.5
                }
            };

            XAxes[0].LabelsRotation = 15;
            XAxes[0].UnitWidth = TimeSpan.FromDays(1).Ticks;
            XAxes[0].MinStep = TimeSpan.FromDays(1).Ticks;

            // Load Pie Chart (Product Distribution)
            var catSeries = await _analyticsService.GetProductDistributionAsync(filter);
            var pieSeriesList = new List<ISeries>();
            foreach (var item in catSeries)
            {
                pieSeriesList.Add(new PieSeries<double>
                {
                    Values = new[] { item.Value },
                    Name = item.Category,
                    InnerRadius = 60 // Make it a Donut chart
                });
            }
            ProductDistributionSeries = pieSeriesList.ToArray();

            // Load Peak Activity Hours
            var peakData = await _analyticsService.GetPeakActivityHoursAsync(filter);
            PeakActivitySeries = new ISeries[]
            {
                new ColumnSeries<double>
                {
                    Values = peakData.Select(x => x.Value).ToArray(),
                    Name = "Activité (kg)",
                    Fill = new SolidColorPaint(new SKColor(50, 115, 246)),
                    MaxBarWidth = 40
                }
            };
            PeakActivityXAxes[0].Labels = peakData.Select(x => x.Category).ToArray();

            // Load Document Type Distribution (Donut)
            var docTypeData = await _analyticsService.GetDocumentTypeDistributionAsync(filter);
            var docPieSeriesList = new List<ISeries>();
            foreach (var item in docTypeData)
            {
                docPieSeriesList.Add(new PieSeries<double>
                {
                    Values = new[] { item.Value },
                    Name = item.Category,
                    InnerRadius = 60
                });
            }
            DocumentTypeSeries = docPieSeriesList.ToArray();

            // Load Client Distribution (Horizontal Bar)
            var clientData = await _analyticsService.GetClientDistributionAsync(filter);
            ClientDistributionSeries = new ISeries[]
            {
                new RowSeries<double>
                {
                    Values = clientData.Select(x => x.Value).ToArray(),
                    Name = "Volume Transporté (kg)",
                    Fill = new SolidColorPaint(new SKColor(46, 204, 113)),
                    DataLabelsPaint = new SolidColorPaint(new SKColor(255, 255, 255)),
                    DataLabelsPosition = LiveChartsCore.Measure.DataLabelsPosition.End
                }
            };
            ClientDistributionYAxes[0].Labels = clientData.Select(x => x.Category).ToArray();
        }

        [RelayCommand]
        private async Task SetPeriodAsync(string period)
        {
            switch (period)
            {
                case "Today":
                    StartDate = DateTime.Today;
                    EndDate = DateTime.Today.AddDays(1).AddTicks(-1);
                    break;
                case "7Days":
                    StartDate = DateTime.Today.AddDays(-7);
                    EndDate = DateTime.Now;
                    break;
                case "ThisMonth":
                    StartDate = new DateTime(DateTime.Today.Year, DateTime.Today.Month, 1);
                    EndDate = DateTime.Now;
                    break;
                default:
                    return;
            }
            await RefreshDataAsync();
        }

        [RelayCommand]
        private async Task ExportToExcelAsync()
        {
            var dialog = new SaveFileDialog
            {
                Filter = "Excel Files|*.xlsx",
                FileName = $"Export_OmniWeigh_{DateTime.Now:yyyyMMdd}.xlsx",
                Title = "Exporter les données"
            };

            if (dialog.ShowDialog() == true)
            {
                try
                {
                    var filter = new OmniWeigh.Core.Services.DTOs.ReportFilter { StartDate = StartDate, EndDate = EndDate };
                    var bytes = await _reportExportService.GenerateExcelReportAsync(filter);
                    System.IO.File.WriteAllBytes(dialog.FileName, bytes);

                    MessageBox.Show("Exportation terminée avec succès.", "Succès", MessageBoxButton.OK, MessageBoxImage.Information);
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Erreur lors de l'exportation : {ex.Message}", "Erreur", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
        }

        [RelayCommand]
        private async Task ExportToPdfAsync()
        {
            var dialog = new SaveFileDialog
            {
                Filter = "PDF Files|*.pdf",
                FileName = $"Rapport_OmniWeigh_{DateTime.Now:yyyyMMdd}.pdf",
                Title = "Générer un Rapport PDF"
            };

            if (dialog.ShowDialog() == true)
            {
                try
                {
                    var filter = new OmniWeigh.Core.Services.DTOs.ReportFilter { StartDate = StartDate, EndDate = EndDate };
                    var bytes = await _reportExportService.GeneratePdfReportAsync(filter);
                    System.IO.File.WriteAllBytes(dialog.FileName, bytes);

                    MessageBox.Show("Rapport PDF généré avec succès.", "Succès", MessageBoxButton.OK, MessageBoxImage.Information);
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Erreur lors de la génération du PDF : {ex.Message}", "Erreur", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
        }
    }
}
