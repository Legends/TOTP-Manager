using System.Windows;
using TOTP.ViewModels;

namespace TOTP.UserControls;

public partial class Dialog : Syncfusion.Windows.Shared.ChromelessWindow
{
    public bool Result { get; private set; }

    public Dialog(DialogViewModel vm)
    {
        InitializeComponent();
        DataContext = vm;
    }

    private void Ok_Click(object sender, RoutedEventArgs e)
    {
        Result = true;
        Close();
    }

    private void Cancel_Click(object sender, RoutedEventArgs e)
    {
        Result = false;
        Close();
    }
}