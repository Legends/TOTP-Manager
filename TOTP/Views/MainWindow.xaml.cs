using Syncfusion.SfSkinManager;
using Syncfusion.Windows.Shared;
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

    private readonly Stopwatch _lifecycleStopwatch = Stopwatch.StartNew();
    private readonly IMainViewModel _vm;
    private bool _dataGridResourcesLoaded;
    private bool _accountsSectionLoaded;
    private bool _editFlyoutViewLoaded;
    private bool _settingsFlyoutViewLoaded;

    public MainWindow(IMainViewModel vm)
    {
        _vm = vm;
        Debug.WriteLine($"MainWindow.ctor.begin ms={_lifecycleStopwatch.ElapsedMilliseconds}");
        InitializeComponent();

        HookFlyoutLazyLoading();
        HookAccountsSectionLazyLoading();

        Debug.WriteLine($"MainWindow.ctor.after.InitializeComponent ms={_lifecycleStopwatch.ElapsedMilliseconds}");
        SetupWindowPositionAtStartup();
        Debug.WriteLine($"MainWindow.ctor.end ms={_lifecycleStopwatch.ElapsedMilliseconds}");

        //EventManager.RegisterClassHandler(typeof(UIElement),
        //    UIElement.GotFocusEvent,
        //    new RoutedEventHandler((s, e) =>
        //    {
        //        System.Diagnostics.Debug.WriteLine($"Focus moved to: {e.Source}");
        //    }), true);
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
                    EnsureSettingsFlyoutLoaded();
                }
            });
    }

    private void HookAccountsSectionLazyLoading()
    {
        _vm.PropertyChanged += (_, e) =>
        {
            if (string.Equals(e.PropertyName, "IsUnlocked", StringComparison.Ordinal))
            {
                Dispatcher.BeginInvoke(TryLoadAccountsSectionIfUnlocked, DispatcherPriority.Background);
            }
        };

        ContentRendered += (_, __) =>
        {
            Dispatcher.BeginInvoke(TryLoadAccountsSectionIfUnlocked, DispatcherPriority.Background);
        };
    }

    private void TryLoadAccountsSectionIfUnlocked()
    {
        if (!IsViewModelUnlocked())
        {
            return;
        }

        EnsureAccountsSectionLoaded();
    }

    private bool IsViewModelUnlocked()
    {
        var isUnlockedProperty = _vm.GetType().GetProperty("IsUnlocked");
        if (isUnlockedProperty?.PropertyType != typeof(bool))
        {
            return true;
        }

        return (bool)(isUnlockedProperty.GetValue(_vm) ?? false);
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

    private void EnsureSettingsFlyoutLoaded()
    {
        if (_settingsFlyoutViewLoaded)
        {
            return;
        }

        SettingsFlyoutHost.FlyoutContent = new SettingsFlyoutView();
        _settingsFlyoutViewLoaded = true;
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

}
