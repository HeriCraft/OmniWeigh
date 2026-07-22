using System;

namespace OmniWeigh.Core.Services
{
    public class HardwareStateProvider : IHardwareStateProvider
    {
        private readonly object _lock = new object();
        private HardwareState _currentState = new HardwareState();

        public HardwareState CurrentState
        {
            get
            {
                lock (_lock)
                {
                    return _currentState;
                }
            }
        }

        public event EventHandler<HardwareStateChangedEventArgs>? StateChanged;

        public void UpdateScaleState(DeviceStatus status, string port = "")
        {
            lock (_lock)
            {
                _currentState = new HardwareState
                {
                    ScaleStatus = status,
                    ScalePort = port,
                    PrinterStatus = _currentState.PrinterStatus
                };
            }
            StateChanged?.Invoke(this, new HardwareStateChangedEventArgs(_currentState));
        }

        public void UpdatePrinterState(DeviceStatus status)
        {
            lock (_lock)
            {
                _currentState = new HardwareState
                {
                    ScaleStatus = _currentState.ScaleStatus,
                    ScalePort = _currentState.ScalePort,
                    PrinterStatus = status
                };
            }
            StateChanged?.Invoke(this, new HardwareStateChangedEventArgs(_currentState));
        }
    }
}
