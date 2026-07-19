using OmniWeigh.Core.Models;

namespace OmniWeigh.Core.Services
{
    public interface IConfigurationRegistry
    {
        HardwareProfile CurrentHardwareProfile { get; }
        Company? CurrentCompany { get; }

        Task InitializeAsync();
        Task UpdateHardwareProfileAsync(HardwareProfile profile);
        Task UpdateCompanyAsync(Company company);

        event EventHandler? ConfigurationChanged;
    }
}
