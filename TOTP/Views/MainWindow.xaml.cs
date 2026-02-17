using Syncfusion.SfSkinManager;
using Syncfusion.Windows.Shared;
using System;
using TOTP.Resources;
using TOTP.Services.Interfaces;

namespace TOTP.Views;

public partial class MainWindow : ChromelessWindow
{
    public MainWindow(IMainViewModel vm)
    {
        InitializeComponent();

        //DataContext = vm ?? throw new ArgumentNullException(nameof(vm));
        //Title = UI.ui_Window_Title_TOTP_Manager;

        SkinManagerHelper.SetScrollBarMode(this, ScrollBarMode.Default);
    }
}
