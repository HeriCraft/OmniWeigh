using System.Windows;
using OmniWeigh.Desktop.ViewModels;

namespace OmniWeigh.Desktop.Views
{
    public partial class NewClientWindow : Window
    {
        public NewClientWindow()
        {
            InitializeComponent();
            // Hook up RequestClose if DataContext is NewClientViewModel
            this.Loaded += NewClientWindow_Loaded;
        }

        private void NewClientWindow_Loaded(object sender, RoutedEventArgs e)
        {
            if (DataContext is NewClientViewModel vm)
            {
                vm.RequestClose += Vm_RequestClose;
            }
        }

        private void Vm_RequestClose(object? sender, bool result)
        {
            // Unsubscribe to avoid memory leak
            if (DataContext is NewClientViewModel vm)
                vm.RequestClose -= Vm_RequestClose;

            this.DialogResult = result;
            this.Close();
        }
    }
}
