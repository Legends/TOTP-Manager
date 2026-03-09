using Syncfusion.SfSkinManager;
using Syncfusion.Windows.Shared;
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

    private readonly Stopwatch _lifecycleStopwatch = Stopwatch.StartNew();
    private readonly IMainViewModel _vm;
    private bool _dataGridResourcesLoaded;
    private bool _accountsSectionLoaded;
    private bool _editFlyoutViewLoaded;

    public MainWindow(IMainViewModel vm)
    {
        _vm = vm;
        Debug.WriteLine($"MainWindow.ctor.begin ms={_lifecycleStopwatch.ElapsedMilliseconds}");
        InitializeComponent();

        HookFlyoutLazyLoading();
        InitializeSettingsFlyout();
        EnsureAccountsSectionLoaded();

        Debug.WriteLine($"MainWindow.ctor.after.InitializeComponent ms={_lifecycleStopwatch.ElapsedMilliseconds}");
        SetupWindowPositionAtStartup();
        Debug.WriteLine($"MainWindow.ctor.end ms={_lifecycleStopwatch.ElapsedMilliseconds}");
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

        DependencyPropertyDescriptor
            .FromProperty(FlyoutHost.IsOpenProperty, typeof(FlyoutHost))
            .AddValueChanged(SettingsFlyoutHost, (_, __) =>
            {
                if (SettingsFlyoutHost.IsOpen)
                {
                    InitializeSettingsFlyout();
                }
            });
    }

    private void InitializeSettingsFlyout()
    {
        if (SettingsFlyoutHost.FlyoutContent != null)
        {
            return;
        }

        SettingsFlyoutHost.FlyoutContent = new SettingsFlyoutView();
    }

    private void EnsureAccountsSectionLoaded()
    {
        if (_accountsSectionLoaded)
        {
            return;
        }

        EnsureDataGridResourcesLoaded();

        var accountsSection = new AccountsSection
        {
            DataContext = _vm
        };

        AccountsSectionHost.Content = accountsSection;
        _vm.GridFilterRefresher = new GridFilterRefresher(accountsSection.AccountsGridControl);
        _vm.RequestGridFilterRefresh?.Invoke();
        _accountsSectionLoaded = true;
    }

    private void EnsureDataGridResourcesLoaded()
    {
        if (_dataGridResourcesLoaded)
        {
            return;
        }

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
                dictionaries.Add(new ResourceDictionary
                {
                    Source = new Uri(SfDataGridDictionaryPath, UriKind.Relative)
                });
            }
        }

        _dataGridResourcesLoaded = true;
    }

    private void EnsureEditFlyoutLoaded()
    {
        if (_editFlyoutViewLoaded)
        {
            return;
        }

        EditFlyoutHost.FlyoutContent = new EditAddAccountFlyoutView();
        _editFlyoutViewLoaded = true;
    }

    private void SetupWindowPositionAtStartup()
    {
        double screenWidth = SystemParameters.PrimaryScreenWidth;
        double screenHeight = SystemParameters.PrimaryScreenHeight;

        double windowWidth = this.Width;
        double windowHeight = this.Height;

        this.Left = (screenWidth - windowWidth) / 2;
        this.Top = screenHeight / 5;
    }
}
