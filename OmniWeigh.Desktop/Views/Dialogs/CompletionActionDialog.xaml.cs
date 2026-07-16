using System.Windows;

namespace OmniWeigh.Desktop.Views.Dialogs
{
    public partial class CompletionActionDialog : Window
    {
        public enum ActionChoice
        {
            Cancel,
            Print,
            SavePdf
        }

        public ActionChoice SelectedAction { get; private set; } = ActionChoice.Cancel;
        public bool IsThermalFormat => FormatComboBox.SelectedIndex == 1;

        public CompletionActionDialog()
        {
            InitializeComponent();
        }

        private void BtnPrint_Click(object sender, RoutedEventArgs e)
        {
            SelectedAction = ActionChoice.Print;
            DialogResult = true;
            Close();
        }

        private void BtnPdf_Click(object sender, RoutedEventArgs e)
        {
            SelectedAction = ActionChoice.SavePdf;
            DialogResult = true;
            Close();
        }

        private void BtnCancel_Click(object sender, RoutedEventArgs e)
        {
            SelectedAction = ActionChoice.Cancel;
            DialogResult = false;
            Close();
        }
    }
}
