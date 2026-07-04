using System;
using System.Threading.Tasks;
using System.Timers;
using OmniWeigh.Core.Drivers;

namespace OmniWeigh.Desktop.Drivers
{
    public class MockBalanceDriver : IBalanceDriver
    {
        private readonly System.Timers.Timer _timer;
        private double _targetWeight = 0.0;
        private double _currentWeight = 0.0;
        private readonly Random _random = new Random();

        public string BrandName => "TCS";
        public string ModelName => "BS23511";
        public bool IsConnected { get; private set; }

        public event EventHandler<double>? WeightReceived;
        public event EventHandler<string>? ConnectionError;

        public MockBalanceDriver()
        {
            _timer = new System.Timers.Timer(200); // Envoi d'une trame toutes les 200ms
            _timer.Elapsed += OnTimerElapsed;
        }

        public Task ConnectedAsync(string portName, int baudRate)
        {
            IsConnected = true;
            _timer.Start();
            return Task.CompletedTask;
        }

        public Task DisconnectAsync()
        {
            IsConnected = false;
            _timer.Stop();
            return Task.CompletedTask;
        }

        // Méthode pour simuler le dépôt d'un camion/colis sur la balance depuis l'IHM
        public void SimulateNewWeight(double weight)
        {
            _targetWeight = weight;
        }

        private void OnTimerElapsed(object? sender, ElapsedEventArgs e)
        {
            if (!IsConnected) return;

            // Effet d'amortissement pour simuler la stabilisation physique de la balance
            if (Math.Abs(_currentWeight - _targetWeight) > 0.05)
            {
                _currentWeight += (_targetWeight - _currentWeight) * 0.3;
                // Ajout d'un léger bruit mécanique avant stabilisation
                _currentWeight += (_random.NextDouble() - 0.5) * 0.1;
            }
            else
            {
                _currentWeight = _targetWeight;
            }

            if (_currentWeight < 0) _currentWeight = 0;

            WeightReceived?.Invoke(this, Math.Round(_currentWeight, 2));
        }

        public void Dispose()
        {
            _timer.Dispose();
        }
    }
}