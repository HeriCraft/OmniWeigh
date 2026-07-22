namespace OmniWeigh.Core.Services
{
    public enum DeviceStatus
    {
        Offline,
        Connecting,
        Online,
        Error
    }

    public class HardwareState
    {
        public DeviceStatus ScaleStatus { get; init; } = DeviceStatus.Offline;
        public string ScalePort { get; init; } = string.Empty;
        public DeviceStatus PrinterStatus { get; init; } = DeviceStatus.Offline;
    }

    public class HardwareStateChangedEventArgs : System.EventArgs
    {
        public HardwareState State { get; }

        public HardwareStateChangedEventArgs(HardwareState state)
        {
            State = state;
        }
    }
}
