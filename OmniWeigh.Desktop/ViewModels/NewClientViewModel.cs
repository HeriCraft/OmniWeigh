using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows.Input;

namespace OmniWeigh.Desktop.ViewModels
{
    public class NewClientViewModel : INotifyPropertyChanged
    {
        public NewClientViewModel()
        {
            Countries = new ObservableCollection<string>
            {
                "Madagascar",
                "France",
                "Côte d'Ivoire",
                "Mali",
                " Sénégal",
                "Autre"
            };
            Country = "Madagascar";
            SaveCommand = new RelayCommand(_ => OnSave());
            CancelCommand = new RelayCommand(_ => OnCancel());
        }

        // Required
        private string _name = string.Empty;
        private string _phone = string.Empty;
        private string _email = string.Empty;

        // Optional
        private string _address1 = string.Empty;
        private string _address2 = string.Empty;
        private string _city = string.Empty;
        private string _postalCode = string.Empty;
        private string _country = string.Empty;

        public string Name { get => _name; set { _name = value; OnPropertyChanged(); } }
        public string Phone { get => _phone; set { _phone = value; OnPropertyChanged(); } }
        public string Email { get => _email; set { _email = value; OnPropertyChanged(); } }

        public string Address1 { get => _address1; set { _address1 = value; OnPropertyChanged(); } }
        public string Address2 { get => _address2; set { _address2 = value; OnPropertyChanged(); } }
        public string City { get => _city; set { _city = value; OnPropertyChanged(); } }
        public string PostalCode { get => _postalCode; set { _postalCode = value; OnPropertyChanged(); } }

        public ObservableCollection<string> Countries { get; }
        public string Country { get => _country; set { _country = value; OnPropertyChanged(); } }

        public ICommand SaveCommand { get; }
        public ICommand CancelCommand { get; }

        // If set, indicates editing existing client
        public int? ClientId { get; set; }

        public event EventHandler<bool>? RequestClose;

        private void OnSave()
        {
            // Basic validation for required fields
            if (string.IsNullOrWhiteSpace(Name) || string.IsNullOrWhiteSpace(Phone) || string.IsNullOrWhiteSpace(Email))
            {
                // In a full app show validation feedback; here we simply do not close
                return;
            }

            IsSaved = true;
            RequestClose?.Invoke(this, true);
        }

        private void OnCancel()
        {
            IsSaved = false;
            RequestClose?.Invoke(this, false);
        }

        public bool IsSaved { get; private set; }

        public ClientItem NewClient => new ClientItem
        {
            Reference = string.Empty,
            Name = Name,
            Phone = Phone,
            Email = Email
        };

        private string GenerateReference()
        {
        // Reference now comes from database Id; keep generator non-operational to avoid conflicts.
        return string.Empty;
        }

        public event PropertyChangedEventHandler? PropertyChanged;
        protected void OnPropertyChanged([CallerMemberName] string? name = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
        }
    }
}
