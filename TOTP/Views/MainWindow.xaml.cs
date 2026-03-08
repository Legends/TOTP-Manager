using Syncfusion.SfSkinManager;
using Syncfusion.Windows.Shared;
using System;
using System.Diagnostics;
using System.Windows;
using TOTP.Infrastructure.Adapters;
using TOTP.ViewModels.Interfaces;
using TOTP.Views.Interfaces;

namespace TOTP.Views;

public partial class MainWindow : ChromelessWindow, IMainWindow
{
    private readonly Stopwatch _lifecycleStopwatch = Stopwatch.StartNew();
    private readonly IMainViewModel _vm;

    public MainWindow(IMainViewModel vm)
    {
        _vm = vm;
        Debug.WriteLine($"MainWindow.ctor.begin ms={_lifecycleStopwatch.ElapsedMilliseconds}");
        InitializeComponent();
        _vm.GridFilterRefresher = new GridFilterRefresher(AccountsSectionControl.AccountsGridControl);
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

}
