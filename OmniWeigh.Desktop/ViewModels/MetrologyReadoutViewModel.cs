using System;
using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace OmniWeigh.Desktop.ViewModels
{
    public class MetrologyReadoutViewModel : INotifyPropertyChanged
    {
        private string _weight = "0.00";
        private string _unit = "kg";
        private bool _isStable;
        private bool _tareActive;
        private DateTime _lastUpdate = DateTime.MinValue;

        public event PropertyChangedEventHandler? PropertyChanged;

        public string Weight
        {
            get => _weight;
            set => SetProperty(ref _weight, value);
        }

        public string Unit
        {
            get => _unit;
            set => SetProperty(ref _unit, value);
        }

        public bool IsStable
        {
            get => _isStable;
            set => SetProperty(ref _isStable, value);
        }

        public bool TareActive
        {
            get => _tareActive;
            set => SetProperty(ref _tareActive, value);
        }

        public DateTime LastUpdate
        {
            get => _lastUpdate;
            set => SetProperty(ref _lastUpdate, value);
        }

        protected bool SetProperty<T>(ref T field, T value, [CallerMemberName] string? propertyName = null)
        {
            if (Equals(field, value))
                return false;
            field = value;
            OnPropertyChanged(propertyName);
            return true;
        }

        protected void OnPropertyChanged([CallerMemberName] string? propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        // Helper to update from a domain model or driver payload (keeps UI mapping centralized)
        public void UpdateFromTelemetry(decimal weightValue, string unit, bool isStable, bool tareActive, DateTime timestamp)
        {
            Weight = weightValue.ToString("F2");
            Unit = unit;
            IsStable = isStable;
            TareActive = tareActive;
            LastUpdate = timestamp;
        }
    }
}
