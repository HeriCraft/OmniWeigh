using System.Windows.Controls;
using Microsoft.Extensions.DependencyInjection;
using OmniWeigh.Desktop.ViewModels;

namespace OmniWeigh.Desktop.Views.Pages
{
    public partial class ParametresView : UserControl
    {
        public ParametresView()
        {
            InitializeComponent();
            var vm = App.Current.Services.GetRequiredService<ParametresViewModel>();
            this.DataContext = vm;
            
            this.Loaded += async (s, e) =>
            {
                await vm.InitializeAsync();
            };
        }
    }
}
