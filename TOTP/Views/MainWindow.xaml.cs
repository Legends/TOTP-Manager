using Syncfusion.SfSkinManager;
using Syncfusion.Windows.Shared;
using Serilog;
using System;
using System.ComponentModel;
using System.Diagnostics;
using System.Threading;
using System.Windows;
using System.Windows.Threading;
using TOTP.Presentation.Adapters;
using TOTP.Presentation.Services.Interfaces;
using TOTP.Core.Services.Interfaces;
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
    private readonly IApplicationLifetime _applicationLifetime;
    private readonly ISettingsWindowCoordinator _settingsWindowCoordinator;
    private bool _dataGridResourcesLoaded;
    private bool _accountsSectionLoaded;
    private bool _editFlyoutViewLoaded;
    private int _processExitStarted;

    public MainWindow(
        IMainViewModel vm,
        IApplicationLifetime applicationLifetime,
        ISettingsWindowCoordinator settingsWindowCoordinator)
    {
        _vm = vm;
        _applicationLifetime = applicationLifetime;
        _settingsWindowCoordinator = settingsWindowCoordinator;
        LogLifecycle("ctor.begin");
        InitializeComponent();
        LogLifecycle("ctor.after_initialize_component");

        _settingsWindowCoordinator.Attach(this, _vm, BringToFront);
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

    protected override void OnClosing(CancelEventArgs e)
    {
        Log.Information("mainwindow.closing begin cancel={Cancel} is_visible={IsVisible}", e.Cancel, IsVisible);
        base.OnClosing(e);

        if (e.Cancel)
        {
            Log.Information("mainwindow.closing canceled");
            return;
        }

        Log.Information("mainwindow.closing process_exit_requested");
        ExitProcessFromMainWindowClose();
    }

    protected override void OnClosed(EventArgs e)
    {
        Log.Information("mainwindow.closed begin");
        var detachWindowCommand = _vm.SessionController.DetachWindowCommand;
        if (detachWindowCommand.CanExecute(null))
        {
            detachWindowCommand.Execute(null);
        }

        _settingsWindowCoordinator.Dispose();

        base.OnClosed(e);
        Log.Information("mainwindow.closed end");

        Log.Information("mainwindow.closed requesting_application_shutdown");
        _applicationLifetime.Shutdown();
    }

    private void ExitProcessFromMainWindowClose()
    {
        if (Interlocked.Exchange(ref _processExitStarted, 1) == 1)
        {
            return;
        }

        try
        {
            var detachWindowCommand = _vm.SessionController.DetachWindowCommand;
            if (detachWindowCommand.CanExecute(null))
            {
                detachWindowCommand.Execute(null);
            }

            _settingsWindowCoordinator.Dispose();

            if (_vm is IDisposable disposableViewModel)
            {
                disposableViewModel.Dispose();
            }
        }
        catch (Exception ex)
        {
            Log.Warning(ex, "mainwindow.close.cleanup_failed");
        }
        finally
        {
            Log.Information("mainwindow.closing environment_exit");
            _applicationLifetime.ExitProcess(0);
        }
    }

    public void BringToFront()
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
