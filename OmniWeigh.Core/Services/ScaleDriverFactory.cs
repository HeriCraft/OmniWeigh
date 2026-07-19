using System;
using System.Threading.Tasks;
using OmniWeigh.Core.Drivers;

namespace OmniWeigh.Core.Services
{
    public class ScaleDriverFactory : IScaleDriverFactory
    {
        private readonly IConfigurationRegistry _registry;

        public ScaleDriverFactory(IConfigurationRegistry registry)
        {
            _registry = registry;
        }

        public IBalanceDriver CreateCurrentDriver()
        {
            var profile = _registry.CurrentHardwareProfile;
            
            return profile.DriverType switch
            {
                "GenericAscii" => new GenericAsciiBalanceDriver(),
                "Mock" => new MockBalanceDriver(),
                _ => new MockBalanceDriver() // Fallback
            };
        }

        public async Task<IBalanceDriver> CreateAndConnectDriverAsync()
        {
            var driver = CreateCurrentDriver();
            var profile = _registry.CurrentHardwareProfile;
            
            await driver.ConnectedAsync(profile.ComPort, profile.BaudRate);
            
            return driver;
        }
    }
}
