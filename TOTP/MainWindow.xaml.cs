using Microsoft.Extensions.Logging;
using Syncfusion.SfSkinManager;
using Syncfusion.UI.Xaml.Grid;
using Syncfusion.Windows.Shared;
using System;
using System.Windows;
using TOTP.Interfaces;
using TOTP.Resources;

namespace TOTP;

/// <summary>
///     Interaction logic for MainWindow.xaml
/// </summary>
public partial class MainWindow : ChromelessWindow
{
    private readonly IMainViewModel _vm;
    private ILogger<MainWindow> _logger;

    public MainWindow(IMainViewModel vm, ILogger<MainWindow> logger)
    {
        _logger = logger;
        InitializeComponent();
        // build action: Resource
        //this.Icon = new BitmapImage(new Uri("pack://application:,,,/Assets/Icons/github.ico"));
        //this.Icon = new BitmapImage(new Uri("pack://application:,,,/totp.ico"));

        DataContext = vm;
        Title = UI.ui_Window_Title_TOTP_Manager;

        _vm = vm ?? throw new ArgumentNullException(nameof(vm));
        SkinManagerHelper.SetScrollBarMode(this, ScrollBarMode.Compact);

        Loaded += OnLoadedAsync;
    }

    private async void OnLoadedAsync(object sender, RoutedEventArgs e)
    {
        Loaded -= OnLoadedAsync; // wichtig, um Mehrfachaufrufe zu vermeiden

        try
        {
            await _vm.InitializeAsync();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, nameof(OnLoadedAsync));
            Application.Current.Shutdown(-1);
        }
    }

    private async void DataGrid_SelectionChanged(object sender, GridSelectionChangedEventArgs e)
    {
        try
        {
            await _vm.OnSelectionChangedAsync();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex.Message, ex);
        }
    }

}