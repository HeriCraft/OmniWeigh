using System.Windows;
using System.Windows.Controls;
using OmniWeigh.Desktop.Views.Pages;

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
            this.DataContext = new OmniWeigh.Desktop.ViewModels.WeighingViewModel();

            // Par défaut afficher la page Accueil (vide). Le contenu principal a été déplacé dans PriseDePoidsView.
            this.ContentRegion.Content = new AccueilView();
        }

        private void BtnAccueil_Click(object sender, RoutedEventArgs e)
        {
            this.ContentRegion.Content = new AccueilView();
        }

        private void BtnPrisePoids_Click(object sender, RoutedEventArgs e)
        {
            this.ContentRegion.Content = new PriseDePoidsView();
        }

        private void BtnHistorique_Click(object sender, RoutedEventArgs e)
        {
            this.ContentRegion.Content = new HistoriqueView();
        }

        private void BtnRapports_Click(object sender, RoutedEventArgs e)
        {
            this.ContentRegion.Content = new RapportsView();
        }

        private void BtnClients_Click(object sender, RoutedEventArgs e)
        {
            this.ContentRegion.Content = new ClientsView();
        }

        private void BtnProduits_Click(object sender, RoutedEventArgs e)
        {
            this.ContentRegion.Content = new ProduitsView();
        }

        private void BtnParametres_Click(object sender, RoutedEventArgs e)
        {
            this.ContentRegion.Content = new ParametresView();
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
