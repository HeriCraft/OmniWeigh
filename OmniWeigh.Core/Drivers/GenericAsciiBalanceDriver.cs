using System;
using System.IO.Ports;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace OmniWeigh.Core.Drivers
{
    public class GenericAsciiBalanceDriver : IBalanceDriver
    {
        private SerialPort? _serialPort;

        public string BrandName => "Generic / Ohaus";

        public string ModelName => "Standard ASCII Stream";

        public bool IsConnected => _serialPort?.IsOpen ?? false;

        public event EventHandler<double>? WeightReceived;
        public event EventHandler<string>? ConnectionError;

        public Task ConnectedAsync(string portName, int baudRate)
        {
            _serialPort = new SerialPort(portName, baudRate, Parity.None, 8, StopBits.One)
            {
                ReadTimeout = 500,
                WriteTimeout = 500
            };

            _serialPort.DataReceived += OnDataReceived;

            try
            {
                _serialPort.Open();
                return Task.CompletedTask;
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException($"Impossible d'ouvrir le port {portName} : {ex.Message}");
            }
        }
        private void OnDataReceived(object sender, SerialDataReceivedEventArgs e)
        {
            if (_serialPort == null || !_serialPort.IsOpen) return;

            try
            {
                // Lecture de la ligne complète envoyée par la balance
                string rawData = _serialPort.ReadLine();
                double? weight = ExtractWeight(rawData);

                if (weight.HasValue)
                {
                    // On notifie l'application principale (le ViewModel) avec la valeur numérique
                    WeightReceived?.Invoke(this, weight.Value);
                }
            }
            catch (Exception ex)
            {
                ConnectionError?.Invoke(this, $"Erreur lors de la lecture des données : {ex.Message}");
            }
        }
        private double? ExtractWeight(string input)
        {
            if (string.IsNullOrWhiteSpace(input)) return null;

            // Regex pour capturer un nombre décimal (ex: +124.50 ou 50.3)
            var match = Regex.Match(input, @"[-+]?[0-9]*\.?[0-9]+");

            if (match.Success && double.TryParse(match.Value, out double result))
            {
                return result;
            }
            return null;
        }

        public Task DisconnectAsync()
        {
            Dispose();
            return Task.CompletedTask;
        }

        public void Dispose()
        {
            if (_serialPort != null)
            {
                _serialPort.DataReceived -= OnDataReceived;
                if (_serialPort.IsOpen)
                {
                    _serialPort.Close();
                }
                _serialPort.Dispose();
                _serialPort = null;
            }
        }
    }
}
