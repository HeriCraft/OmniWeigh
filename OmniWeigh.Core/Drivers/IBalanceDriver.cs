namespace OmniWeigh.Core.Drivers
{
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
