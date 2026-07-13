using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows.Input;

namespace OmniWeigh.Desktop.ViewModels
{
    public class NewVehicleViewModel : INotifyPropertyChanged
    {
        public NewVehicleViewModel()
        {
            VehicleTypes = new ObservableCollection<string>(new[] { "Camion", "Crafter", "Sprinter" });
            SaveCommand = new RelayCommand(_ => OnSave());
            CancelCommand = new RelayCommand(_ => OnCancel());
        }

        private string _selectedType = string.Empty;
        private string _registration = string.Empty;
        private string _maxLoad = string.Empty;
        private string _imageSourcePath = string.Empty;

        public ObservableCollection<string> VehicleTypes { get; }
        public string SelectedType { get => _selectedType; set { _selectedType = value; OnPropertyChanged(); } }
        public string Registration { get => _registration; set { _registration = value; OnPropertyChanged(); } }
        public string MaxLoad { get => _maxLoad; set { _maxLoad = value; OnPropertyChanged(); } }
        public string ImageSourcePath { get => _imageSourcePath; set { _imageSourcePath = value; OnPropertyChanged(); } }

        public ICommand SaveCommand { get; }
        public ICommand CancelCommand { get; }

        public event EventHandler<bool>? RequestClose;

        private void OnSave()
        {
            // Validation: Type and Registration mandatory
            if (string.IsNullOrWhiteSpace(SelectedType) || string.IsNullOrWhiteSpace(Registration)) return;
            IsSaved = true;
            RequestClose?.Invoke(this, true);
        }

        private void OnCancel()
        {
            IsSaved = false;
            RequestClose?.Invoke(this, false);
        }

        public bool IsSaved { get; private set; }

        public event PropertyChangedEventHandler? PropertyChanged;
        protected void OnPropertyChanged([CallerMemberName] string? name = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
        }
    }
}
