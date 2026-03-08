using Syncfusion.SfSkinManager;
using Syncfusion.Windows.Shared;
using System;
using System.Diagnostics;
using System.Windows;
using System.Windows.Threading;
using TOTP.Infrastructure.Adapters;
using TOTP.Resources;
using TOTP.ViewModels.Interfaces;
using TOTP.Views.Components;
using TOTP.Views.Interfaces;

namespace TOTP.Views;

public partial class MainWindow : ChromelessWindow, IMainWindow
{
    private readonly Stopwatch _lifecycleStopwatch = Stopwatch.StartNew();
    private readonly IMainViewModel _vm;
    private bool _accountsSectionLoaded;
    public MainWindow(IMainViewModel vm)
    {
        _vm = vm;
        Debug.WriteLine($"MainWindow.ctor.begin ms={_lifecycleStopwatch.ElapsedMilliseconds}");
        InitializeComponent();
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

    protected override void OnContentRendered(EventArgs e)
    {
        base.OnContentRendered(e);
        Debug.WriteLine($"MainWindow.OnContentRendered ms={_lifecycleStopwatch.ElapsedMilliseconds}");
        if (_accountsSectionLoaded)
        {
            return;
        }

        Dispatcher.BeginInvoke(LoadAccountsSectionDeferred, DispatcherPriority.Background);
    }

    private void LoadAccountsSectionDeferred()
    {
        if (_accountsSectionLoaded)
        {
            return;
        }

        var section = new AccountsSection();
        AccountsSectionHost.Content = section;
        AccountsSectionPlaceholder.Visibility = Visibility.Collapsed;

        _vm.GridFilterRefresher = new GridFilterRefresher(section.AccountsGridControl);
        _accountsSectionLoaded = true;
    }
}
