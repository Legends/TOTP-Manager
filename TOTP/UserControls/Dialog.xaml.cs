using System;
using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Imaging;

using TOTP.ViewModels;

namespace TOTP.UserControls;

public partial class Dialog : Syncfusion.Windows.Shared.ChromelessWindow
{
    public bool Result { get; private set; }

    public Dialog(DialogViewModel vm)
    {
        InitializeComponent();
        DataContext = vm;


        // Set TitleBarBackground using switch expression
        TitleBarBackground = vm.Caption switch
        {
            "Info" => Brushes.LightSkyBlue,
            "Warning" => Brushes.Yellow,
            "Error" => Brushes.Red,
            _ => Brushes.Gray // fallback
        };

        // Set Icon (ImageSource) based on caption
        var iconPath = vm.Caption switch
        {
            "Info" => "pack://application:,,,/TOTP.Manager;component/Assets/Icons/Info.png",
            "Warning" => "pack://application:,,,/TOTP.Manager;component/Assets/Icons/Warning.png",
            "Error" => "pack://application:,,,/TOTP.Manager;component/Assets/Icons/Wrong.png",
            _ => null
        };

        if (iconPath is not null)
        {
            Icon = new BitmapImage(new Uri(iconPath));
        }

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