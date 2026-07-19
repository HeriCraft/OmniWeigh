using System.Windows;

namespace OmniWeigh.Desktop.Views.Dialogs
{
    public partial class InformationDialog : Window
    {
        public InformationDialog(string message, string title = "Information")
        {
            InitializeComponent();
            MessageText.Text = message;
            TitleText.Text = title.ToUpper();
            Owner = Application.Current.MainWindow;
        }

        private void BtnOk_Click(object sender, RoutedEventArgs e)
        {
            this.DialogResult = true;
            this.Close();
        }
    }
}
