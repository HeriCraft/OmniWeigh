using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows.Input;
using OmniWeigh.Core.Drivers;
using OmniWeigh.Desktop.Drivers;

namespace OmniWeigh.Desktop.ViewModels
{
    public class WeighingViewModel : INotifyPropertyChanged
    {
        private readonly IBalanceDriver _balanceDriver;

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

            // On démarre la connexion simulée immédiatement pour la maquette
            _ = _balanceDriver.ConnectedAsync("COM5", 9600);

            // On pose un poids initial de 2.00 kg
            mock.SimulateNewWeight(2.00);
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

        // --- Commandes ---
        public ICommand EnregistrerCommand { get; }
        public ICommand ImprimerCommand { get; }
        public ICommand RemiseAZeroCommand { get; }

        private void OnWeightReceived(object? sender, double weightValue)
        {
            // Marshalling vers le Thread UI de WPF
            System.Windows.Application.Current.Dispatcher.Invoke(() =>
            {
                // Détection de stabilité rudimentaire pour la maquette :
                // Si la valeur est identique à la précédente, c'est stable.
                IsStable = Math.Abs(weightValue - _lastWeight) < 0.01;

                CurrentWeight = weightValue;
                FrameCount++;
                _lastWeight = weightValue;
            });
        }

        public event PropertyChangedEventHandler? PropertyChanged;
        protected void OnPropertyChanged([CallerMemberName] string? name = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
        }
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
}