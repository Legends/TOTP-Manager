using Github2FA.ViewModels;
using Syncfusion.Windows.Shared;
using System.Windows;

//ControlNamespace###

namespace Github2FA.Windows
{
    /// <summary>
    /// Interaction logic for KeyValueDialog.xaml
    /// </summary>
    public partial class KeyValueDialog : ChromelessWindow
    {
        public KeyValueDialogViewModel ViewModel { get; } = new KeyValueDialogViewModel();

        public KeyValueDialog()
        {
            InitializeComponent();
            DataContext = ViewModel;

            //ControlMethodCall###
        }

        private void Ok_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = true;
        }


    }
}
