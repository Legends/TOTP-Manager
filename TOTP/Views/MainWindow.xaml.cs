using OpenCvSharp.ML;
using Syncfusion.SfSkinManager;
using Syncfusion.Windows.Shared;
using System;
using TOTP.Resources;
using TOTP.Services.Interfaces;

namespace TOTP.Views;

public partial class MainWindow : ChromelessWindow, IMainWindow
{

    public MainWindow(IMainViewModel vm)
    {
        InitializeComponent();

        //DataContext = vm ?? throw new ArgumentNullException(nameof(vm));
        //Title = UI.ui_Window_Title_TOTP_Manager;

        //SkinManagerHelper.SetScrollBarMode(this, ScrollBarMode.Default);

        SecretsGrid.ItemsSourceChanged += SecretsGrid_ItemsSourceChanged;

        //vm.PropertyChanged += OnViewModelPropertyChanged;
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
}
