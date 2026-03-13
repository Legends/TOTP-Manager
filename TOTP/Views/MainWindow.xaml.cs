using Syncfusion.SfSkinManager;
using Syncfusion.Windows.Shared;
using Serilog;
using System;
using System.ComponentModel;
using System.Diagnostics;
using System.Windows;
using System.Windows.Threading;
using TOTP.Infrastructure.Adapters;
using TOTP.UserControls;
using TOTP.ViewModels.Interfaces;
using TOTP.Views.Components;
using TOTP.Views.Interfaces;

namespace TOTP.Views;

public partial class MainWindow : ChromelessWindow, IMainWindow
{
    private const string SfDataGridDictionaryPath = "Styles/SfDataGrid.xaml";
    private static readonly bool EnableLifecycleLogging = false;

    private readonly Stopwatch _lifecycleStopwatch = Stopwatch.StartNew();
    private readonly IMainViewModel _vm;
    private SettingsWindow? _settingsWindow;
    private bool _settingsWindowPreloadQueued;
    private bool _settingsWindowPreloaded;
    private bool _dataGridResourcesLoaded;
    private bool _accountsSectionLoaded;
    private bool _editFlyoutViewLoaded;
    private bool _allowSettingsWindowClose;
    private bool _handlingSettingsWindowClosing;

    public MainWindow(IMainViewModel vm)
    {
        _vm = vm;
        LogLifecycle("ctor.begin");
        InitializeComponent();
        LogLifecycle("ctor.after_initialize_component");

        _vm.PropertyChanged += MainViewModel_PropertyChanged;
        HookFlyoutLazyLoading();
        LogLifecycle("ctor.after_hook_flyout_lazy_loading");
        EnsureAccountsSectionLoaded();
        LogLifecycle("ctor.after_ensure_accounts_section_loaded");

        SetupWindowPositionAtStartup();
        LogLifecycle("ctor.end");
    }

    private void HookFlyoutLazyLoading()
    {
        DependencyPropertyDescriptor
            .FromProperty(FlyoutHost.IsOpenProperty, typeof(FlyoutHost))
            .AddValueChanged(EditFlyoutHost, (_, __) =>
            {
                if (EditFlyoutHost.IsOpen)
                {
                    EnsureEditFlyoutLoaded();
                }
            });
    }

    private void EnsureAccountsSectionLoaded()
    {
        if (_accountsSectionLoaded)
        {
            LogLifecycle("accounts_section.load.skipped_already_loaded");
            return;
        }

        LogLifecycle("accounts_section.load.begin");
        EnsureDataGridResourcesLoaded();
        LogLifecycle("accounts_section.load.after_datagrid_resources");

        var accountsSection = new AccountsSection
        {
            DataContext = _vm
        };
        LogLifecycle("accounts_section.load.after_construct_section");

        AccountsSectionHost.Content = accountsSection;
        LogLifecycle("accounts_section.load.after_assign_content");
        _vm.GridFilterRefresher = new GridFilterRefresher(accountsSection.AccountsGridControl);
        LogLifecycle("accounts_section.load.after_grid_filter_refresher");
        _vm.RequestGridFilterRefresh?.Invoke();
        _accountsSectionLoaded = true;
        LogLifecycle("accounts_section.load.end");
    }

    private void EnsureDataGridResourcesLoaded()
    {
        if (_dataGridResourcesLoaded)
        {
            LogLifecycle("datagrid_resources.load.skipped_already_loaded");
            return;
        }

        LogLifecycle("datagrid_resources.load.begin");
        if (Application.Current?.Resources?.MergedDictionaries != null)
        {
            var dictionaries = Application.Current.Resources.MergedDictionaries;
            var alreadyLoaded = false;
            foreach (var dictionary in dictionaries)
            {
                var source = dictionary.Source?.OriginalString;
                if (string.Equals(source, SfDataGridDictionaryPath, StringComparison.OrdinalIgnoreCase) ||
                    string.Equals(source, "/" + SfDataGridDictionaryPath, StringComparison.OrdinalIgnoreCase))
                {
                    alreadyLoaded = true;
                    break;
                }
            }

            if (!alreadyLoaded)
            {
                LogLifecycle("datagrid_resources.load.add_dictionary.begin");
                dictionaries.Add(new ResourceDictionary
                {
                    Source = new Uri(SfDataGridDictionaryPath, UriKind.Relative)
                });
                LogLifecycle("datagrid_resources.load.add_dictionary.end");
            }
        }

        _dataGridResourcesLoaded = true;
        LogLifecycle("datagrid_resources.load.end");
    }

    private void EnsureEditFlyoutLoaded()
    {
        if (_editFlyoutViewLoaded)
        {
            LogLifecycle("edit_flyout.init.skipped_already_loaded");
            return;
        }

        LogLifecycle("edit_flyout.init.begin");
        EditFlyoutHost.FlyoutContent = new EditAddAccountFlyoutView();
        _editFlyoutViewLoaded = true;
        LogLifecycle("edit_flyout.init.end");
    }

    private void SetupWindowPositionAtStartup()
    {
        double screenWidth = SystemParameters.PrimaryScreenWidth;
        double screenHeight = SystemParameters.PrimaryScreenHeight;

        double windowWidth = Width;
        double windowHeight = Height;

        Left = (screenWidth - windowWidth) / 2;
        Top = screenHeight / 5;
        LogLifecycle("window_position.set");
    }

    private void MainViewModel_PropertyChanged(object? sender, PropertyChangedEventArgs e)
    {
        if (e.PropertyName != nameof(IMainViewModel.IsSettingsViewOpen) &&
            e.PropertyName != nameof(IMainViewModel.SettingsVm))
        {
            return;
        }

        Dispatcher.Invoke(SyncSettingsWindow);

        if (e.PropertyName == nameof(IMainViewModel.SettingsVm))
        {
            QueueSettingsWindowPreload();
        }
    }

    private void SyncSettingsWindow()
    {
        if (_vm.IsSettingsViewOpen && _vm.SettingsVm != null)
        {
            EnsureSettingsWindowCreated();
            if (_settingsWindow == null)
            {
                return;
            }

            if (!_settingsWindow.IsVisible)
            {
                _settingsWindow.Show();
            }

            _settingsWindow.Activate();
            _settingsWindow.Focus();
            return;
        }

        if (_settingsWindow != null && _settingsWindow.IsVisible)
        {
            _settingsWindow.Hide();
        }
    }

    private void QueueSettingsWindowPreload()
    {
        if (_settingsWindowPreloaded || _settingsWindowPreloadQueued || _vm.SettingsVm == null)
        {
            return;
        }

        _settingsWindowPreloadQueued = true;
        Dispatcher.BeginInvoke(DispatcherPriority.ContextIdle, new Action(() =>
        {
            _settingsWindowPreloadQueued = false;
            if (_settingsWindowPreloaded || _vm.SettingsVm == null)
            {
                return;
            }

            EnsureSettingsWindowCreated();
            _settingsWindow?.ApplyTemplate();
            _settingsWindow?.UpdateLayout();
            _settingsWindowPreloaded = true;
            LogLifecycle("settings_window.preloaded");
        }));
    }

    private void EnsureSettingsWindowCreated()
    {
        if (_vm.SettingsVm == null)
        {
            return;
        }

        if (_settingsWindow == null)
        {
            _settingsWindow = new SettingsWindow
            {
                Owner = this,
                DataContext = _vm.SettingsVm
            };
            _settingsWindow.Closing += SettingsWindow_Closing;
            return;
        }

        if (!ReferenceEquals(_settingsWindow.DataContext, _vm.SettingsVm))
        {
            _settingsWindow.DataContext = _vm.SettingsVm;
        }
    }

    private void SettingsWindow_Closing(object? sender, CancelEventArgs e)
    {
        if (_allowSettingsWindowClose)
        {
            return;
        }

        e.Cancel = true;
        if (_handlingSettingsWindowClosing)
        {
            return;
        }

        try
        {
            _handlingSettingsWindowClosing = true;
            _settingsWindow?.Hide();
            if (_vm.IsSettingsViewOpen)
            {
                _vm.IsSettingsViewOpen = false;
            }

            BringMainWindowToFront();
        }
        finally
        {
            _handlingSettingsWindowClosing = false;
        }
    }

    protected override void OnClosed(EventArgs e)
    {
        if (_settingsWindow != null)
        {
            _allowSettingsWindowClose = true;
            _settingsWindow.Closing -= SettingsWindow_Closing;
            _settingsWindow.Close();
            _settingsWindow = null;
        }

        base.OnClosed(e);
    }

    private void BringMainWindowToFront()
    {
        if (!IsVisible)
        {
            Show();
        }

        if (WindowState == WindowState.Minimized)
        {
            WindowState = WindowState.Normal;
        }

        Activate();
        Topmost = true;
        Topmost = false;
        Focus();
    }

    private void LogLifecycle(string step)
    {
        if (!EnableLifecycleLogging)
        {
            return;
        }

        var elapsedMs = _lifecycleStopwatch.ElapsedMilliseconds;
        Debug.WriteLine($"MainWindow.{step} ms={elapsedMs}");
        Log.Information("mainwindow.lifecycle step={Step} elapsed_ms={ElapsedMs}", step, elapsedMs);
    }
}
