using System.Windows.Controls;

namespace OmniWeigh.Desktop.Views.Pages
{
    public partial class HistoriqueView : UserControl
    {
        public HistoriqueView()
        {
            InitializeComponent();
            if (!System.ComponentModel.DesignerProperties.GetIsInDesignMode(this))
            {
                var vm = Microsoft.Extensions.DependencyInjection.ServiceProviderServiceExtensions.GetRequiredService<OmniWeigh.Desktop.ViewModels.HistoriqueViewModel>(App.Current.Services);
                this.DataContext = vm;
                Loaded += async (s, e) => await vm.InitializeAsync();
            }
        }
    }
}
