using System;

namespace OmniWeigh.Core.Services
{
    public interface IHardwareStateProvider
    {
        HardwareState CurrentState { get; }
        event EventHandler<HardwareStateChangedEventArgs>? StateChanged;

        void UpdateScaleState(DeviceStatus status, string port = "");
        void UpdatePrinterState(DeviceStatus status);
    }
}
