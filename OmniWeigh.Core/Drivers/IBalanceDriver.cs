namespace OmniWeigh.Core.Drivers
{
    public record WeightReading(double Value, bool IsStable, string RawFrame);
    public interface IBalanceDriver : IDisposable
    {
        string BrandName { get; }
        string ModelName { get; }
        bool IsConnected { get; }

        event EventHandler<double>? WeightReceived;

        event EventHandler<string>? ConnectionError;

        Task ConnectedAsync(string portName, int baudRate);
        Task DisconnectAsync();
    }
}
