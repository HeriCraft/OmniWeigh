using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text.Json;
using System.ComponentModel;
using System.Windows.Data;
using System.Runtime.CompilerServices;
using System.Windows.Input;
using OmniWeigh.Core.Drivers;

namespace OmniWeigh.Desktop.ViewModels
{
    public class WeighingViewModel : INotifyPropertyChanged
    {
        private readonly IBalanceDriver _balanceDriver;
        private readonly OmniWeigh.Core.Services.IClientService _clientService;

        // États de l'IHM
        private double _currentWeight = 0.00;
        private bool _isStable = true;
        private int _frameCount = 0;
        private string _balanceModel = "Loading...";
        private string _comPort = "COM5";
        private string _operationMode = "Continu (TRN2)";
        private double _lastWeight = -1.0;

        // Informations Document
        private bool _isDeliveryNote = true;
        private string _documentNumber = "BL-000125";
        private string _operatorName = "ANDRIAM.";
        private string _selectedClient = "MADAGASCAR";
        private string _selectedProduct = "SAVON 200g";
        private string _reference = "SAV200G-01";
        private string _vehiclePlate = "1234 TBA";
        private string _selectedMenu = "Accueil";

        public WeighingViewModel()
        {
            // Initialisation avec le mock pour la maquette
            var mock = new MockBalanceDriver();
            _balanceDriver = mock;

            // Déclenchement de la configuration de base
            BalanceModel = $"{_balanceDriver.BrandName} {_balanceDriver.ModelName}";

            // Abonnements aux événements du Core
            _balanceDriver.WeightReceived += OnWeightReceived;

            // Initialisation des commandes
            RemiseAZeroCommand = new RelayCommand(_ => mock.SimulateNewWeight(0.0));
            EnregistrerCommand = new RelayCommand(_ => { /* Sauvegarde BDD */ });
            ImprimerCommand = new RelayCommand(_ => { /* Impression ticket */ });
            SelectMenuCommand = new RelayCommand(p => SelectedMenu = p?.ToString() ?? string.Empty);
            OpenNewClientCommand = new RelayCommand(_ => OpenNewClient());
            ClearClientSearchCommand = new RelayCommand(_ => ClientSearchQuery = string.Empty);

            // Charger la liste des clients depuis le service Core (synchronisé ici pour l'initialisation)
            _clientService = new OmniWeigh.Core.Services.ClientService();
            try
            {
                var clientDtos = System.Threading.Tasks.Task.Run(() => _clientService.GetAllAsync()).GetAwaiter().GetResult();
                foreach (var c in clientDtos)
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
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Failed to load clients from DB via service: {ex}");
            }

            // Setup vue filtrée pour la liste des clients
            ClientsView = CollectionViewSource.GetDefaultView(ClientsList);
            ClientsView.Filter = ClientFilter;

            // On démarre la connexion simulée immédiatement pour la maquette
            _ = _balanceDriver.ConnectedAsync("COM5", 9600);

            // On pose un poids initial de 2.00 kg
            mock.SimulateNewWeight(2.00);
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

        // --- Propriétés de Binding Métrologique ---
        public double CurrentWeight
        {
            get => _currentWeight;
            set
            {
                _currentWeight = value;
                OnPropertyChanged();
                OnPropertyChanged(nameof(PoidsBrut));
                OnPropertyChanged(nameof(PoidsNet));
            }
        }

        public bool IsStable
        {
            get => _isStable;
            set { _isStable = value; OnPropertyChanged(); OnPropertyChanged(nameof(StabilityStatus)); }
        }

        public string StabilityStatus => IsStable ? "Stable" : "En cours...";
        public double PoidsBrut => CurrentWeight;
        public double Tare { get; set; } = 0.00;
        public double PoidsNet => PoidsBrut - Tare;

        // --- Informations Matériel / État ---
        public string BalanceModel { get => _balanceModel; set { _balanceModel = value; OnPropertyChanged(); } }
        public string ComPort { get => _comPort; set { _comPort = value; OnPropertyChanged(); } }
        public string OperationMode { get => _operationMode; set { _operationMode = value; OnPropertyChanged(); } }
        public int FrameCount { get => _frameCount; set { _frameCount = value; OnPropertyChanged(); } }

        // --- Informations Métier ---
        public string DocumentNumber { get => _documentNumber; set { _documentNumber = value; OnPropertyChanged(); } }
        public string OperatorName { get => _operatorName; set { _operatorName = value; OnPropertyChanged(); } }
        public string Reference { get => _reference; set { _reference = value; OnPropertyChanged(); } }
        public bool IsDeliveryNote { get => _isDeliveryNote; set { _isDeliveryNote = value; OnPropertyChanged(); } }

        public string VehiclePlate
        {
            get => _vehiclePlate;
            set
            {
                _vehiclePlate = value;
                OnPropertyChanged();
                // Simulation : Si on change de véhicule, le poids sur la balance change dynamiquement !
                if (_balanceDriver is MockBalanceDriver mock)
                {
                    double nouveauPoidsSimule = value == "1234 TBA" ? 2.00 : 1450.50;
                    mock.SimulateNewWeight(nouveauPoidsSimule);
                }
            }
        }

        public ObservableCollection<string> Clients { get; set; } = new() { "MADAGASCAR", "SIMEX-CI", "LOGISTIQUE S.A." };
        public ObservableCollection<string> Produits { get; set; } = new() { "SAVON 200g", "HUILE BRUTE", "MATIÈRE PREMIÈRE" };
        public ObservableCollection<string> Vehicules { get; set; } = new() { "1234 TBA", "5678 TAA", "9876 TEB" };

        // Liste riche des clients affichée dans la page Clients (chargée depuis la base au démarrage)
        public ObservableCollection<ClientItem> ClientsList { get; set; } = new();

        // --- Commandes ---
        public ICommand EnregistrerCommand { get; }
        public ICommand ImprimerCommand { get; }
        public ICommand RemiseAZeroCommand { get; }
        public ICommand SelectMenuCommand { get; }
        public ICommand OpenNewClientCommand { get; }
        public ICommand ClearClientSearchCommand { get; }

        // Vue filtrée exposée à la View
        public ICollectionView ClientsView { get; private set; }

        private string _clientSearchQuery = string.Empty;
        public string ClientSearchQuery
        {
            get => _clientSearchQuery;
            set
            {
                _clientSearchQuery = value;
                OnPropertyChanged();
                ClientsView?.Refresh();
            }
        }

        public string SelectedMenu
        {
            get => _selectedMenu;
            set { _selectedMenu = value; OnPropertyChanged(); }
        }

// Petit modèle de client pour l'affichage dans la grille (déclaré plus bas)

        private void OnWeightReceived(object? sender, double weightValue)
        {
            try
            {
                // Marshalling vers le Thread UI de WPF si possible, sinon exécute directement
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
                System.Diagnostics.Debug.WriteLine($"OnWeightReceived error: {ex}");
            }
        }

        private void UpdateWeightState(double weightValue)
        {
            // Détection de stabilité rudimentaire pour la maquette :
            // Si la valeur est identique à la précédente, c'est stable.
            IsStable = Math.Abs(weightValue - _lastWeight) < 0.01;

            CurrentWeight = weightValue;
            FrameCount++;
            _lastWeight = weightValue;
        }

        public event PropertyChangedEventHandler? PropertyChanged;
        protected void OnPropertyChanged([CallerMemberName] string? name = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
        }

        private void OpenNewClient()
        {
            // Create VM and window
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

                        var dto = new OmniWeigh.Core.Services.DTOs.ClientDto
                        {
                            Name = vm.Name,
                            ContactInfo = System.Text.Json.JsonSerializer.Serialize(contact),
                            Phone = vm.Phone,
                            Email = vm.Email
                        };

                        var saved = System.Threading.Tasks.Task.Run(() => _clientService.AddAsync(dto)).GetAwaiter().GetResult();

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
                        System.Diagnostics.Debug.WriteLine($"Failed to save client via service: {ex}");
                    }
            }
        }

        // Edit/Delete feature rolled back
    }

    // Un implémentation ultra-simple de ICommand pour éviter d'ajouter des dépendances MVVM externes
    public class RelayCommand : ICommand
    {
        private readonly Action<object?> _execute;
        public RelayCommand(Action<object?> execute) => _execute = execute;
        public bool CanExecute(object? parameter) => true;
        public void Execute(object? parameter) => _execute(parameter);
        public event EventHandler? CanExecuteChanged;
    }

    // Petit modèle de client pour l'affichage dans la grille
    public class ClientItem
    {
        // Database Id
        public int Id { get; set; }
        public string Reference { get; set; } = string.Empty;
        public string Name { get; set; } = string.Empty;
        public string Phone { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
    }
}