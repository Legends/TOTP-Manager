

using Microsoft.Xaml.Behaviors;
using Syncfusion.UI.Xaml.Grid;
using System.Windows;

namespace TOTP.Behaviors;

public class DataGridEditingStateBehavior : Behavior<SfDataGrid>
{
    public static readonly DependencyProperty IsEditingProperty =
        DependencyProperty.Register(
            nameof(IsEditing),
            typeof(bool),
            typeof(DataGridEditingStateBehavior),
            new FrameworkPropertyMetadata(false, FrameworkPropertyMetadataOptions.BindsTwoWayByDefault));

    public bool IsEditing
    {
        get => (bool)GetValue(IsEditingProperty);
        set => SetValue(IsEditingProperty, value);
    }

    protected override void OnAttached()
    {
        AssociatedObject.CurrentCellBeginEdit += OnBegin;
        AssociatedObject.CurrentCellEndEdit += OnEnd;

    }

    protected override void OnDetaching()
    {
        AssociatedObject.CurrentCellBeginEdit -= OnBegin;
        AssociatedObject.CurrentCellEndEdit -= OnEnd;

    }

    private void OnBegin(object? s, CurrentCellBeginEditEventArgs e)
    {
        IsEditing = true;
    }
    private void OnEnd(object? s, CurrentCellEndEditEventArgs e)
    {
        IsEditing = false;
    }
}

