using System.IO;
using System.Windows;
using Microsoft.Win32;
using OmniWeigh.Desktop.ViewModels;

namespace OmniWeigh.Desktop.Views
{
    public partial class NewProductWindow : Window
    {
        public NewProductWindow()
        {
            InitializeComponent();
            this.Loaded += NewProductWindow_Loaded;
        }

        private void NewProductWindow_Loaded(object sender, RoutedEventArgs e)
        {
            if (DataContext is NewProductViewModel vm)
            {
                vm.RequestClose += Vm_RequestClose;
            }
        }

        private void Vm_RequestClose(object? sender, bool result)
        {
            if (DataContext is NewProductViewModel vm)
                vm.RequestClose -= Vm_RequestClose;

            this.DialogResult = result;
            this.Close();
        }

        private void Browse_Click(object sender, RoutedEventArgs e)
        {
            var dlg = new OpenFileDialog();
            dlg.Filter = "Image files|*.png;*.jpg;*.jpeg;*.bmp;*.gif|All files|*.*";
            if (dlg.ShowDialog() == true)
            {
                if (DataContext is NewProductViewModel vm)
                {
                    vm.ImageSourcePath = dlg.FileName;
                }
            }
        }
    }
}
