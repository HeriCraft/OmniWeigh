using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using OmniWeigh.Core.Data;
using OmniWeigh.Core.Models;

namespace OmniWeigh.Core.Services
{
    public class SettingsService : ISettingsService
    {
        private readonly OmniDbContext _dbContext;

        public SettingsService(OmniDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        public async Task<Company> GetCompanyAsync()
        {
            await _dbContext.Database.EnsureCreatedAsync();
            var company = await _dbContext.Companies.FirstOrDefaultAsync(c => c.Id == 1);
            if (company == null)
            {
                company = new Company { Id = 1, Name = "Mon Entreprise", Slogan = "", Address1 = "", Address2 = "", Phone = "", Email = "", LogoPath = "" };
                _dbContext.Companies.Add(company);
                await _dbContext.SaveChangesAsync();
            }
            return company;
        }

        public async Task SaveCompanyAsync(Company company)
        {
            await _dbContext.Database.EnsureCreatedAsync();
            
            // Handle logo copy to local app data folder to prevent broken links
            if (!string.IsNullOrWhiteSpace(company.LogoPath) && File.Exists(company.LogoPath))
            {
                var appDataFolder = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData);
                var imagesFolder = Path.Combine(appDataFolder, "OmniWeigh", "images");
                Directory.CreateDirectory(imagesFolder);

                if (!company.LogoPath.StartsWith(imagesFolder, StringComparison.OrdinalIgnoreCase))
                {
                    string extension = Path.GetExtension(company.LogoPath);
                    string newFileName = $"logo_{Guid.NewGuid()}{extension}";
                    string newPath = Path.Combine(imagesFolder, newFileName);
                    File.Copy(company.LogoPath, newPath, true);
                    company.LogoPath = newPath;
                }
            }

            var existing = await _dbContext.Companies.FirstOrDefaultAsync(c => c.Id == 1);
            if (existing != null)
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
            else
            {
                company.Id = 1;
                _dbContext.Companies.Add(company);
            }
            await _dbContext.SaveChangesAsync();
        }

        public async Task<string> GetAppSettingAsync(string key, string defaultValue = "")
        {
            await _dbContext.Database.EnsureCreatedAsync();
            var setting = await _dbContext.AppSettings.FirstOrDefaultAsync(s => s.Key == key);
            return setting != null ? setting.Value : defaultValue;
        }

        public async Task SaveAppSettingAsync(string key, string value)
        {
            await _dbContext.Database.EnsureCreatedAsync();
            var setting = await _dbContext.AppSettings.FirstOrDefaultAsync(s => s.Key == key);
            if (setting != null)
            {
                setting.Value = value;
                _dbContext.AppSettings.Update(setting);
            }
            else
            {
                _dbContext.AppSettings.Add(new AppSetting { Key = key, Value = value });
            }
            await _dbContext.SaveChangesAsync();
        }

        public async Task SaveAllSettingsAsync(Company company, Dictionary<string, string> appSettings)
        {
            await SaveCompanyAsync(company);
            foreach (var kvp in appSettings)
            {
                await SaveAppSettingAsync(kvp.Key, kvp.Value);
            }
        }
    }
}
