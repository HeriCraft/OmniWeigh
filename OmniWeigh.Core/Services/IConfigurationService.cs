using OmniWeigh.Core.Models;

namespace OmniWeigh.Core.Services
{
    public interface IConfigurationService
    {
        Task<HardwareProfile> GetHardwareProfileAsync();
        Task SaveHardwareProfileAsync(HardwareProfile profile);
        
        Task<Company?> GetCompanyAsync();
        Task SaveCompanyAsync(Company company);
    }
}
