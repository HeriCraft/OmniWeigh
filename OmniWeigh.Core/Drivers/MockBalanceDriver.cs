using System;
using System.Threading;
using System.Threading.Tasks;
using OmniWeigh.Core.Drivers;

namespace OmniWeigh.Core.Drivers
{
    // A deterministic, async-friendly mock driver using PeriodicTimer and CancellationToken
    public sealed class MockBalanceDriver : IBalanceDriver, IAsyncDisposable
    {
        private readonly Random _random = new Random();
        private readonly TimeSpan _period = TimeSpan.FromMilliseconds(200);
        private CancellationTokenSource? _cts;
        private Task? _loopTask;

        private double _targetWeight = 0.0;
        private double _currentWeight = 0.0;

        public string BrandName => "TCS";
        public string ModelName => "BS23511";
        public bool IsConnected { get; private set; }

        public event EventHandler<double>? WeightReceived;
        public event EventHandler<WeightReading>? WeightReadingReceived;
        public event EventHandler<string>? ConnectionError;

        public MockBalanceDriver()
        {
        }

        public Task ConnectedAsync(string portName, int baudRate)
        {
            if (IsConnected) return Task.CompletedTask;

            _cts = new CancellationTokenSource();
            _loopTask = Task.Run(() => LoopAsync(_cts.Token));
            IsConnected = true;
            return Task.CompletedTask;
        }

        public async Task DisconnectAsync()
        {
            if (!IsConnected) return;
            try
            {
                _cts?.Cancel();
                if (_loopTask != null) await _loopTask.ConfigureAwait(false);
            }
            catch (OperationCanceledException) { }
            finally
            {
                IsConnected = false;
                _cts?.Dispose();
                _cts = null;
                _loopTask = null;
            }
        }

        public void SimulateNewWeight(double weight)
        {
            _targetWeight = weight;
        }

        private async Task LoopAsync(CancellationToken ct)
        {
            try
            {
                using var timer = new PeriodicTimer(_period);
                while (await timer.WaitForNextTickAsync(ct).ConfigureAwait(false))
                {
                    // simulate physical damping
                    if (Math.Abs(_currentWeight - _targetWeight) > 0.05)
                    {
                        _currentWeight += (_targetWeight - _currentWeight) * 0.3;
                        _currentWeight += (_random.NextDouble() - 0.5) * 0.1;
                    }
                    else
                    {
                        _currentWeight = _targetWeight;
                    }

                    if (_currentWeight < 0) _currentWeight = 0;

                    var rounded = Math.Round(_currentWeight, 2);
                    WeightReceived?.Invoke(this, rounded);
                    WeightReadingReceived?.Invoke(this, new WeightReading(rounded, Math.Abs(_currentWeight - _targetWeight) < 0.01, rounded.ToString()));
                }
            }
            catch (OperationCanceledException) { }
            catch (Exception ex)
            {
                ConnectionError?.Invoke(this, ex.Message);
            }
        }

        public void Dispose()
        {
            // prefer async disposal but implement sync for compatibility
            _cts?.Cancel();
            _cts?.Dispose();
            _cts = null;
        }

        public async ValueTask DisposeAsync()
        {
            await DisconnectAsync().ConfigureAwait(false);
        }
    }
}
