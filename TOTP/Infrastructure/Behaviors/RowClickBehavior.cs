using System;
using System.Windows;
using System.Windows.Input;
using System.Windows.Threading;
using Microsoft.Xaml.Behaviors;
using Syncfusion.UI.Xaml.Grid;
using Syncfusion.UI.Xaml.Grid.Helpers;

namespace TOTP.Infrastructure.Behaviors;

public class RowClickBehavior : Behavior<SfDataGrid>
{
    private const int SingleClickDelayMs = 250;
    private DispatcherTimer? _singleClickTimer;
    private object? _pendingSingleClickItem;

    public static readonly DependencyProperty SelectionChangedCommandProperty =
        DependencyProperty.Register(nameof(SelectionChangedCommand), typeof(ICommand),
            typeof(RowClickBehavior), new PropertyMetadata(null));

    public ICommand SelectionChangedCommand
    {
        get => (ICommand)GetValue(SelectionChangedCommandProperty);
        set => SetValue(SelectionChangedCommandProperty, value);
    }

    protected override void OnAttached()
    {
        AssociatedObject.PreviewMouseDown += OnPreviewMouseDown;
        base.OnAttached();
    }

    protected override void OnDetaching()
    {
        AssociatedObject.PreviewMouseDown -= OnPreviewMouseDown;
        DisposeSingleClickTimer();
        base.OnDetaching();
    }

    private void OnPreviewMouseDown(object sender, MouseButtonEventArgs e)
    {
        var grid = AssociatedObject;
        var vc = grid.GetVisualContainer();
        var rc = vc.PointToCellRowColumnIndex(e.GetPosition(vc));

        if (rc.IsEmpty || rc.RowIndex <= 0 || grid.View == null)
        {
            return;
        }

        var recordIndex = grid.ResolveToRecordIndex(rc.RowIndex);
        if (recordIndex < 0 || recordIndex >= grid.View.Records.Count)
        {
            return;
        }

        var item = grid.View.Records[recordIndex].Data;
        if (item == null)
        {
            return;
        }

        if (e.ChangedButton == MouseButton.Right)
        {
            grid.SelectedItem = item;
            grid.CurrentItem = item;
            return;
        }

        if (e.ChangedButton != MouseButton.Left)
        {
            return;
        }

        grid.SelectedItem = item;
        grid.CurrentItem = item;

        if (e.ClickCount == 2)
        {
            CancelPendingSingleClick();
            return;
        }

        if (e.ClickCount == 1)
        {
            QueueSingleClick(item);
        }
    }

    private void QueueSingleClick(object item)
    {
        _pendingSingleClickItem = item;

        if (_singleClickTimer == null)
        {
            _singleClickTimer = new DispatcherTimer
            {
                Interval = TimeSpan.FromMilliseconds(SingleClickDelayMs)
            };
            _singleClickTimer.Tick += OnSingleClickTimerTick;
        }

        _singleClickTimer.Stop();
        _singleClickTimer.Start();
    }

    private void OnSingleClickTimerTick(object? sender, EventArgs e)
    {
        if (_singleClickTimer == null)
        {
            return;
        }

        _singleClickTimer.Stop();

        var item = _pendingSingleClickItem;
        _pendingSingleClickItem = null;

        if (item != null && SelectionChangedCommand != null && SelectionChangedCommand.CanExecute(item))
        {
            SelectionChangedCommand.Execute(item);
        }
    }

    private void CancelPendingSingleClick()
    {
        _pendingSingleClickItem = null;
        _singleClickTimer?.Stop();
    }

    private void DisposeSingleClickTimer()
    {
        if (_singleClickTimer == null)
        {
            return;
        }

        _singleClickTimer.Stop();
        _singleClickTimer.Tick -= OnSingleClickTimerTick;
        _singleClickTimer = null;
        _pendingSingleClickItem = null;
    }
}
