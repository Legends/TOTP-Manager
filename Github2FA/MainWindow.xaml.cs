using Microsoft.Extensions.Configuration;
using OtpNet;
using Syncfusion.UI.Xaml.Grid;
using Syncfusion.UI.Xaml.Grid.Helpers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Animation;
 

namespace Github2FA
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private readonly IConfiguration _configuration;
        private List<KeyValuePair<string, string>> _secrets;

        public MainWindow(IConfiguration configuration)
        {
            InitializeComponent();
            _configuration = configuration;
            ReadSecretCodes();
            SecretsGrid.ItemsSource = _secrets;
        }

        private void ReadSecretCodes()
        {
            _secrets = _configuration?.AsEnumerable().Skip(1)
                ?.Where(pair => pair.Value != null) // Filter out null values
                .Select(pair => new KeyValuePair<string, string>(pair.Key, pair.Value!)) // Use null-forgiving operator
                .ToList() ?? new List<KeyValuePair<string, string>>(); // Default to an empty list if null
        }

        private async void SecretsGrid_SelectionChanged(object sender, GridSelectionChangedEventArgs e)
        {
            if (SecretsGrid.SelectedItem is KeyValuePair<string, string> selectedSecret)
            {
                string platform = selectedSecret.Key;
                string platformSecret = selectedSecret.Value;

                var totp = new Totp(Base32Encoding.ToBytes(platformSecret));
                string totpCode = totp.ComputeTotp();

                lblCode.Content = $"{platform}: {totpCode}";
                Clipboard.SetText(totpCode);
                lblCopiedToClipboard.Visibility = Visibility.Visible;

                //AnimateSelectedRow();

                
                await Task.Delay(2500);
                lblCopiedToClipboard.Visibility = Visibility.Hidden;
            }
        }
        private void AnimateSelectedRow()
        {
            if (SecretsGrid.SelectedItem == null)
                return;

            int rowIndex = SecretsGrid.ResolveToRowIndex(SecretsGrid.SelectedItem);

            var row = SecretsGrid.GetRowGenerator()?.Items
                .FirstOrDefault(r => r.RowIndex == rowIndex) as DataRowBase;

            if (row == null)
                return;

            foreach (var cell in FindVisualChildren<ContentControl>(row.Element))
            {
                var brush = new SolidColorBrush(Colors.Transparent);
                cell.Background = brush;

                var animation = new ColorAnimation
                {
                    From = Colors.LightYellow,
                    To = Colors.White,
                    Duration = TimeSpan.FromMilliseconds(400),
                    AutoReverse = false
                };

                brush.BeginAnimation(SolidColorBrush.ColorProperty, animation);
            }
        }

        private static IEnumerable<T> FindVisualChildren<T>(DependencyObject parent) where T : DependencyObject
        {
            if (parent == null) yield break;

            for (int i = 0; i < VisualTreeHelper.GetChildrenCount(parent); i++)
            {
                var child = VisualTreeHelper.GetChild(parent, i);

                if (child is T correctlyTyped)
                    yield return correctlyTyped;

                foreach (var descendent in FindVisualChildren<T>(child))
                    yield return descendent;
            }
        }



    }
}
