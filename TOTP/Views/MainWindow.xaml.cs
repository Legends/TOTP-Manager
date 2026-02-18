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

        

        //SecretsGrid.ItemsSourceChanged += SecretsGrid_ItemsSourceChanged;

        //vm.PropertyChanged += OnViewModelPropertyChanged;
    }

    protected override void OnContentRendered(EventArgs e)
    {
        base.OnContentRendered(e);
        AccountsGrid.ItemsSourceChanged += AccountsGrid_ItemsSourceChanged;
    }

    private void AccountsGrid_ItemsSourceChanged(object? sender, Syncfusion.UI.Xaml.Grid.GridItemsSourceChangedEventArgs e)
    {

        if (DataContext is IMainViewModel vm && AccountsGrid.View != null && AccountsGrid.View.Filter == null)
        {
            // Attach filtering handler
            AccountsGrid.View.Filter = vm.DoFilterGrid;
            //SecretsGrid.View.RefreshFilter(true); 


            // Setup RefreshFilter callable from View
            vm.RequestGridFilterRefresh = () =>
            {
                if (!Dispatcher.CheckAccess())
                    Dispatcher.BeginInvoke(new Action(() => AccountsGrid.View?.RefreshFilter()));
                else
                    AccountsGrid.View?.RefreshFilter();
            };

            // Filters the grid datasource based on vm.DoFilterGrid
            AccountsGrid.View.RefreshFilter();
        }
    }
}
