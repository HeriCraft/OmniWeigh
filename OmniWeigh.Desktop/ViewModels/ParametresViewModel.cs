using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO.Ports;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Microsoft.Win32;
using OmniWeigh.Core.Models;
using OmniWeigh.Core.Services;
using OmniWeigh.Desktop.Views.Dialogs;

namespace OmniWeigh.Desktop.ViewModels
{
    public partial class ParametresViewModel : ObservableObject
    {
        private readonly ISettingsService _settingsService;

        [ObservableProperty]
        private string _companyName = string.Empty;

        [ObservableProperty]
        private string _companySlogan = string.Empty;

        [ObservableProperty]
        private string _companyAddress1 = string.Empty;

        [ObservableProperty]
        private string _companyAddress2 = string.Empty;

        [ObservableProperty]
        private string _companyPhone = string.Empty;

        [ObservableProperty]
        private string _companyEmail = string.Empty;

        [ObservableProperty]
        private string _companyLogoPath = string.Empty;

        public string AppVersion => System.Reflection.Assembly.GetExecutingAssembly().GetName().Version?.ToString() ?? "1.0.0.0";

        // Application Settings
        public ObservableCollection<string> AvailableScaleDrivers { get; } = new ObservableCollection<string>
        {
            "MockBalanceDriver",
            "SerialPortDriver"
        };

        [ObservableProperty]
        private string _selectedScaleDriver = "MockBalanceDriver";

        public ObservableCollection<string> AvailableComPorts { get; } = new ObservableCollection<string>();

        [ObservableProperty]
        private string _selectedComPort = "COM1";

        public ObservableCollection<int> AvailableBaudRates { get; } = new ObservableCollection<int>
        {
            1200, 2400, 4800, 9600, 19200, 38400, 57600, 115200
        };

        [ObservableProperty]
        private int _selectedBaudRate = 9600;

        public ObservableCollection<int> AvailableDataBits { get; } = new ObservableCollection<int> { 7, 8 };

        [ObservableProperty]
        private int _selectedDataBits = 8;

        public ObservableCollection<string> AvailableParities { get; } = new ObservableCollection<string>
        {
            "None", "Odd", "Even", "Mark", "Space"
        };

        [ObservableProperty]
        private string _selectedParity = "None";

        public ParametresViewModel(ISettingsService settingsService)
        {
            _settingsService = settingsService;
            RefreshComPorts();
        }

        private void RefreshComPorts()
        {
            AvailableComPorts.Clear();
            var ports = SerialPort.GetPortNames();
            if (ports.Length == 0)
            {
                AvailableComPorts.Add("COM1");
            }
            else
            {
                foreach (var port in ports.Distinct().OrderBy(p => p))
                {
                    AvailableComPorts.Add(port);
                }
            }
        }

        public async Task InitializeAsync()
        {
            var company = await _settingsService.GetCompanyAsync();
            CompanyName = company.Name;
            CompanySlogan = company.Slogan;
            CompanyAddress1 = company.Address1;
            CompanyAddress2 = company.Address2;
            CompanyPhone = company.Phone;
            CompanyEmail = company.Email;
            CompanyLogoPath = company.LogoPath;

            var driver = await _settingsService.GetAppSettingAsync("ScaleDriver", "MockBalanceDriver");
            if (AvailableScaleDrivers.Contains(driver)) SelectedScaleDriver = driver;

            var comPort = await _settingsService.GetAppSettingAsync("ComPort", "COM1");
            if (!AvailableComPorts.Contains(comPort)) AvailableComPorts.Add(comPort);
            SelectedComPort = comPort;

            if (int.TryParse(await _settingsService.GetAppSettingAsync("BaudRate", "9600"), out int baudRate))
                SelectedBaudRate = baudRate;

            if (int.TryParse(await _settingsService.GetAppSettingAsync("DataBits", "8"), out int dataBits))
                SelectedDataBits = dataBits;

            var parity = await _settingsService.GetAppSettingAsync("Parity", "None");
            if (AvailableParities.Contains(parity)) SelectedParity = parity;
        }

        [RelayCommand]
        private void PickLogo()
        {
            OpenFileDialog openFileDialog = new OpenFileDialog
            {
                Filter = "Fichiers image (*.png;*.jpg;*.jpeg)|*.png;*.jpg;*.jpeg|Tous les fichiers (*.*)|*.*",
                Title = "Sélectionner un logo pour l'entreprise"
            };

            if (openFileDialog.ShowDialog() == true)
            {
                CompanyLogoPath = openFileDialog.FileName;
            }
        }

        [RelayCommand]
        private async Task SaveAsync()
        {
            try
            {
                var company = new Company
                {
                    Name = CompanyName,
                    Slogan = CompanySlogan,
                    Address1 = CompanyAddress1,
                    Address2 = CompanyAddress2,
                    Phone = CompanyPhone,
                    Email = CompanyEmail,
                    LogoPath = CompanyLogoPath
                };

                var appSettings = new Dictionary<string, string>
                {
                    { "ScaleDriver", SelectedScaleDriver },
                    { "ComPort", SelectedComPort },
                    { "BaudRate", SelectedBaudRate.ToString() },
                    { "DataBits", SelectedDataBits.ToString() },
                    { "Parity", SelectedParity }
                };

                await _settingsService.SaveAllSettingsAsync(company, appSettings);
                
                // Re-bind logo path in case it was copied to local AppData
                var updatedCompany = await _settingsService.GetCompanyAsync();
                CompanyLogoPath = updatedCompany.LogoPath;

                var successDialog = new InformationDialog("Paramètres sauvegardés avec succès.", "Succès");
                successDialog.ShowDialog();
            }
            catch (Exception ex)
            {
                var errorDialog = new InformationDialog($"Erreur lors de la sauvegarde : {ex.Message}", "Erreur");
                errorDialog.ShowDialog();
            }
        }
    }
}
