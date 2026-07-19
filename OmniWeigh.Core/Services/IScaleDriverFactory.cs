using OmniWeigh.Core.Drivers;

namespace OmniWeigh.Core.Services
{
    public interface IScaleDriverFactory
    {
        IBalanceDriver CreateCurrentDriver();
        Task<IBalanceDriver> CreateAndConnectDriverAsync();
    }
}
