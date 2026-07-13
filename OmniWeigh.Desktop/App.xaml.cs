using System;
using System.Windows;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using OmniWeigh.Core.Services;
using OmniWeigh.Core.Drivers;
using OmniWeigh.Desktop.ViewModels;

namespace OmniWeigh.Desktop
{
    public partial class App : Application
    {
        private IServiceProvider? _serviceProvider;

        protected override void OnStartup(StartupEventArgs e)
        {
            base.OnStartup(e);

            var services = new ServiceCollection();
            ConfigureServices(services);

            _serviceProvider = services.BuildServiceProvider();

            // Resolve ViewModel and initialize before showing the main window
            var mainWindow = new Views.MainWindow();
            var vm = _serviceProvider.GetRequiredService<WeighingViewModel>();
            mainWindow.DataContext = vm;

            // Initialize asynchronously but wait to ensure UI populated
            _ = vm.InitializeAsync();

            mainWindow.Show();
        }

        private void ConfigureServices(IServiceCollection services)
        {
            services.AddLogging(builder => builder.AddDebug());

            // Core services
            services.AddSingleton<IClientService, ClientService>();
            services.AddSingleton<IProductService, ProductService>();
            services.AddSingleton<IVehicleService, VehicleService>();
            services.AddSingleton<IBalanceDriver, MockBalanceDriver>();

            // ViewModels
            services.AddSingleton<WeighingViewModel>();
        }
    }
}

