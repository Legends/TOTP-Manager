using Syncfusion.Windows.Shared;
using System;
using System.Windows.Media.Imaging;
using TOTP.Helper;
using TOTP.Interfaces;

namespace TOTP.UserControls;

public partial class UserMessageDialog : ChromelessWindow
{
    public bool Result { get; private set; }

    public UserMessageDialog(IUserMessageDialogViewModel vm)
    {
        InitializeComponent();
        DataContext = vm;

        // Bind ViewModel-provided values
        TitleBarBackground = vm.TitleBarBackground;
        if (!string.IsNullOrWhiteSpace(vm.IconPath) && Common.PackUriExists(vm.IconPath))
        {
            Icon = new BitmapImage(new Uri(vm.IconPath));
        }
        // Subscribe to close request
        vm.RequestClose += OnRequestClose;
        Closed += (_, _) => vm.RequestClose -= OnRequestClose; // ! Important ! otherwise you get loops and unhandled exceptions
    }

    private void OnRequestClose(object? sender, bool result)
    {
        DialogResult = Result = result;
        Close();
    }

    //private void Ok_Click(object sender, RoutedEventArgs e)
    //{
    //    Result = true;
    //    DialogResult = true; // ← important
    //    Close();
    //}

    //private void Cancel_Click(object sender, RoutedEventArgs e)
    //{
    //    Result = false;
    //    DialogResult = false; // ← important
    //    Close();
    //}

}