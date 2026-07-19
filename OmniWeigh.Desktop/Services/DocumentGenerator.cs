using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Documents;
using System.Windows.Media;
using System.Windows.Controls;

namespace OmniWeigh.Desktop.Services
{
    public class DocumentGenerator
    {
        public static FlowDocument GenerateDocument(
            string documentType,
            string documentNumber,
            string operatorName,
            DateTime sessionDate,
            OmniWeigh.Desktop.ViewModels.ClientItem? client,
            OmniWeigh.Desktop.ViewModels.VehicleItem? vehicle,
            IEnumerable<OmniWeigh.Core.Models.WeighingHistory> entries,
            OmniWeigh.Core.Models.Company? company,
            bool isThermal = false)
        {
            var primaryBlue = (SolidColorBrush)new BrushConverter().ConvertFromString("#1C3A6C")!;
            var borderBlue = (SolidColorBrush)new BrushConverter().ConvertFromString("#D9E2EC")!;
            var lightBlueBg = (SolidColorBrush)new BrushConverter().ConvertFromString("#F0F4F8")!;
            var textBrush = Brushes.Black;

            var doc = new FlowDocument
            {
                ColumnWidth = 999999,
                FontFamily = new FontFamily("Segoe UI"),
                PagePadding = isThermal ? new Thickness(10) : new Thickness(30),
                PageWidth = isThermal ? 300 : 793,
                Background = Brushes.White,
                Foreground = textBrush
            };

            if (isThermal)
            {
                // Thermal format keeps a simplified structure
                doc.PagePadding = new Thickness(5);
                doc.Blocks.Add(new Paragraph(new Run(company?.Name ?? "ENTREPRISE")) { FontSize = 14, FontWeight = FontWeights.Bold, TextAlignment = TextAlignment.Center, Margin = new Thickness(0, 0, 0, 2) });
                doc.Blocks.Add(new Paragraph(new Run(documentType.ToUpper() + $" N° {documentNumber}")) { FontSize = 10, FontWeight = FontWeights.Bold, TextAlignment = TextAlignment.Center, Margin = new Thickness(0,0,0,10) });
                
                var cltTxt = new TextBlock { TextWrapping = TextWrapping.Wrap, Margin = new Thickness(0,0,0,2) };
                cltTxt.Inlines.Add(new Run("Clt: ") { FontWeight = FontWeights.Bold, FontSize = 10 });
                cltTxt.Inlines.Add(new Run(client?.Name ?? "-") { FontSize = 10 });
                doc.Blocks.Add(new BlockUIContainer(cltTxt));

                var vehTxt = new TextBlock { TextWrapping = TextWrapping.Wrap, Margin = new Thickness(0,0,0,10) };
                vehTxt.Inlines.Add(new Run("Véh: ") { FontWeight = FontWeights.Bold, FontSize = 10 });
                vehTxt.Inlines.Add(new Run(vehicle?.Registration ?? "-") { FontSize = 10 });
                doc.Blocks.Add(new BlockUIContainer(vehTxt));
                
                var itemsTable = new Table { Margin = new Thickness(0, 5, 0, 5), BorderThickness = new Thickness(0) };
                itemsTable.Columns.Add(new TableColumn { Width = new GridLength(45, GridUnitType.Star) });
                itemsTable.Columns.Add(new TableColumn { Width = new GridLength(20, GridUnitType.Star) });
                itemsTable.Columns.Add(new TableColumn { Width = new GridLength(35, GridUnitType.Star) });
                var rg = new TableRowGroup();
                itemsTable.RowGroups.Add(rg);
                
                var hr = new TableRow { Background = Brushes.White };
                hr.Cells.Add(new TableCell(new Paragraph(new Run("Désig.")) { FontWeight = FontWeights.Bold, FontSize = 9, Padding = new Thickness(0,0,0,2) }));
                hr.Cells.Add(new TableCell(new Paragraph(new Run("Qté")) { FontWeight = FontWeights.Bold, FontSize = 9, TextAlignment = TextAlignment.Center, Padding = new Thickness(0,0,0,2) }));
                hr.Cells.Add(new TableCell(new Paragraph(new Run("Brut")) { FontWeight = FontWeights.Bold, FontSize = 9, TextAlignment = TextAlignment.Right, Padding = new Thickness(0,0,0,2) }));
                rg.Rows.Add(hr);

                double total = 0;
                foreach (var e in entries)
                {
                    total += e.GrossWeight;
                    var r = new TableRow();
                    r.Cells.Add(new TableCell(new Paragraph(new Run(e.Product?.Name ?? $"ID {e.ProductId}")) { FontSize = 9, Padding = new Thickness(0,2,0,2) }));
                    r.Cells.Add(new TableCell(new Paragraph(new Run($"{e.Quantity} {e.Unit}")) { FontSize = 9, TextAlignment = TextAlignment.Center, Padding = new Thickness(0,2,0,2) }));
                    r.Cells.Add(new TableCell(new Paragraph(new Run(e.GrossWeight.ToString("F2"))) { FontSize = 9, TextAlignment = TextAlignment.Right, Padding = new Thickness(0,2,0,2) }));
                    rg.Rows.Add(r);
                }
                
                var fr = new TableRow { Background = Brushes.White };
                fr.Cells.Add(new TableCell(new Paragraph(new Run("TOTAL")) { FontWeight = FontWeights.Bold, FontSize = 10, Padding = new Thickness(0,5,0,0) }) { ColumnSpan = 2 });
                fr.Cells.Add(new TableCell(new Paragraph(new Run(total.ToString("F2") + " kg")) { FontWeight = FontWeights.Bold, FontSize = 10, TextAlignment = TextAlignment.Right, Padding = new Thickness(0,5,0,0) }));
                rg.Rows.Add(fr);
                
                doc.Blocks.Add(itemsTable);
                doc.Blocks.Add(new Paragraph(new Run($"Opér: {operatorName}")) { FontSize = 9, TextAlignment = TextAlignment.Center, Margin = new Thickness(0,10,0,0) });
                doc.Blocks.Add(new Paragraph(new Run($"{sessionDate:dd/MM/yyyy HH:mm}")) { FontSize = 9, TextAlignment = TextAlignment.Center, Margin = new Thickness(0,0,0,10) });
                return doc;
            }

            // A4 Premium Format Layout using BlockUIContainer for fixed layout sections
            
            // 1. Header Grid
            var headerGrid = new Grid { Margin = new Thickness(0, 0, 0, 15) };
            headerGrid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(2.5, GridUnitType.Star) });
            headerGrid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(3, GridUnitType.Star) });
            headerGrid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(2.5, GridUnitType.Star) });

            // Company Info
            var coSp = new StackPanel { VerticalAlignment = VerticalAlignment.Top };
            if (company != null && !string.IsNullOrEmpty(company.LogoPath) && System.IO.File.Exists(company.LogoPath))
            {
                try 
                {
                    var bmp = new System.Windows.Media.Imaging.BitmapImage();
                    bmp.BeginInit();
                    bmp.UriSource = new Uri(company.LogoPath, UriKind.Absolute);
                    bmp.CacheOption = System.Windows.Media.Imaging.BitmapCacheOption.OnLoad;
                    bmp.EndInit();
                    coSp.Children.Add(new Image { Source = bmp, MaxHeight = 50, HorizontalAlignment = HorizontalAlignment.Left, Margin = new Thickness(0,0,0,5) });
                }
                catch
                {
                    coSp.Children.Add(new TextBlock { Text = company.Name.ToUpper(), FontSize = 22, FontWeight = FontWeights.Bold, Foreground = primaryBlue, Margin = new Thickness(0,0,0,5) });
                }
            }
            else
            {
                coSp.Children.Add(new TextBlock { Text = (company?.Name ?? "SIMEX-ci").ToUpper(), FontSize = 22, FontWeight = FontWeights.Bold, Foreground = primaryBlue, Margin = new Thickness(0,0,0,5) });
            }
            coSp.Children.Add(new TextBlock { Text = company?.Slogan ?? "Pesage • Métrologie • Maintenance", FontSize = 9 });
            coSp.Children.Add(new TextBlock { Text = company?.Address1 ?? "", FontSize = 9 });
            coSp.Children.Add(new TextBlock { Text = company?.Address2 ?? "", FontSize = 9 });
            if (!string.IsNullOrEmpty(company?.Phone)) coSp.Children.Add(new TextBlock { Text = $"Tél : {company.Phone}", FontSize = 9 });
            if (!string.IsNullOrEmpty(company?.Email)) coSp.Children.Add(new TextBlock { Text = $"Email : {company.Email}", FontSize = 9 });
            Grid.SetColumn(coSp, 0);
            headerGrid.Children.Add(coSp);

            // Title
            var titleTb = new TextBlock { Text = documentType.ToUpper(), FontSize = 20, FontWeight = FontWeights.Bold, Foreground = primaryBlue, TextAlignment = TextAlignment.Center, VerticalAlignment = VerticalAlignment.Top, Margin = new Thickness(0,5,0,0) };
            Grid.SetColumn(titleTb, 1);
            headerGrid.Children.Add(titleTb);

            // Doc Info
            var docInfoSp = new StackPanel { VerticalAlignment = VerticalAlignment.Top, HorizontalAlignment = HorizontalAlignment.Right, Width = 180 };
            var numBorder = new Border { BorderBrush = borderBlue, BorderThickness = new Thickness(1), Padding = new Thickness(5), Margin = new Thickness(0,0,0,10) };
            numBorder.Child = new TextBlock { Text = $"N° {documentNumber}", FontSize = 12, FontWeight = FontWeights.Bold, Foreground = Brushes.Red, TextAlignment = TextAlignment.Center };
            docInfoSp.Children.Add(numBorder);
            docInfoSp.Children.Add(new TextBlock { Text = $"Date      : {sessionDate:dd/MM/yyyy}", FontSize = 10 });
            docInfoSp.Children.Add(new TextBlock { Text = $"Heure    : {sessionDate:HH:mm:ss}", FontSize = 10 });
            docInfoSp.Children.Add(new TextBlock { Text = $"Page      : 1 / 1", FontSize = 10 });
            Grid.SetColumn(docInfoSp, 2);
            headerGrid.Children.Add(docInfoSp);

            doc.Blocks.Add(new BlockUIContainer(headerGrid));

            // Helper to create Box UI
            Border CreateBoxUI(string title, Action<Grid> buildRows)
            {
                var border = new Border { BorderThickness = new Thickness(1), BorderBrush = borderBlue, Margin = new Thickness(0,0,0,10), Background = Brushes.White };
                var grid = new Grid();
                grid.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto });
                
                var headerBg = new Border { Background = lightBlueBg, Padding = new Thickness(5) };
                headerBg.Child = new TextBlock { Text = title, FontSize = 11, FontWeight = FontWeights.Bold, Foreground = primaryBlue };
                Grid.SetRow(headerBg, 0);
                grid.Children.Add(headerBg);
                
                var contentGrid = new Grid { Margin = new Thickness(5) };
                contentGrid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(120) });
                contentGrid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) });
                buildRows(contentGrid);
                
                Grid.SetRow(contentGrid, 1);
                grid.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto });
                grid.Children.Add(contentGrid);
                
                border.Child = grid;
                return border;
            }

            void AddRowUI(Grid g, int rowIndex, string label, string value)
            {
                g.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto });
                var t1 = new TextBlock { Text = label, FontSize = 10, Margin = new Thickness(0,2,0,2) };
                var t2 = new TextBlock { Text = $": {value}", FontSize = 10, Margin = new Thickness(0,2,0,2) };
                Grid.SetRow(t1, rowIndex); Grid.SetColumn(t1, 0);
                Grid.SetRow(t2, rowIndex); Grid.SetColumn(t2, 1);
                g.Children.Add(t1); g.Children.Add(t2);
            }

            // Client & Livraison (Side by Side)
            var infoBoxesGrid = new Grid { Margin = new Thickness(0,0,0,5) };
            infoBoxesGrid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) });
            infoBoxesGrid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(10) });
            infoBoxesGrid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) });
            
            var clientBoxUI = CreateBoxUI("1. CLIENT", g => {
                AddRowUI(g, 0, "Nom / Raison sociale", (client != null && !string.IsNullOrWhiteSpace(client.Name)) ? client.Name : "-");
                AddRowUI(g, 1, "Téléphone", (client != null && !string.IsNullOrWhiteSpace(client.Phone)) ? client.Phone : "-");
                AddRowUI(g, 2, "Réf. Client", (client != null && !string.IsNullOrWhiteSpace(client.Reference)) ? client.Reference : "-");
            });
            Grid.SetColumn(clientBoxUI, 0);
            infoBoxesGrid.Children.Add(clientBoxUI);

            var deliveryBoxUI = CreateBoxUI("2. INFORMATIONS LIVRAISON", g => {
                AddRowUI(g, 0, "Lieu de livraison", "-");
                AddRowUI(g, 1, "Transporteur", "-");
                AddRowUI(g, 2, "Véhicule", (vehicle != null && !string.IsNullOrWhiteSpace(vehicle.Registration)) ? vehicle.Registration : "-");
                AddRowUI(g, 3, "Chauffeur", operatorName);
            });
            Grid.SetColumn(deliveryBoxUI, 2);
            infoBoxesGrid.Children.Add(deliveryBoxUI);

            doc.Blocks.Add(new BlockUIContainer(infoBoxesGrid));

            // 3. Document Info
            var docBoxUI = CreateBoxUI("3. INFORMATIONS DOCUMENT", g => {
                g.ColumnDefinitions.Clear();
                g.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) });
                g.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) });
                g.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto });
                
                var t1 = new TextBlock { Text = $"Type de document    : {documentType}", FontSize = 10 };
                var t2 = new TextBlock { Text = $"Référence commande : -", FontSize = 10 };
                Grid.SetRow(t1, 0); Grid.SetColumn(t1, 0);
                Grid.SetRow(t2, 0); Grid.SetColumn(t2, 1);
                g.Children.Add(t1); g.Children.Add(t2);
            });
            doc.Blocks.Add(new BlockUIContainer(docBoxUI));

            // 4. Products Table (Using FlowDocument Table to allow page breaks if many items)
            var pTable = new Table { BorderThickness = new Thickness(1), BorderBrush = borderBlue, Margin = new Thickness(0,0,0,5), CellSpacing = 0 };
            pTable.Columns.Add(new TableColumn { Width = new GridLength(5, GridUnitType.Star) });    // N° (5%)
            pTable.Columns.Add(new TableColumn { Width = new GridLength(13, GridUnitType.Star) });   // Ref (13%)
            pTable.Columns.Add(new TableColumn { Width = new GridLength(35, GridUnitType.Star) });   // Desig (35%)
            pTable.Columns.Add(new TableColumn { Width = new GridLength(8, GridUnitType.Star) });    // Qte (8%)
            pTable.Columns.Add(new TableColumn { Width = new GridLength(7, GridUnitType.Star) });    // Unité (7%)
            pTable.Columns.Add(new TableColumn { Width = new GridLength(14, GridUnitType.Star) });   // Poids (14%)
            pTable.Columns.Add(new TableColumn { Width = new GridLength(18, GridUnitType.Star) });   // Obs (18%)

            var pGrp = new TableRowGroup();
            pTable.RowGroups.Add(pGrp);
            
            var pHeaderTop = new TableRow { Background = lightBlueBg };
            pHeaderTop.Cells.Add(new TableCell(new Paragraph(new Run("4. DÉTAIL DES PRODUITS LIVRÉS")) { FontSize = 11, FontWeight = FontWeights.Bold, Foreground = primaryBlue, Padding = new Thickness(3) }) { ColumnSpan = 7 });
            pGrp.Rows.Add(pHeaderTop);

            var pHeaderCol = new TableRow { Background = lightBlueBg };
            string[] headers = { "N°", "Référence", "Désignation", "Qté", "Unité", "Poids Brut (kg)", "Observations" };
            foreach (var h in headers)
            {
                pHeaderCol.Cells.Add(new TableCell(new Paragraph(new Run(h)) { FontSize = 10, FontWeight = FontWeights.SemiBold, Foreground = primaryBlue, TextAlignment = TextAlignment.Center, Padding = new Thickness(1,3,1,3) }) { BorderThickness = new Thickness(0,0,1,1), BorderBrush = borderBlue });
            }
            pGrp.Rows.Add(pHeaderCol);

            int idx = 1;
            double totalBrut = 0;
            foreach (var e in entries)
            {
                totalBrut += e.GrossWeight;
                var r = new TableRow();
                r.Cells.Add(new TableCell(new Paragraph(new Run(idx.ToString())) { FontSize = 10, TextAlignment = TextAlignment.Center, Padding = new Thickness(3) }) { BorderThickness = new Thickness(0,0,1,1), BorderBrush = borderBlue });
                r.Cells.Add(new TableCell(new Paragraph(new Run($"P-{(e.Product?.Id ?? e.ProductId):D5}")) { FontSize = 10, TextAlignment = TextAlignment.Center, Padding = new Thickness(3) }) { BorderThickness = new Thickness(0,0,1,1), BorderBrush = borderBlue });
                r.Cells.Add(new TableCell(new Paragraph(new Run(e.Product?.Name ?? $"ID {e.ProductId}")) { FontSize = 10, TextAlignment = TextAlignment.Center, Padding = new Thickness(3) }) { BorderThickness = new Thickness(0,0,1,1), BorderBrush = borderBlue });
                r.Cells.Add(new TableCell(new Paragraph(new Run(e.Quantity.ToString())) { FontSize = 10, TextAlignment = TextAlignment.Center, Padding = new Thickness(3) }) { BorderThickness = new Thickness(0,0,1,1), BorderBrush = borderBlue });
                r.Cells.Add(new TableCell(new Paragraph(new Run(e.Unit.ToString())) { FontSize = 10, TextAlignment = TextAlignment.Center, Padding = new Thickness(3) }) { BorderThickness = new Thickness(0,0,1,1), BorderBrush = borderBlue });
                r.Cells.Add(new TableCell(new Paragraph(new Run(e.GrossWeight.ToString("F2"))) { FontSize = 10, TextAlignment = TextAlignment.Center, Padding = new Thickness(3) }) { BorderThickness = new Thickness(0,0,1,1), BorderBrush = borderBlue });
                r.Cells.Add(new TableCell(new Paragraph(new Run(string.IsNullOrWhiteSpace(e.Observation) ? "-" : e.Observation)) { FontSize = 10, TextAlignment = TextAlignment.Center, Padding = new Thickness(3) }) { BorderThickness = new Thickness(0,0,0,1), BorderBrush = borderBlue });
                pGrp.Rows.Add(r);
                idx++;
            }
            doc.Blocks.Add(pTable);

            // Total section
            var totalGrid = new Grid { Margin = new Thickness(0,0,0,10) };
            totalGrid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) });
            var totalTb = new TextBlock { TextAlignment = TextAlignment.Right };
            totalTb.Inlines.Add(new Run("TOTAL POIDS BRUT :  ") { FontSize = 11, FontWeight = FontWeights.Bold, Foreground = primaryBlue });
            totalTb.Inlines.Add(new Run($"{totalBrut:F2} kg") { FontSize = 11, FontWeight = FontWeights.Bold });
            totalGrid.Children.Add(totalTb);
            doc.Blocks.Add(new BlockUIContainer(totalGrid));

            // 5. Observations Box & Signatures
            var footerGrid = new Grid();
            footerGrid.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto });
            footerGrid.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto });
            
            var obsBoxUI = CreateBoxUI("5. OBSERVATIONS", g => {
                var globalObs = string.Join("\n", entries.Where(x => !string.IsNullOrWhiteSpace(x.Observation)).Select(x => x.Observation));
                if (string.IsNullOrWhiteSpace(globalObs)) globalObs = "Marchandises reçues en bon état.";
                var tb = new TextBlock { Text = globalObs, FontSize = 10, Margin = new Thickness(0,5,0,20), TextWrapping = TextWrapping.Wrap };
                Grid.SetColumnSpan(tb, 2);
                g.Children.Add(tb);
            });
            Grid.SetRow(obsBoxUI, 0);
            footerGrid.Children.Add(obsBoxUI);

            var sigGrid = new Grid { Margin = new Thickness(0,10,0,0) };
            sigGrid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) });
            sigGrid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) });
            sigGrid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) });
            
            var sig1 = new TextBlock { Text = "Préparé par\n\n\n\n" + operatorName.ToUpper(), FontSize = 10, TextAlignment = TextAlignment.Center };
            var sig2 = new TextBlock { Text = "Vérifié par\n\n\n\n....................", FontSize = 10, TextAlignment = TextAlignment.Center };
            var sig3 = new TextBlock { Text = "Reçu par le client\n\n\n\n........................................", FontSize = 10, TextAlignment = TextAlignment.Center };
            
            Grid.SetColumn(sig1, 0); Grid.SetColumn(sig2, 1); Grid.SetColumn(sig3, 2);
            sigGrid.Children.Add(sig1); sigGrid.Children.Add(sig2); sigGrid.Children.Add(sig3);
            
            Grid.SetRow(sigGrid, 1);
            footerGrid.Children.Add(sigGrid);
            
            doc.Blocks.Add(new BlockUIContainer(footerGrid));

            return doc;
        }
    }
}
