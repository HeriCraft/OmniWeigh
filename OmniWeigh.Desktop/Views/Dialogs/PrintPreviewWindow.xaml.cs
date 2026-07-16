using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;

namespace OmniWeigh.Desktop.Views.Dialogs
{
    public partial class PrintPreviewWindow : Window
    {
        private FlowDocument _document;

        public PrintPreviewWindow(FlowDocument document)
        {
            InitializeComponent();
            _document = document;
            DocViewer.Document = _document;
        }

        private void BtnPrint_Click(object sender, RoutedEventArgs e)
        {
            var printDialog = new PrintDialog();
            if (printDialog.ShowDialog() == true)
            {
                // To avoid pagination issues, we re-clone or just print directly
                // but IDocumentPaginatorSource is implemented by FlowDocument
                printDialog.PrintDocument(((IDocumentPaginatorSource)_document).DocumentPaginator, "Bon de Livraison");
                this.Close();
            }
        }

        private void BtnClose_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }
    }
}
