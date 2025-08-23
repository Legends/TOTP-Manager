using Microsoft.Extensions.Logging;
using Syncfusion.SfSkinManager;
using Syncfusion.UI.Xaml.Grid;
using Syncfusion.UI.Xaml.ScrollAxis;
using Syncfusion.Windows.Shared;
using System;
using System.Windows;
using System.Windows.Media;
using TOTP.Interfaces;
using TOTP.Resources;

namespace TOTP;

/// <summary>
///     Interaction logic for MainWindow.xaml
/// </summary>
public partial class MainWindow : ChromelessWindow
{
    private readonly IMainViewModel _vm;
    private ILogger<MainWindow> _logger;

    private void PasswordBox_LostFocus(object sender, RoutedEventArgs e)
    {
        var grid = FindParent<SfDataGrid>((DependencyObject)sender);
        if (grid == null || grid.CurrentCellInfo == null) return;

        // Move to another cell to trigger validation
        var currentIndex = 1;
        var nextIndex = currentIndex + 1 < grid.Columns.Count ? currentIndex + 1 : currentIndex - 1;

        if (nextIndex >= 0 && nextIndex < grid.Columns.Count)
        {
            grid.MoveCurrentCell(new RowColumnIndex(0, 0), true);
        }
    }



    public static T? FindParent<T>(DependencyObject child) where T : DependencyObject
    {
        DependencyObject parent = VisualTreeHelper.GetParent(child);
        while (parent != null && parent is not T)
        {
            parent = VisualTreeHelper.GetParent(parent);
        }
        return parent as T;
    }



    public MainWindow(IMainViewModel vm, ILogger<MainWindow> logger)
    {
        _logger = logger;
        InitializeComponent();
        // build action: Resource
        //this.Icon = new BitmapImage(new Uri("pack://application:,,,/Assets/Icons/github.ico"));
        //this.Icon = new BitmapImage(new Uri("pack://application:,,,/totp.ico"));

        DataContext = vm;
        Title = UI.ui_Window_Title_TOTP_Manager;

        _vm = vm ?? throw new ArgumentNullException(nameof(vm));
        SkinManagerHelper.SetScrollBarMode(this, ScrollBarMode.Compact);

        Loaded += OnLoadedAsync;
    }

    //private void OnAddClick(object sender, RoutedEventArgs e)
    //{
    //    if (SecretsGrid.View.CurrentEditItem != null)
    //        return;

    //    // now execute the VM command
    //    (DataContext as MainViewModel)?.AddNewSecretCommand.Execute(null);
    //}


    private async void OnLoadedAsync(object sender, RoutedEventArgs e)
    {
        Loaded -= OnLoadedAsync; // wichtig, um Mehrfachaufrufe zu vermeiden

        try
        {
            await _vm.InitializeAsync();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, nameof(OnLoadedAsync));
            Application.Current.Shutdown(-1);
        }
    }

    private async void DataGrid_SelectionChanged(object sender, GridSelectionChangedEventArgs e)
    {
        try
        {
            await _vm.OnSelectionChangedAsync();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex.Message, ex);
        }
    }

}