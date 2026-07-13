using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using System.Windows.Data;
using System.Windows.Input;
using Microsoft.Extensions.Logging;
using OmniWeigh.Core.Drivers;
using System.Data;
using OmniWeigh.Core.Services.DTOs;

namespace OmniWeigh.Desktop.ViewModels
{
    public class WeighingViewModel : INotifyPropertyChanged, IDisposable
    {
        private readonly IBalanceDriver _balanceDriver;
        private readonly OmniWeigh.Core.Services.IClientService _clientService;
        private readonly OmniWeigh.Core.Services.IProductService _productService;
        private readonly OmniWeigh.Core.Services.IVehicleService _vehicleService;
        private readonly ILogger<WeighingViewModel> _logger;

        // States
        private double _currentWeight;
        private bool _isStable = true;
        private int _frameCount;
        private string _balanceModel = "Loading...";
        private string _comPort = "COM5";
        private string _operationMode = "Continu (TRN2)";
        private double _lastWeight = -1.0;

        // Document info
        private bool _isDeliveryNote = true;
        private string _documentNumber = "BL-000125";
        private string _operatorName = "ANDRIAM.";
        private string _reference = string.Empty;
        private string _vehiclePlate = "1234 TBA";
        private string _selectedMenu = "Accueil";

        public WeighingViewModel(IBalanceDriver balanceDriver, OmniWeigh.Core.Services.IClientService clientService, OmniWeigh.Core.Services.IProductService productService, OmniWeigh.Core.Services.IVehicleService vehicleService, ILogger<WeighingViewModel> logger)
        {
            _balanceDriver = balanceDriver ?? throw new ArgumentNullException(nameof(balanceDriver));
            _clientService = clientService ?? throw new ArgumentNullException(nameof(clientService));
            _productService = productService ?? throw new ArgumentNullException(nameof(productService));
            _vehicleService = vehicleService ?? throw new ArgumentNullException(nameof(vehicleService));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));

            BalanceModel = $"{_balanceDriver.BrandName} {_balanceDriver.ModelName}";

            // Commands
            RemiseAZeroCommand = new RelayCommand(_ =>
            {
                if (_balanceDriver is OmniWeigh.Core.Drivers.MockBalanceDriver mock)
                    mock.SimulateNewWeight(0.0);
            });

            EnregistrerCommand = new RelayCommand(_ => { /* persist weighing via Core service - implement when needed */ });
            ImprimerCommand = new RelayCommand(_ => { /* print ticket - implement */ });
            SelectMenuCommand = new RelayCommand(p => SelectedMenu = p?.ToString() ?? string.Empty);
            OpenNewClientCommand = new RelayCommand(_ => OpenNewClient());
            OpenNewProductCommand = new RelayCommand(_ => OpenNewProduct());
            OpenNewVehicleCommand = new RelayCommand(_ => OpenNewVehicle());
            ClearClientSearchCommand = new RelayCommand(_ => ClientSearchQuery = string.Empty);

            // Collections and views
            ClientsList = new ObservableCollection<ClientItem>();
            ClientsView = CollectionViewSource.GetDefaultView(ClientsList);
            ClientsView.Filter = ClientFilter;

            ProductsList = new ObservableCollection<ProductItem>();
            VehiclesList = new ObservableCollection<VehicleItem>();
            VehicleTypes = new ObservableCollection<string>(new[] { "Camion", "Crafter", "Sprinter" });

            // Vehicle types shared collection (allows adding new types at runtime)
            VehicleTypes = new ObservableCollection<string>(new[] { "Camion", "Crafter", "Sprinter" });

            // Vehicles will be loaded during InitializeAsync from IVehicleService

        }

        public async Task InitializeAsync()
        {
            try
            {
                _balanceDriver.WeightReceived += OnWeightReceived;
                _ = _balanceDriver.ConnectedAsync(ComPort, 9600);

                if (_balanceDriver is OmniWeigh.Core.Drivers.MockBalanceDriver mock)
                    mock.SimulateNewWeight(2.00);

                var clients = await _clientService.GetAllAsync().ConfigureAwait(false);
                var products = await _productService.GetAllAsync().ConfigureAwait(false);
                var vehicles = await _vehicleService.GetAllAsync().ConfigureAwait(false);

                System.Windows.Application.Current?.Dispatcher.Invoke(() =>
                {
                    foreach (var c in clients)
                    {
                        ClientsList.Add(new ClientItem
                        {
                            Id = c.Id,
                            Reference = c.Reference,
                            Name = c.Name,
                            Phone = c.Phone,
                            Email = c.Email
                        });
                    }

                    foreach (var p in products)
                    {
                        ProductsList.Add(new ProductItem
                        {
                            Id = p.Id,
                            Reference = p.Reference,
                            Name = p.Name,
                            ImagePath = p.ImageFileName is not null ? System.IO.Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "OmniWeigh", "images", p.ImageFileName) : string.Empty
                        });
                    }

                    foreach (var v in vehicles)
                    {
                        VehiclesList.Add(new VehicleItem
                        {
                            Id = v.Id,
                            Registration = v.Registration,
                            Type = v.Type,
                            MaxLoad = v.MaxLoad ?? string.Empty,
                            ImagePath = v.ImageFileName is not null ? System.IO.Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "OmniWeigh", "images", v.ImageFileName) : string.Empty
                        });
                    }
                });

                _logger.LogInformation("WeighingViewModel initialized: {Clients} clients, {Products} products", ClientsList.Count, ProductsList.Count);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Initialization failed");
            }
        }

        private bool ClientFilter(object obj)
        {
            if (obj is not ClientItem client) return false;
            if (string.IsNullOrWhiteSpace(ClientSearchQuery)) return true;
            var q = ClientSearchQuery.Trim();
            return (client.Name?.IndexOf(q, StringComparison.OrdinalIgnoreCase) ?? -1) >= 0
                || (client.Reference?.IndexOf(q, StringComparison.OrdinalIgnoreCase) ?? -1) >= 0
                || (client.Email?.IndexOf(q, StringComparison.OrdinalIgnoreCase) ?? -1) >= 0
                || (client.Phone?.IndexOf(q, StringComparison.OrdinalIgnoreCase) ?? -1) >= 0;
        }

        // --- Bindable properties ---
        public double CurrentWeight { get => _currentWeight; private set { _currentWeight = value; OnPropertyChanged(); OnPropertyChanged(nameof(PoidsBrut)); OnPropertyChanged(nameof(PoidsNet)); } }
        public bool IsStable { get => _isStable; private set { _isStable = value; OnPropertyChanged(); OnPropertyChanged(nameof(StabilityStatus)); } }
        public string StabilityStatus => IsStable ? "Stable" : "En cours...";
        public double PoidsBrut => CurrentWeight;
        public double Tare { get; set; } = 0.00;
        public double PoidsNet => PoidsBrut - Tare;

        public string BalanceModel { get => _balanceModel; set { _balanceModel = value; OnPropertyChanged(); } }
        public string ComPort { get => _comPort; set { _comPort = value; OnPropertyChanged(); } }
        public string OperationMode { get => _operationMode; set { _operationMode = value; OnPropertyChanged(); } }
        public int FrameCount { get => _frameCount; private set { _frameCount = value; OnPropertyChanged(); } }

        public string DocumentNumber { get => _documentNumber; set { _documentNumber = value; OnPropertyChanged(); } }
        public string OperatorName { get => _operatorName; set { _operatorName = value; OnPropertyChanged(); } }
        public string Reference { get => _reference; set { _reference = value; OnPropertyChanged(); } }
        public bool IsDeliveryNote { get => _isDeliveryNote; set { _isDeliveryNote = value; OnPropertyChanged(); } }

        public string VehiclePlate { get => _vehiclePlate; set { _vehiclePlate = value; OnPropertyChanged(); if (_balanceDriver is OmniWeigh.Core.Drivers.MockBalanceDriver mock) { double weight = value == "1234 TBA" ? 2.00 : 1450.50; mock.SimulateNewWeight(weight); } } }

        public ObservableCollection<string> Clients { get; } = new() { "MADAGASCAR", "SIMEX-CI", "LOGISTIQUE S.A." };
        public ObservableCollection<string> Produits { get; } = new() { "SAVON 200g", "HUILE BRUTE", "MATIÈRE PREMIÈRE" };
        public ObservableCollection<string> Vehicules { get; } = new() { "1234 TBA", "5678 TAA", "9876 TEB" };
        public ObservableCollection<string> VehicleTypes { get; }

        public ObservableCollection<ClientItem> ClientsList { get; }
        public ObservableCollection<ProductItem> ProductsList { get; }
        public ObservableCollection<VehicleItem> VehiclesList { get; }

        public ICollectionView ClientsView { get; }

        public ICommand EnregistrerCommand { get; }
        public ICommand ImprimerCommand { get; }
        public ICommand RemiseAZeroCommand { get; }
        public ICommand SelectMenuCommand { get; }
        public ICommand OpenNewClientCommand { get; }
        public ICommand OpenNewProductCommand { get; }
        public ICommand ClearClientSearchCommand { get; }
        public ICommand OpenNewVehicleCommand { get; }

        private string _clientSearchQuery = string.Empty;
        public string ClientSearchQuery { get => _clientSearchQuery; set { _clientSearchQuery = value; OnPropertyChanged(); ClientsView?.Refresh(); } }

        public string SelectedMenu { get => _selectedMenu; set { _selectedMenu = value; OnPropertyChanged(); } }

        private void OnWeightReceived(object? sender, double weightValue)
        {
            try
            {
                var dispatcher = System.Windows.Application.Current?.Dispatcher ?? System.Windows.Threading.Dispatcher.CurrentDispatcher;
                if (dispatcher != null && !dispatcher.CheckAccess())
                {
                    dispatcher.Invoke(() => UpdateWeightState(weightValue));
                }
                else
                {
                    UpdateWeightState(weightValue);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "OnWeightReceived error");
            }
        }

        private void UpdateWeightState(double weightValue)
        {
            IsStable = Math.Abs(weightValue - _lastWeight) < 0.01;
            CurrentWeight = weightValue;
            FrameCount++;
            _lastWeight = weightValue;
        }

        private void OpenNewClient()
        {
            var vm = new NewClientViewModel();
            var win = new OmniWeigh.Desktop.Views.NewClientWindow
            {
                DataContext = vm
            };

            var result = win.ShowDialog();
            if (result == true && vm.IsSaved)
            {
                try
                {
                    var contact = new
                    {
                        phone = vm.Phone,
                        email = vm.Email,
                        address1 = vm.Address1,
                        address2 = vm.Address2,
                        city = vm.City,
                        postalCode = vm.PostalCode,
                        country = vm.Country
                    };

                    var dto = new ClientDto
                    {
                        Name = vm.Name,
                        ContactInfo = System.Text.Json.JsonSerializer.Serialize(contact),
                        Phone = vm.Phone,
                        Email = vm.Email
                    };

                    var saved = Task.Run(() => _clientService.AddAsync(dto)).GetAwaiter().GetResult();

                    var added = new ClientItem
                    {
                        Id = saved.Id,
                        Reference = !string.IsNullOrWhiteSpace(saved.Reference) ? saved.Reference : $"C-{saved.Id:D5}",
                        Name = saved.Name,
                        Phone = saved.Phone,
                        Email = saved.Email
                    };

                    ClientsList.Add(added);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Failed to save client via service");
                }
            }
        }

        private void OpenNewProduct()
        {
            var vm = new NewProductViewModel();
            var win = new OmniWeigh.Desktop.Views.NewProductWindow
            {
                DataContext = vm
            };

            var result = win.ShowDialog();
            if (result == true && vm.IsSaved)
            {
                try
                {
                    var dto = new OmniWeigh.Core.Services.DTOs.ProductDto
                    {
                        Name = vm.Name
                    };

                    var saved = Task.Run(() => _productService.AddAsync(dto, vm.ImageSourcePath)).GetAwaiter().GetResult();

                    var added = new ProductItem
                    {
                        Id = saved.Id,
                        Reference = saved.Reference,
                        Name = saved.Name,
                        ImagePath = saved.ImageFileName is not null ? System.IO.Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "OmniWeigh", "images", saved.ImageFileName) : string.Empty
                    };

                    ProductsList.Add(added);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Failed to save product via service");
                }
            }
        }

        // Open dialog to create a new vehicle, persist and add to list
        public void OpenNewVehicle()
        {
            var vm = new NewVehicleViewModel();
            // seed dialog with existing types so user can add new type
            foreach (var t in VehicleTypes)
                vm.VehicleTypes.Add(t);

            var win = new OmniWeigh.Desktop.Views.NewVehicleWindow
            {
                DataContext = vm
            };

            var result = win.ShowDialog();
            if (result == true && vm.IsSaved)
            {
                try
                {
                    // If new type provided and not existing, add to shared collection
                    if (!string.IsNullOrWhiteSpace(vm.SelectedType) && !VehicleTypes.Contains(vm.SelectedType))
                    {
                        VehicleTypes.Add(vm.SelectedType);
                    }

                    var dto = new OmniWeigh.Core.Services.DTOs.VehicleDto
                    {
                        Registration = vm.Registration,
                        Type = vm.SelectedType,
                        MaxLoad = string.IsNullOrWhiteSpace(vm.MaxLoad) ? null : vm.MaxLoad
                    };

                    var saved = Task.Run(() => _vehicleService.AddAsync(dto, vm.ImageSourcePath)).GetAwaiter().GetResult();

                    VehiclesList.Add(new VehicleItem
                    {
                        Id = saved.Id,
                        Registration = saved.Registration,
                        Type = saved.Type,
                        MaxLoad = saved.MaxLoad ?? string.Empty,
                        ImagePath = saved.ImageFileName is not null ? System.IO.Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "OmniWeigh", "images", saved.ImageFileName) : string.Empty
                    });
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Failed to save vehicle via service");
                }
            }
        }

        // Vehicle table creation and persistence handled by OmniWeigh.Core services

        public event PropertyChangedEventHandler? PropertyChanged;
        protected void OnPropertyChanged([CallerMemberName] string? name = null) => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));

        public void Dispose()
        {
            try
            {
                _balanceDriver.WeightReceived -= OnWeightReceived;
            }
            catch { }
        }
    }

    public class RelayCommand : ICommand
    {
        private readonly Action<object?> _execute;
        public RelayCommand(Action<object?> execute) => _execute = execute;
        public bool CanExecute(object? parameter) => true;
        public void Execute(object? parameter) => _execute(parameter);
        public event EventHandler? CanExecuteChanged;
    }

    public class ClientItem
    {
        public int Id { get; set; }
        public string Reference { get; set; } = string.Empty;
        public string Name { get; set; } = string.Empty;
        public string Phone { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
    }

    public class ProductItem
    {
        public int Id { get; set; }
        public string Reference { get; set; } = string.Empty;
        public string Name { get; set; } = string.Empty;
        public string ImagePath { get; set; } = string.Empty;
    }
    public class VehicleItem
    {
        public int Id { get; set; }
        public string Registration { get; set; } = string.Empty;
        public string Type { get; set; } = string.Empty;
        public string ImagePath { get; set; } = string.Empty;
        public string MaxLoad { get; set; } = string.Empty;
    }
}

