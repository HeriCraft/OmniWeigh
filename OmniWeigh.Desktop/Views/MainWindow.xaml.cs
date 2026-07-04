using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace OmniWeigh.Desktop.Views
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
        }
    }

    // Petite classe temporaire pour afficher les fausses lignes dans la maquette
    public class MockRow
    {
        public string Id { get; set; } = string.Empty;
        public string DateTime { get; set; } = string.Empty;
        public string Client { get; set; } = string.Empty;
        public string Product { get; set; } = string.Empty;
        public string Gross { get; set; } = string.Empty;
        public string Tare { get; set; } = string.Empty;
        public string Net { get; set; } = string.Empty;
    }
}