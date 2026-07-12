namespace OmniWeigh.Core.Drivers
{
    // Lightweight immutable record to transport richer weight frames from drivers
    public record WeightReading(double Value, bool IsStable, string RawFrame);

    public interface IBalanceDriver : IDisposable
    {
        string BrandName { get; }
        string ModelName { get; }
        bool IsConnected { get; }

        // Backwards-compatible simple event (double) kept for existing consumers
        event EventHandler<double>? WeightReceived;

        // New richer event providing metadata if available
        event EventHandler<WeightReading>? WeightReadingReceived;

        // Non-fatal connection errors
        event EventHandler<string>? ConnectionError;

        // Connect/Disconnect the driver. Implementations should honor cancellation when possible.
        Task ConnectedAsync(string portName, int baudRate);
        Task DisconnectAsync();

        // Default convenience overloads added to allow future async cancellation-aware APIs without
        // breaking existing implementations. Implementations may override these via explicit implementation.
        async Task ConnectedAsync(string portName, int baudRate, CancellationToken cancellationToken)
        {
            // Default behavior: call the non-cancellable method and ignore cancellationToken.
            await ConnectedAsync(portName, baudRate).ConfigureAwait(false);
        }

        async Task DisconnectAsync(CancellationToken cancellationToken)
        {
            await DisconnectAsync().ConfigureAwait(false);
        }
    }
}
