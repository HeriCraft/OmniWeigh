using System.Windows;
using System.Windows.Controls;
using CommunityToolkit.Mvvm.Messaging;
using OmniWeigh.Desktop.Views.Pages;

namespace OmniWeigh.Desktop.Views
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window, CommunityToolkit.Mvvm.Messaging.IRecipient<OmniWeigh.Desktop.Messages.NavigateToPriseDePoidsMessage>
    {
        public MainWindow()
        {
            InitializeComponent();
            // DataContext is set by App DI bootstrap

            // Par défaut afficher la page Accueil (vide). Le contenu principal a été déplacé dans PriseDePoidsView.
            this.ContentRegion.Content = new AccueilView();

            WeakReferenceMessenger.Default.RegisterAll(this);
        }

        public void Receive(OmniWeigh.Desktop.Messages.NavigateToPriseDePoidsMessage message)
        {
            Application.Current.Dispatcher.Invoke(() =>
            {
                StartWeighingSessionWorkflow();
            });
        }

        private void BtnAccueil_Click(object sender, RoutedEventArgs e)
        {
            this.ContentRegion.Content = new AccueilView();
        }

        private void BtnPrisePoids_Click(object sender, RoutedEventArgs e)
        {
            StartWeighingSessionWorkflow();
        }

        private void StartWeighingSessionWorkflow()
        {
            var dialog = new Views.Dialogs.PreSessionWarningDialog();
            dialog.Owner = this;

            if (dialog.ShowDialog() == true && dialog.SessionStarted)
            {
                this.ContentRegion.Content = new PriseDePoidsView();
                
                // Update Sidebar visually
                var vm = this.DataContext as OmniWeigh.Desktop.ViewModels.WeighingViewModel;
                if (vm != null)
                {
                    vm.SelectedMenu = "PriseDePoids";
                }
            }
            else
            {
                // Revert visual selection if canceled (since standard command logic might have selected it)
                var vm = this.DataContext as OmniWeigh.Desktop.ViewModels.WeighingViewModel;
                if (vm != null)
                {
                    vm.SelectedMenu = "Accueil"; // Back to home
                }
                this.ContentRegion.Content = new AccueilView();
            }
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
        private void BtnVehicules_Click(object sender, RoutedEventArgs e)
        {
            this.ContentRegion.Content = new VehiculesView();
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
