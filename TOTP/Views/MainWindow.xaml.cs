using Syncfusion.SfSkinManager;
using Syncfusion.Windows.Shared;
using Serilog;
using System;
using System.ComponentModel;
using System.Diagnostics;
using System.Windows;
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
    private bool _closingSettingsWindow;
    private bool _dataGridResourcesLoaded;
    private bool _accountsSectionLoaded;
    private bool _editFlyoutViewLoaded;

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
    }

    private void SyncSettingsWindow()
    {
        if (_vm.IsSettingsViewOpen && _vm.SettingsVm != null)
        {
            if (_settingsWindow == null)
            {
                _settingsWindow = new SettingsWindow
                {
                    Owner = this,
                    DataContext = _vm.SettingsVm
                };
                _settingsWindow.Closed += SettingsWindow_Closed;
            }
            else if (!ReferenceEquals(_settingsWindow.DataContext, _vm.SettingsVm))
            {
                _settingsWindow.DataContext = _vm.SettingsVm;
            }

            if (!_settingsWindow.IsVisible)
            {
                _settingsWindow.Show();
            }

            _settingsWindow.Activate();
            _settingsWindow.Focus();
            return;
        }

        if (_settingsWindow != null && _settingsWindow.IsVisible && !_closingSettingsWindow)
        {
            _closingSettingsWindow = true;
            _settingsWindow.Close();
            _closingSettingsWindow = false;
        }
    }

    private void SettingsWindow_Closed(object? sender, EventArgs e)
    {
        if (_settingsWindow != null)
        {
            _settingsWindow.Closed -= SettingsWindow_Closed;
            _settingsWindow = null;
        }

        if (_vm.IsSettingsViewOpen)
        {
            _vm.IsSettingsViewOpen = false;
        }
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
