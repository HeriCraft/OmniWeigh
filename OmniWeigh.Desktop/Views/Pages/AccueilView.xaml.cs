using System.Windows.Controls;

namespace OmniWeigh.Desktop.Views.Pages
{
    public partial class AccueilView : UserControl
    {
        public AccueilView()
        {
            InitializeComponent();
            if (!System.ComponentModel.DesignerProperties.GetIsInDesignMode(this))
            {
                var vm = Microsoft.Extensions.DependencyInjection.ServiceProviderServiceExtensions.GetRequiredService<OmniWeigh.Desktop.ViewModels.AccueilViewModel>(App.Current.Services);
                this.DataContext = vm;
                Loaded += async (s, e) => await vm.InitializeAsync();
            }
        }
    }
}
