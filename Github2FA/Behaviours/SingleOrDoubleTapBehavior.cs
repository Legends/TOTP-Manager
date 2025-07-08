using System;
using System.Windows;
using System.Windows.Input;
using System.Windows.Threading;
using Microsoft.Xaml.Behaviors;
using Syncfusion.UI.Xaml.Grid;


namespace Github2FA.Behaviors;

public class SingleOrDoubleTapBehavior : Behavior<SfDataGrid>
{
    private DispatcherTimer _tapTimer;
    private bool _doubleTapOccurred;

    public int TapDelay
    {
        get { return (int)GetValue(TapDelayProperty); }
        set { SetValue(TapDelayProperty, value); }
    }

    public static readonly DependencyProperty TapDelayProperty =
        DependencyProperty.Register(nameof(TapDelay), typeof(int), typeof(SingleOrDoubleTapBehavior), new PropertyMetadata(100));

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

        _tapTimer = new DispatcherTimer
        {
            Interval = TimeSpan.FromMilliseconds(TapDelay)
        };
        _tapTimer.Tick += TapTimer_Tick;
    }

    protected override void OnDetaching()
    {
        base.OnDetaching();

        AssociatedObject.CellTapped -= AssociatedObject_CellTapped;
        AssociatedObject.CellDoubleTapped -= AssociatedObject_CellDoubleTapped;

        if (_tapTimer != null)
        {
            _tapTimer.Stop();
            _tapTimer.Tick -= TapTimer_Tick;
            _tapTimer = null;
        }
    }

    private void AssociatedObject_CellTapped(object sender, GridCellTappedEventArgs e)
    {
        _doubleTapOccurred = false;
        _tapTimer.Start();
    }

    private void AssociatedObject_CellDoubleTapped(object sender, GridCellDoubleTappedEventArgs e)
    {
        _doubleTapOccurred = true;
        _tapTimer.Stop();

        if (DoubleTapCommand != null && DoubleTapCommand.CanExecute(e))
        {
            DoubleTapCommand.Execute(e);
        }
    }

    private void TapTimer_Tick(object sender, EventArgs e)
    {
        _tapTimer.Stop();

        if (!_doubleTapOccurred)
        {
            if (SingleTapCommand != null && SingleTapCommand.CanExecute(null))
            {
                SingleTapCommand.Execute(null);
            }
        }
    }
}
