using System.Windows.Controls;

namespace OmniWeigh.Desktop.Views.Pages
{
    public partial class RapportsView : UserControl
    {
        public RapportsView()
        {
            InitializeComponent();
            if (!System.ComponentModel.DesignerProperties.GetIsInDesignMode(this))
            {
                var vm = Microsoft.Extensions.DependencyInjection.ServiceProviderServiceExtensions.GetRequiredService<OmniWeigh.Desktop.ViewModels.RapportsViewModel>(App.Current.Services);
                this.DataContext = vm;
                Loaded += async (s, e) => await vm.InitializeAsync();
            }
        }
    }
}
