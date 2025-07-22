using Microsoft.Xaml.Behaviors;
using Syncfusion.UI.Xaml.Grid;
using System.Windows;

namespace TOTP.Behaviors;

public class SfDataGridContextMenuBehavior : Behavior<SfDataGrid>
{
    IMainViewModel? _vm;
    protected override void OnAttached()
    {
        base.OnAttached();

        AssociatedObject.GridContextMenuOpening += OnContextMenuOpening;
    }

    protected override void OnDetaching()
    {
        AssociatedObject.GridContextMenuOpening -= OnContextMenuOpening;
        base.OnDetaching();
    }

    private void OnContextMenuOpening(object sender, GridContextMenuEventArgs e)
    {
        if (e.ContextMenuType == ContextMenuType.RecordCell)
        {
            _vm = AssociatedObject.DataContext as IMainViewModel;
            _vm.IsContextmenuOpen = true;

            // Subscribe to Closed (it is safe to remove first to avoid duplicate handlers)
            e.ContextMenu.Closed -= ContextMenu_Closed;
            e.ContextMenu.Closed += ContextMenu_Closed;
        }
    }

    private void ContextMenu_Closed(object sender, RoutedEventArgs e)
    {
        _vm.IsContextmenuOpen = false;
    }
}
