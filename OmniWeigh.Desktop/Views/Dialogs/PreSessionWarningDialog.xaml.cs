using System.Windows;

namespace OmniWeigh.Desktop.Views.Dialogs
{
    public partial class PreSessionWarningDialog : Window
    {
        public bool SessionStarted { get; private set; }

        public PreSessionWarningDialog()
        {
            InitializeComponent();
        }

        private void BtnStart_Click(object sender, RoutedEventArgs e)
        {
            SessionStarted = true;
            DialogResult = true;
            Close();
        }

        private void BtnCancel_Click(object sender, RoutedEventArgs e)
        {
            SessionStarted = false;
            DialogResult = false;
            Close();
        }
    }
}
