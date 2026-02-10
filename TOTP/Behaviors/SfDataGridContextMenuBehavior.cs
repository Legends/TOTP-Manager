using Microsoft.Xaml.Behaviors;
using Syncfusion.UI.Xaml.Grid;
using System.Windows;
using TOTP.Services.Interfaces;

namespace TOTP.Behaviors;

public class SfDataGridContextMenuBehavior : Behavior<SfDataGrid>
{
    private IMainViewModel? _vm;

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

    private void OnContextMenuOpening(object? sender, GridContextMenuEventArgs e)
    {
        if (e.ContextMenuType == ContextMenuType.RecordCell)
            if (AssociatedObject.DataContext is IMainViewModel vm)
            {
                _vm = vm;
                vm.IsContextmenuOpen = true;

                e.ContextMenu.Closed -= ContextMenu_Closed;
                e.ContextMenu.Closed += ContextMenu_Closed;
            }
    }

    private void ContextMenu_Closed(object sender, RoutedEventArgs e)
    {
        if (_vm is null) return;

        _vm.IsContextmenuOpen = false;
    }
}