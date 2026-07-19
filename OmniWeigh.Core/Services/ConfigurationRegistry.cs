using Microsoft.Extensions.DependencyInjection;
using OmniWeigh.Core.Models;

namespace OmniWeigh.Core.Services
{
    public class ConfigurationRegistry : IConfigurationRegistry
    {
        private readonly IServiceScopeFactory _scopeFactory;
        
        public HardwareProfile CurrentHardwareProfile { get; private set; } = new HardwareProfile();
        public Company? CurrentCompany { get; private set; }

        public event EventHandler? ConfigurationChanged;

        public ConfigurationRegistry(IServiceScopeFactory scopeFactory)
        {
            _scopeFactory = scopeFactory;
        }

        public async Task InitializeAsync()
        {
            using var scope = _scopeFactory.CreateScope();
            var configService = scope.ServiceProvider.GetRequiredService<IConfigurationService>();
            
            CurrentHardwareProfile = await configService.GetHardwareProfileAsync();
            CurrentCompany = await configService.GetCompanyAsync();
            
            NotifyChanged();
        }

        public async Task UpdateHardwareProfileAsync(HardwareProfile profile)
        {
            using var scope = _scopeFactory.CreateScope();
            var configService = scope.ServiceProvider.GetRequiredService<IConfigurationService>();
            
            await configService.SaveHardwareProfileAsync(profile);
            CurrentHardwareProfile = profile;
            
            NotifyChanged();
        }

        public async Task UpdateCompanyAsync(Company company)
        {
            using var scope = _scopeFactory.CreateScope();
            var configService = scope.ServiceProvider.GetRequiredService<IConfigurationService>();
            
            await configService.SaveCompanyAsync(company);
            CurrentCompany = company;
            
            NotifyChanged();
        }

        private void NotifyChanged()
        {
            ConfigurationChanged?.Invoke(this, EventArgs.Empty);
        }
    }
}
