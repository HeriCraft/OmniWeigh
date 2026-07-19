using System.Text.Json;
using Microsoft.EntityFrameworkCore;
using OmniWeigh.Core.Data;
using OmniWeigh.Core.Models;

namespace OmniWeigh.Core.Services
{
    public class ConfigurationService : IConfigurationService
    {
        private readonly OmniDbContext _dbContext;
        private const string HardwareProfileKey = "HardwareProfile";

        public ConfigurationService(OmniDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        public async Task<HardwareProfile> GetHardwareProfileAsync()
        {
            var setting = await _dbContext.AppSettings.FirstOrDefaultAsync(s => s.Key == HardwareProfileKey);
            if (setting == null || string.IsNullOrWhiteSpace(setting.Value))
            {
                return new HardwareProfile(); // Return defaults
            }

            try
            {
                return JsonSerializer.Deserialize<HardwareProfile>(setting.Value) ?? new HardwareProfile();
            }
            catch
            {
                return new HardwareProfile();
            }
        }

        public async Task SaveHardwareProfileAsync(HardwareProfile profile)
        {
            var json = JsonSerializer.Serialize(profile);
            var setting = await _dbContext.AppSettings.FirstOrDefaultAsync(s => s.Key == HardwareProfileKey);

            if (setting == null)
            {
                setting = new AppSetting { Key = HardwareProfileKey, Value = json };
                _dbContext.AppSettings.Add(setting);
            }
            else
            {
                setting.Value = json;
                _dbContext.AppSettings.Update(setting);
            }

            await _dbContext.SaveChangesAsync();
        }

        public async Task<Company?> GetCompanyAsync()
        {
            return await _dbContext.Companies.FirstOrDefaultAsync();
        }

        public async Task SaveCompanyAsync(Company company)
        {
            var existing = await _dbContext.Companies.FirstOrDefaultAsync();
            if (existing == null)
            {
                _dbContext.Companies.Add(company);
            }
            else
            {
                existing.Name = company.Name;
                existing.Slogan = company.Slogan;
                existing.Address1 = company.Address1;
                existing.Address2 = company.Address2;
                existing.Phone = company.Phone;
                existing.Email = company.Email;
                existing.LogoPath = company.LogoPath;
                _dbContext.Companies.Update(existing);
            }
            await _dbContext.SaveChangesAsync();
        }
    }
}
