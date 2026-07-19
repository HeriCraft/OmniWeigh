using System.Collections.ObjectModel;
using System.Threading.Tasks;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using CommunityToolkit.Mvvm.Messaging;
using OmniWeigh.Core.Services;
using OmniWeigh.Core.Services.DTOs;
using OmniWeigh.Desktop.Messages;

namespace OmniWeigh.Desktop.ViewModels
{
    public partial class HistoriqueViewModel : ObservableObject, IRecipient<WeighingSavedMessage>
    {
        private readonly IWeighingSessionService _weighingSessionService;

        public HistoriqueViewModel(IWeighingSessionService weighingSessionService)
        {
            _weighingSessionService = weighingSessionService;
            HistoryRecords = new ObservableCollection<HistoryRecordDto>();
            
            WeakReferenceMessenger.Default.Register(this);
        }

        public ObservableCollection<HistoryRecordDto> HistoryRecords { get; }

        [ObservableProperty]
        [NotifyPropertyChangedFor(nameof(CanGoPrevious))]
        private int _currentPage = 1;

        [ObservableProperty]
        [NotifyPropertyChangedFor(nameof(CanGoNext))]
        private int _totalPages = 1;

        [ObservableProperty]
        private int _totalItems = 0;

        private int _pageSize = 50;

        public bool CanGoPrevious => CurrentPage > 1;
        public bool CanGoNext => CurrentPage < TotalPages;

        [ObservableProperty]
        private string? _filterClient;

        [ObservableProperty]
        private string? _filterProduct;

        [ObservableProperty]
        private double _filterMinWeight = 0;

        [ObservableProperty]
        private double _filterMaxWeight = 100000;

        public async Task InitializeAsync()
        {
            await LoadPageAsync(1);
        }

        private async Task LoadPageAsync(int page)
        {
            var filter = new HistoryFilterDto
            {
                ClientName = FilterClient,
                ProductName = FilterProduct,
                MinWeight = FilterMinWeight > 0 ? FilterMinWeight : null,
                MaxWeight = FilterMaxWeight < 100000 ? FilterMaxWeight : null
            };

            var result = await _weighingSessionService.GetHistoryAsync(page, _pageSize, filter);
            
            HistoryRecords.Clear();
            foreach (var record in result.Records)
            {
                HistoryRecords.Add(record);
            }

            TotalItems = result.TotalCount;
            TotalPages = (TotalItems + _pageSize - 1) / _pageSize;
            if (TotalPages == 0) TotalPages = 1;
            
            CurrentPage = page;
        }

        [RelayCommand(CanExecute = nameof(CanGoPrevious))]
        private async Task PreviousPageAsync()
        {
            if (CanGoPrevious)
            {
                await LoadPageAsync(CurrentPage - 1);
            }
        }

        [RelayCommand(CanExecute = nameof(CanGoNext))]
        private async Task NextPageAsync()
        {
            if (CanGoNext)
            {
                await LoadPageAsync(CurrentPage + 1);
            }
        }

        [RelayCommand]
        private async Task ApplyFiltersAsync()
        {
            await LoadPageAsync(1);
        }

        [RelayCommand]
        private async Task ResetFiltersAsync()
        {
            FilterClient = null;
            FilterProduct = null;
            FilterMinWeight = 0;
            FilterMaxWeight = 100000;
            await LoadPageAsync(1);
        }

        public void Receive(WeighingSavedMessage message)
        {
            // Update the UI on the main thread
            System.Windows.Application.Current.Dispatcher.InvokeAsync(() =>
            {
                var record = message.NewRecord;

                // Check if it matches current active filters
                if (!string.IsNullOrWhiteSpace(FilterClient) && !record.ClientName.Contains(FilterClient, System.StringComparison.OrdinalIgnoreCase))
                    return;

                if (!string.IsNullOrWhiteSpace(FilterProduct) && !record.ProductName.Contains(FilterProduct, System.StringComparison.OrdinalIgnoreCase))
                    return;

                if (FilterMinWeight > 0 && record.NetWeight < FilterMinWeight)
                    return;

                if (FilterMaxWeight < 100000 && record.NetWeight > FilterMaxWeight)
                    return;

                // Push seamlessly into the collection without full reload if we are on page 1
                if (CurrentPage == 1)
                {
                    HistoryRecords.Insert(0, record);

                    // Respect page size by removing the last item if we exceed it
                    if (HistoryRecords.Count > _pageSize)
                    {
                        HistoryRecords.RemoveAt(HistoryRecords.Count - 1);
                    }
                }

                TotalItems++;
                TotalPages = (TotalItems + _pageSize - 1) / _pageSize;
                if (TotalPages == 0) TotalPages = 1;
            });
        }
    }
}
