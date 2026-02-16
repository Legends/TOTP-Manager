using Microsoft.Extensions.Logging;
using Syncfusion.SfSkinManager;
using Syncfusion.Windows.Shared;
using System;
using System.ComponentModel;
using System.Windows;
using TOTP.Resources;
using TOTP.Services.Interfaces;
using TOTP.ViewModels;

namespace TOTP.Views;

/// <summary>
///     Interaction logic for MainWindow.xaml
/// </summary>
public partial class MainWindow : ChromelessWindow
{
    private readonly IMainViewModel _vm;
    private readonly IInputActivityMonitor _activityMonitor;
    private bool _isActivityMonitorAttached;
    private ILogger<MainWindow> _logger;

    public MainWindow(IMainViewModel vm, IInputActivityMonitor activityMonitor, ILogger<MainWindow> logger)
    {

        _logger = logger;
        _activityMonitor = activityMonitor;
        InitializeComponent();

        SetupWindowPositionAtStartup();

        // build action: Resource
        //this.Icon = new BitmapImage(new Uri("pack://application:,,,/Assets/Icons/github.ico"));
        //this.Icon = new BitmapImage(new Uri("pack://application:,,,/totp.ico"));

        DataContext = vm;
        Title = UI.ui_Window_Title_TOTP_Manager;

        _vm = vm ?? throw new ArgumentNullException(nameof(vm));
        SkinManagerHelper.SetScrollBarMode(this, ScrollBarMode.Default);

        Loaded += OnLoadedAsync;
        SecretsGrid.ItemsSourceChanged += SecretsGrid_ItemsSourceChanged;
        Closed += OnClosed;
        _vm.PropertyChanged += OnViewModelPropertyChanged;
    }

    private void SetupWindowPositionAtStartup()
    {
        double screenWidth = SystemParameters.PrimaryScreenWidth;
        double screenHeight = SystemParameters.PrimaryScreenHeight;

        double windowWidth = this.Width;
        double windowHeight = this.Height;

        // Center horizontally, 1/5 from top
        this.Left = (screenWidth - windowWidth) / 2;
        this.Top = screenHeight / 5;
    }

    private async void OnLoadedAsync(object sender, RoutedEventArgs e)
    {
        Loaded -= OnLoadedAsync; // wichtig, um Mehrfachaufrufe zu vermeiden

        try
        {
            await _vm.InitializeAsync();
            UpdateActivityMonitorState();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, nameof(OnLoadedAsync));
            Application.Current.Shutdown(-1);
        }
    }

    private void SecretsGrid_ItemsSourceChanged(object? sender, Syncfusion.UI.Xaml.Grid.GridItemsSourceChangedEventArgs e)
    {

        if (DataContext is IMainViewModel vm && SecretsGrid.View != null && SecretsGrid.View.Filter == null)
        {
            // Attach filtering handler
            SecretsGrid.View.Filter = vm.DoFilterGrid;
            //SecretsGrid.View.RefreshFilter(true); 


            // Setup RefreshFilter callable from View
            vm.RequestGridFilterRefresh = () =>
            {
                if (!Dispatcher.CheckAccess())
                    Dispatcher.BeginInvoke(new Action(() => SecretsGrid.View?.RefreshFilter()));
                else
                    SecretsGrid.View?.RefreshFilter();
            };

            // Filters the grid datasource based on vm.DoFilterGrid
            SecretsGrid.View.RefreshFilter();
        }
    }

    private void OnClosed(object? sender, EventArgs e)
    {
        Closed -= OnClosed;
        _vm.PropertyChanged -= OnViewModelPropertyChanged;
        DetachActivityMonitor();
    }

    private void OnViewModelPropertyChanged(object? sender, PropertyChangedEventArgs e)
    {
        if (e.PropertyName == nameof(MainViewModel.IsUnlocked))
        {
            UpdateActivityMonitorState();
        }
    }

    private void UpdateActivityMonitorState()
    {
        if (_vm is not MainViewModel vm)
            return;

        if (vm.IsUnlocked)
        {
            AttachActivityMonitor();
        }
        else
        {
            DetachActivityMonitor();
        }
    }

    private void AttachActivityMonitor()
    {
        if (_isActivityMonitorAttached)
            return;

        _activityMonitor.Attach(this);
        _isActivityMonitorAttached = true;
    }

    private void DetachActivityMonitor()
    {
        if (!_isActivityMonitorAttached)
            return;

        _activityMonitor.Detach();
        _isActivityMonitorAttached = false;
    }

    protected override void OnStateChanged(EventArgs e)
    {
        base.OnStateChanged(e);

        if (WindowState == WindowState.Minimized)
        {
            if (DataContext is MainViewModel vm)
                vm.Lock();
        }
    }


}
