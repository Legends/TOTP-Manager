using System.Windows;
using System.Windows.Input;
using Microsoft.Xaml.Behaviors;
using Syncfusion.UI.Xaml.Grid;

namespace TOTP.Behaviors;

public class SfDataGridEditingBehavior : Behavior<SfDataGrid>
{
    public static readonly DependencyProperty BeginEditCommandProperty =
        DependencyProperty.Register(nameof(BeginEditCommand), typeof(ICommand), typeof(SfDataGridEditingBehavior));

    public static readonly DependencyProperty EndEditCommandProperty =
        DependencyProperty.Register(nameof(EndEditCommand), typeof(ICommand), typeof(SfDataGridEditingBehavior));

    public static readonly DependencyProperty DoubleClickCommandProperty =
        DependencyProperty.Register(nameof(DoubleClickCommand), typeof(ICommand), typeof(SfDataGridEditingBehavior));

    public ICommand? BeginEditCommand
    {
        get => (ICommand?)GetValue(BeginEditCommandProperty);
        set => SetValue(BeginEditCommandProperty, value);
    }

    public ICommand? EndEditCommand
    {
        get => (ICommand?)GetValue(EndEditCommandProperty);
        set => SetValue(EndEditCommandProperty, value);
    }

    public ICommand? DoubleClickCommand
    {
        get => (ICommand?)GetValue(DoubleClickCommandProperty);
        set => SetValue(DoubleClickCommandProperty, value);
    }

    protected override void OnAttached()
    {
        base.OnAttached();
        AssociatedObject.CurrentCellBeginEdit += OnBeginEdit;
        AssociatedObject.CurrentCellEndEdit += OnEndEdit;
        AssociatedObject.MouseDoubleClick += OnMouseDoubleClick;
    }

    protected override void OnDetaching()
    {
        base.OnDetaching();
        AssociatedObject.CurrentCellBeginEdit -= OnBeginEdit;
        AssociatedObject.CurrentCellEndEdit -= OnEndEdit;
        AssociatedObject.MouseDoubleClick -= OnMouseDoubleClick;
    }

    private void OnBeginEdit(object? sender, CurrentCellBeginEditEventArgs e)
    {
        if (AssociatedObject.SelectedItem != null &&
            BeginEditCommand?.CanExecute(AssociatedObject.SelectedItem) == true)
            BeginEditCommand.Execute(AssociatedObject.SelectedItem);
    }

    private void OnEndEdit(object? sender, CurrentCellEndEditEventArgs e)
    {
        if (AssociatedObject.SelectedItem != null && EndEditCommand?.CanExecute(AssociatedObject.SelectedItem) == true)
            EndEditCommand.Execute(AssociatedObject.SelectedItem);
    }

    private void OnMouseDoubleClick(object sender, MouseButtonEventArgs e)
    {
        if (AssociatedObject.SelectedItem != null &&
            DoubleClickCommand?.CanExecute(AssociatedObject.SelectedItem) == true)
            DoubleClickCommand.Execute(AssociatedObject.SelectedItem);
    }
}