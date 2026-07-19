using System.Collections.Generic;
using System.Threading.Tasks;
using OmniWeigh.Core.Models;

namespace OmniWeigh.Core.Services
{
    public interface ISettingsService
    {
        Task<Company> GetCompanyAsync();
        Task SaveCompanyAsync(Company company);
        Task<string> GetAppSettingAsync(string key, string defaultValue = "");
        Task SaveAppSettingAsync(string key, string value);
        Task SaveAllSettingsAsync(Company company, Dictionary<string, string> appSettings);
    }
}
