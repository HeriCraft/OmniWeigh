using System.Windows;

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