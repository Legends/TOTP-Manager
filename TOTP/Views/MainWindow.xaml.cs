using Syncfusion.SfSkinManager;
using Syncfusion.Windows.Shared;
using System;
using System.Windows;
using TOTP.Resources;
using TOTP.Services.Interfaces;

namespace TOTP.Views;

public partial class MainWindow : ChromelessWindow
{
    private readonly IMainViewModel _vm;
    private bool _isInitialized;

    public MainWindow(IMainViewModel vm)
    {
        InitializeComponent();

        _vm = vm ?? throw new ArgumentNullException(nameof(vm));
        DataContext = _vm;
        Title = UI.ui_Window_Title_TOTP_Manager;

        SkinManagerHelper.SetScrollBarMode(this, ScrollBarMode.Default);

        Loaded += OnLoadedAsync;
    }

    private async void OnLoadedAsync(object sender, RoutedEventArgs e)
    {
        if (_isInitialized)
            return;

        _isInitialized = true;
        await _vm.InitializeAsync();
    }
}
