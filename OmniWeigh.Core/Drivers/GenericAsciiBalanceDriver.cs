using System;
using System.IO.Ports;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace OmniWeigh.Core.Drivers
{
    public class GenericAsciiBalanceDriver : IBalanceDriver
    {
        private SerialPort? _serialPort;
        private static readonly Regex _weightRegex = new(@"[-+]?[0-9]*\.?[0-9]+", RegexOptions.Compiled | RegexOptions.CultureInvariant);

        public string BrandName => "Generic / Ohaus";

        public string ModelName => "Standard ASCII Stream";

        public bool IsConnected => _serialPort?.IsOpen ?? false;

        public event EventHandler<double>? WeightReceived;
        public event EventHandler<WeightReading>? WeightReadingReceived;
        public event EventHandler<string>? ConnectionError;

        public Task ConnectedAsync(string portName, int baudRate)
        {
            if (_serialPort != null && _serialPort.IsOpen) return Task.CompletedTask;

            _serialPort = new SerialPort(portName, baudRate, Parity.None, 8, StopBits.One)
            {
                ReadTimeout = 1000,
                WriteTimeout = 1000,
                NewLine = "\r\n"
            };

            _serialPort.DataReceived += OnDataReceived;

            try
            {
                _serialPort.Open();
                return Task.CompletedTask;
            }
            catch (Exception ex)
            {
                ConnectionError?.Invoke(this, $"Impossible d'ouvrir le port {portName} : {ex.Message}");
                throw new InvalidOperationException($"Impossible d'ouvrir le port {portName} : {ex.Message}");
            }
        }

        private void OnDataReceived(object? sender, SerialDataReceivedEventArgs e)
        {
            try
            {
                if (_serialPort == null || !_serialPort.IsOpen) return;

                string rawData = _serialPort.ReadLine();
                if (string.IsNullOrWhiteSpace(rawData)) return;

                var match = _weightRegex.Match(rawData);
                if (match.Success && double.TryParse(match.Value, out double result))
                {
                    var rounded = Math.Round(result, 2);
                    WeightReceived?.Invoke(this, rounded);
                    WeightReadingReceived?.Invoke(this, new WeightReading(rounded, true, rawData));
                }
            }
            catch (Exception ex)
            {
                ConnectionError?.Invoke(this, $"Erreur lors de la lecture des données : {ex.Message}");
            }
        }

        public Task DisconnectAsync()
        {
            Dispose();
            return Task.CompletedTask;
        }

        public void Dispose()
        {
            try
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
            catch
            {
                // swallow disposal exceptions to avoid throwing from finalizers
            }
        }
    }
}
