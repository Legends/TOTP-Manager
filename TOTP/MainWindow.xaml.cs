using Microsoft.Extensions.Logging;
using Syncfusion.SfSkinManager;
using Syncfusion.UI.Xaml.Grid;
using Syncfusion.Windows.Shared;
using System;
using System.Windows;
using TOTP.Interfaces;
using TOTP.Resources;
using TOTP.ViewModels;

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

        double screenWidth = SystemParameters.PrimaryScreenWidth;
        double screenHeight = SystemParameters.PrimaryScreenHeight;

        double windowWidth = this.Width;
        double windowHeight = this.Height;

        // Center horizontally, 1/4 from top
        this.Left = (screenWidth - windowWidth) / 2;
        this.Top = screenHeight / 4;


        // build action: Resource
        //this.Icon = new BitmapImage(new Uri("pack://application:,,,/Assets/Icons/github.ico"));
        //this.Icon = new BitmapImage(new Uri("pack://application:,,,/totp.ico"));

        DataContext = vm;
        Title = UI.ui_Window_Title_TOTP_Manager;

        _vm = vm ?? throw new ArgumentNullException(nameof(vm));
        SkinManagerHelper.SetScrollBarMode(this, ScrollBarMode.Default);

        Loaded += OnLoadedAsync;
        DataContextChanged += (_, __) => WireGridFiltering();

        SecretsGrid.ItemsSourceChanged += SecretsGrid_ItemsSourceChanged;

    }

    private async void OnLoadedAsync(object sender, RoutedEventArgs e)
    {
        Loaded -= OnLoadedAsync; // wichtig, um Mehrfachaufrufe zu vermeiden

        try
        {
            await _vm.InitializeAsync();
            WireGridFiltering();

        }
        catch (Exception ex)
        {
            _logger.LogError(ex, nameof(OnLoadedAsync));
            Application.Current.Shutdown(-1);
        }
    }
    private void SecretsGrid_ItemsSourceChanged(object? sender, GridItemsSourceChangedEventArgs e)
    {
        // Re-attach the filter when the ItemsSource instance changes
        if (DataContext is IMainViewModel vm)
        {
            SecretsGrid.View.Filter = vm.DoFilterGrid;
            SecretsGrid.View.RefreshFilter();
        }
    }

    private void WireGridFiltering()
    {
        if (DataContext is not IMainViewModel vm) return;

        // 1) Point Syncfusion filter to your VM's predicate
        SecretsGrid.View.Filter = vm.DoFilterGrid;

        // 2) Let the VM ask the grid to refresh (debounced via ExecuteSearch/RefreshView)
        vm.RequestGridFilterRefresh = () =>
        {
            if (!Dispatcher.CheckAccess())
                Dispatcher.BeginInvoke(new Action(() => SecretsGrid.View.RefreshFilter()));
            else
                SecretsGrid.View.RefreshFilter();
        };

        // Initial apply
        SecretsGrid.View.RefreshFilter();
    }

    private async void DataGrid_SelectionChanged(object sender, GridSelectionChangedEventArgs e)
    {
        try
        {
            var gRow = (e.AddedItems[0] as GridRowInfo);
            _vm.SelectedSecret = gRow.RowData as SecretItemViewModel;
            await _vm.OnSelectionChangedAsync();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex.Message, ex);
        }
    }

}