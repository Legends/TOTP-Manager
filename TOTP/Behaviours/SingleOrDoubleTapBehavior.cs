using Microsoft.Xaml.Behaviors;
using Syncfusion.UI.Xaml.Grid;
using System.Diagnostics;
using System.Windows;
using System.Windows.Input;


namespace TOTP.Behaviors;

public class SingleOrDoubleTapBehavior : Behavior<SfDataGrid>
{

    private bool _doubleTapOccurred;

    public int TapDelay
    {
        get { return (int)GetValue(TapDelayProperty); }
        set { SetValue(TapDelayProperty, value); }
    }

    public static readonly DependencyProperty TapDelayProperty =
    DependencyProperty.Register(nameof(TapDelay), typeof(int), typeof(SingleOrDoubleTapBehavior), new PropertyMetadata(300));


    public ICommand SingleTapCommand
    {
        get { return (ICommand)GetValue(SingleTapCommandProperty); }
        set { SetValue(SingleTapCommandProperty, value); }
    }

    public static readonly DependencyProperty SingleTapCommandProperty =
        DependencyProperty.Register(nameof(SingleTapCommand), typeof(ICommand), typeof(SingleOrDoubleTapBehavior), new PropertyMetadata(null));

    public ICommand DoubleTapCommand
    {
        get { return (ICommand)GetValue(DoubleTapCommandProperty); }
        set { SetValue(DoubleTapCommandProperty, value); }
    }

    public static readonly DependencyProperty DoubleTapCommandProperty =
        DependencyProperty.Register(nameof(DoubleTapCommand), typeof(ICommand), typeof(SingleOrDoubleTapBehavior), new PropertyMetadata(null));

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

    private void AssociatedObject_CellTapped(object sender, GridCellTappedEventArgs e)
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


    private void AssociatedObject_CellDoubleTapped(object sender, GridCellDoubleTappedEventArgs e)
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
