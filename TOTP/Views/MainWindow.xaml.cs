using OpenCvSharp.ML;
using Syncfusion.SfSkinManager;
using Syncfusion.Windows.Shared;
using System;
using TOTP.Infrastructure.Adapters;
using TOTP.Resources;
using TOTP.Services.Interfaces;

namespace TOTP.Views;

public partial class MainWindow : ChromelessWindow, IMainWindow
{
    private readonly IMainViewModel _vm;
    public MainWindow(IMainViewModel vm)
    {
        _vm = vm;
        InitializeComponent();
    }

    protected override void OnContentRendered(EventArgs e)
    {
        base.OnContentRendered(e);
        
        // Setup RefreshFilter callable from View
        _vm.GridFilterRefresher = new GridFilterRefresher(AccountsGrid);
 
    }
 
}
