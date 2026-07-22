using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Printing;
using System.Threading.Tasks;
using System.Windows;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using CommunityToolkit.Mvvm.Messaging;
using OmniWeigh.Core.Drivers;
using OmniWeigh.Core.Services;
using OmniWeigh.Core.Services.DTOs;
using OmniWeigh.Desktop.Messages;
using System.Windows.Threading;

namespace OmniWeigh.Desktop.ViewModels
{
    public partial class AccueilViewModel : ObservableObject
    {
        private readonly IAnalyticsService _analyticsService;
        private readonly IWeighingHistoryQueryService _historyQueryService;
        private readonly IBalanceDriver _balanceDriver;

        private readonly DispatcherTimer _clockTimer;

        public AccueilViewModel(
            IAnalyticsService analyticsService,
            IWeighingHistoryQueryService historyQueryService,
            IBalanceDriver balanceDriver)
        {
            _analyticsService = analyticsService;
            _historyQueryService = historyQueryService;
            _balanceDriver = balanceDriver;

            RecentActivities = new ObservableCollection<WeighingHistoryItemDto>();
            
            // Subscribe to balance connection events if needed, for simplicity we'll just check status during init
            UpdateHardwareStatus();

            _clockTimer = new DispatcherTimer { Interval = TimeSpan.FromSeconds(1) };
            _clockTimer.Tick += (s, e) => UpdateClock();
            _clockTimer.Start();
            UpdateClock();

            _balanceDriver.WeightReadingReceived += OnWeightReadingReceived;
        }

        private void UpdateClock()
        {
            CurrentTime = DateTime.Now.ToString("HH:mm:ss");
            CurrentDate = DateTime.Now.ToString("dddd d MMMM yyyy").ToUpper();
        }

        private void OnWeightReadingReceived(object? sender, WeightReading e)
        {
            if (e == null) return;
            Application.Current?.Dispatcher?.InvokeAsync(() =>
            {
                LiveWeight = e.Value.ToString("N0"); // Simple integer display for large weights or adjust precision as needed
            });
        }

        [ObservableProperty]
        private int _totalWeighingsToday;

        [ObservableProperty]
        private double _totalNetWeightToday;

        [ObservableProperty]
        private string _lastDocumentReference = "-";

        [ObservableProperty]
        private string _scaleStatus = "Déconnectée";

        [ObservableProperty]
        private string _scaleStatusColor = "#E74C3C"; // Default red

        [ObservableProperty]
        private string _printerStatus = "Prête (A4)"; // Mock status for now

        [ObservableProperty]
        private string _printerStatusColor = "#3498DB"; // Info blue

        [ObservableProperty]
        private string _currentTime = "";

        [ObservableProperty]
        private string _currentDate = "";

        [ObservableProperty]
        private string _liveWeight = "0";

        public ObservableCollection<WeighingHistoryItemDto> RecentActivities { get; }

        public async Task InitializeAsync()
        {
            await LoadDailyMetricsAsync();
            await LoadRecentActivitiesAsync();
            UpdateHardwareStatus();
        }

        private async Task LoadDailyMetricsAsync()
        {
            var metrics = await _analyticsService.GetDailyMetricsAsync();
            TotalWeighingsToday = metrics.TotalWeighingsToday;
            TotalNetWeightToday = metrics.TotalNetWeightToday;
            LastDocumentReference = string.IsNullOrWhiteSpace(metrics.LastDocumentReference) ? "-" : metrics.LastDocumentReference;
        }

        private async Task LoadRecentActivitiesAsync()
        {
            var filter = new WeighingHistoryFilterDto
            {
                Page = 1,
                PageSize = 10
            };

            var result = await _historyQueryService.GetHistoryAsync(filter);

            Application.Current.Dispatcher.Invoke(() =>
            {
                RecentActivities.Clear();
                foreach (var item in result.Items)
                {
                    RecentActivities.Add(item);
                }
            });
        }

        private void UpdateHardwareStatus()
        {
            ScaleStatus = _balanceDriver.IsConnected ? "Connectée" : "Déconnectée";
            ScaleStatusColor = _balanceDriver.IsConnected ? "#2ECC71" : "#E74C3C"; // Green if connected, Red if not
            
            try
            {
                using var printServer = new LocalPrintServer();
                var defaultPrinter = printServer.DefaultPrintQueue;
                if (defaultPrinter != null)
                {
                    bool isOffline = defaultPrinter.IsOffline;
                    PrinterStatus = isOffline ? $"Hors Ligne ({defaultPrinter.Name})" : $"Prête ({defaultPrinter.Name})";
                    PrinterStatusColor = isOffline ? "#F1C40F" : "#3498DB"; 
                }
                else
                {
                    PrinterStatus = "Aucune impr.";
                    PrinterStatusColor = "#E74C3C";
                }
            }
            catch
            {
                PrinterStatus = "Erreur impr.";
                PrinterStatusColor = "#E74C3C";
            }
        }

        [RelayCommand]
        private void NewWeighingSession()
        {
            WeakReferenceMessenger.Default.Send(new NavigateToPriseDePoidsMessage());
        }

        [RelayCommand]
        private void Reprint(WeighingHistoryItemDto item)
        {
            if (item == null) return;
            // For now, this is a placeholder. In a real app, this would query the document and send to PrinterService
            MessageBox.Show($"Impression du ticket {item.WeighingReference} demandée.", "Impression", MessageBoxButton.OK, MessageBoxImage.Information);
        }
        
        [RelayCommand]
        private void ViewDetails(WeighingHistoryItemDto item)
        {
            if (item == null) return;
            MessageBox.Show($"Détails pour {item.WeighingReference}\nClient: {item.ClientName}\nProduit: {item.ProductName}\nPoids Net: {item.NetWeight} {item.Unit}", "Détails", MessageBoxButton.OK, MessageBoxImage.Information);
        }
    }
}
