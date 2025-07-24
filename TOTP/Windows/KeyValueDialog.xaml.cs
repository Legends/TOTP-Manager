using System.Windows;
using Syncfusion.Windows.Shared;
using TOTP.ViewModels;

//ControlNamespace###

namespace TOTP.Windows;

/// <summary>
///     Interaction logic for KeyValueDialog.xaml
/// </summary>
public partial class KeyValueDialog : ChromelessWindow
{
    public KeyValueDialog()
    {
        InitializeComponent();
        DataContext = ViewModel;

        //ControlMethodCall###
    }

    public KeyValueDialogViewModel ViewModel { get; } = new();

    private void Ok_Click(object sender, RoutedEventArgs e)
    {
        DialogResult = true;
    }
}