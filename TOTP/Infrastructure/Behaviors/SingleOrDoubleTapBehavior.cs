using Microsoft.Xaml.Behaviors;
using Syncfusion.UI.Xaml.Grid;
using System.Diagnostics;
using System.Windows;
using System.Windows.Input;

namespace TOTP.Infrastructure.Behaviors;

public class SingleOrDoubleTapBehavior : Behavior<SfDataGrid>
{
    public static readonly DependencyProperty TapDelayProperty =
        DependencyProperty.Register(nameof(TapDelay), typeof(int), typeof(SingleOrDoubleTapBehavior),
            new PropertyMetadata(300));

    public static readonly DependencyProperty SingleTapCommandProperty =
        DependencyProperty.Register(nameof(SingleTapCommand), typeof(ICommand), typeof(SingleOrDoubleTapBehavior),
            new PropertyMetadata(null));

    public static readonly DependencyProperty DoubleTapCommandProperty =
        DependencyProperty.Register(nameof(DoubleTapCommand), typeof(ICommand), typeof(SingleOrDoubleTapBehavior),
            new PropertyMetadata(null));

    //private readonly bool _doubleTapOccurred;

    public int TapDelay
    {
        get => (int)GetValue(TapDelayProperty);
        set => SetValue(TapDelayProperty, value);
    }


    public ICommand SingleTapCommand
    {
        get => (ICommand)GetValue(SingleTapCommandProperty);
        set => SetValue(SingleTapCommandProperty, value);
    }

    public ICommand DoubleTapCommand
    {
        get => (ICommand)GetValue(DoubleTapCommandProperty);
        set => SetValue(DoubleTapCommandProperty, value);
    }

    protected override void OnAttached()
    {
        base.OnAttached();

        AssociatedObject.CellTapped += AssociatedObject_CellTapped;
        AssociatedObject.CellDoubleTapped += AssociatedObject_CellDoubleTapped;
    }

    protected override void OnDetaching()
    {
        base.OnDetaching();

        AssociatedObject.CellTapped -= AssociatedObject_CellTapped;
        AssociatedObject.CellDoubleTapped -= AssociatedObject_CellDoubleTapped;
    }

    private void AssociatedObject_CellTapped(object? sender, GridCellTappedEventArgs e)
    {
        //_doubleTapOccurred = false;

        //// Schedule single tap execution after delay
        //AssociatedObject.Dispatcher.InvokeAsync(async () =>
        //{
        //    await Task.Delay(TapDelay);

        //    if (!_doubleTapOccurred)
        //    {
        //InvokeSingleTap();
        //    }
        //});
    }


    private void AssociatedObject_CellDoubleTapped(object? sender, GridCellDoubleTappedEventArgs e)
    {
        //_doubleTapOccurred = true;

        //if (DoubleTapCommand != null && DoubleTapCommand.CanExecute(e))
        //{
        DoubleTapCommand.Execute(e);
        //}
    }


    protected virtual void InvokeSingleTap()
    {
        if (SingleTapCommand != null && SingleTapCommand.CanExecute(null))
        {
            Debug.WriteLine(" SingleTapCommand.Execute(null);");
            SingleTapCommand.Execute(null);
        }
    }
}