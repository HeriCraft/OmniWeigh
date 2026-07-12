using System;
using System.ComponentModel;
using System.IO;
using System.Collections.ObjectModel;
using System.Runtime.CompilerServices;
using System.Windows.Input;

namespace OmniWeigh.Desktop.ViewModels
{
    public class NewProductViewModel : INotifyPropertyChanged
    {
        public NewProductViewModel()
        {
            SaveCommand = new RelayCommand(_ => OnSave());
            CancelCommand = new RelayCommand(_ => OnCancel());
        }

        public int? ProductId { get; set; }

        private string _name = string.Empty;
        private string _imageSourcePath = string.Empty; // original file picked
        private decimal _unitPrice = 0m;
        private string _currency = "MGA";

        public string Name { get => _name; set { _name = value; OnPropertyChanged(); } }
        public string ImageSourcePath { get => _imageSourcePath; set { _imageSourcePath = value; OnPropertyChanged(); } }
        public decimal UnitPrice { get => _unitPrice; set { _unitPrice = value; OnPropertyChanged(); } }
        public string Currency { get => _currency; set { _currency = value; OnPropertyChanged(); } }

        public ICommand SaveCommand { get; }
        public ICommand CancelCommand { get; }

        public event EventHandler<bool>? RequestClose;

        private void OnSave()
        {
            // Tous les champs sont obligatoires sauf l'image
            if (string.IsNullOrWhiteSpace(Name)) return;
            // UnitPrice par défaut 0 mais il est considéré comme obligatoire : autoriser 0 mais on s'assure que la valeur est présente
            if (UnitPrice < 0) return;
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
