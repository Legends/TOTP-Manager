using Microsoft.Extensions.Configuration;
using OtpNet;
using System.Reflection;
using System.Text;
using System.Text.Unicode;
using System.Threading.Tasks;
using System.Windows;

namespace Github2FA
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        IConfiguration _configuration;
        public MainWindow()
        {
            InitializeComponent();
            // You have to create a user secrets.json file first:
            // right-click project: Manage user secrets
            // in secrets.json enter: { "sharedGithubSecret": "yourGithubKeyGoesHere" }
            // the github secret key can originally be obtained from here:
            // https://github.com/settings/security?type=app#two-factor-summary
            // save.
            _configuration = new ConfigurationBuilder()
                        .AddUserSecrets(Assembly.GetExecutingAssembly(), true)
                        .Build();
        }

        private async void Button_Click(object sender, RoutedEventArgs e)
        {
            // the github secret key can originally be obtained from here:
            // https://github.com/settings/security?type=app#two-factor-summary
            // Click => Two-Factor methods: => Authenticator app => ... => Edit
            // Either you scan the QR code now with your phone or you click on the link below to get the secret code/key:
            // "You can use the >> setup key << to manually configure your authenticator app."
            // Now right-click your project and click "Manage user secrets" and add the key there like:
            // {
            //  "sharedGithubSecret": "NXYZPPWERLMK4"
            // }

            string sharedSecret = _configuration["sharedGithubSecret"]; //"NUBTWTGJ6UU7SMK4"; 

            var totp = new Totp(Base32Encoding.ToBytes(sharedSecret));

            // Generate a TOTP code:
            string totpCode = totp.ComputeTotp(); // This generates a TOTP code for the current time.

            lblCode.Content = totpCode;
            Clipboard.SetText(totpCode);
            lblClipboard.Visibility = Visibility.Visible;

            await Task.Delay(2500).ContinueWith((ct) =>
            {
                Dispatcher.Invoke(() =>
                {
                    lblClipboard.Visibility = Visibility.Hidden;
                });

            });
        }
    }
}
