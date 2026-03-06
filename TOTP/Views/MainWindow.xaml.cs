using OpenCvSharp.ML;
using Syncfusion.SfSkinManager;
using Syncfusion.Windows.Shared;
using System;
using System.Windows;
using TOTP.Infrastructure.Adapters;
using TOTP.Resources;
using TOTP.ViewModels.Interfaces;
using TOTP.Views.Interfaces;

namespace TOTP.Views;

public partial class MainWindow : ChromelessWindow, IMainWindow
{
    private readonly IMainViewModel _vm;
    public MainWindow(IMainViewModel vm)
    {
        _vm = vm;
        InitializeComponent();
        SetupWindowPositionAtStartup();

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
        
        // Setup RefreshFilter callable from View
        _vm.GridFilterRefresher = new GridFilterRefresher(AccountsSectionView.AccountsGridControl);
 
    }
 
}
