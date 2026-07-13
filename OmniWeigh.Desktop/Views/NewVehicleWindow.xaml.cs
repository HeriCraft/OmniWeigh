using Microsoft.Win32;
using System.Windows;
using System.Windows.Controls;
using OmniWeigh.Desktop.ViewModels;

namespace OmniWeigh.Desktop.Views
{
    public partial class NewVehicleWindow : Window
    {
        public NewVehicleWindow()
        {
            InitializeComponent();
            this.Loaded += NewVehicleWindow_Loaded;
        }

        private void NewVehicleWindow_Loaded(object sender, RoutedEventArgs e)
        {
            if (DataContext is NewVehicleViewModel vm)
            {
                vm.RequestClose += Vm_RequestClose;
            }
        }

        private void Vm_RequestClose(object? sender, bool result)
        {
            if (DataContext is NewVehicleViewModel vm)
                vm.RequestClose -= Vm_RequestClose;

            this.DialogResult = result;
            this.Close();
        }
        private void Browse_Click(object sender, RoutedEventArgs e)
        {
            if (DataContext is NewVehicleViewModel vm)
            {
                var dlg = new OpenFileDialog();
                dlg.Filter = "Image files (*.png;*.jpg;*.jpeg;*.webp)|*.png;*.jpg;*.jpeg;*.webp|All files (*.*)|*.*";
                var result = dlg.ShowDialog(this);
                if (result == true)
                {
                    vm.ImageSourcePath = dlg.FileName;
                }
            }
        }
    }
}
