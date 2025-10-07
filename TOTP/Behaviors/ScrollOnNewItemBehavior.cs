using Microsoft.Xaml.Behaviors;
using Syncfusion.UI.Xaml.Grid;
using Syncfusion.UI.Xaml.ScrollAxis;
using System;
using System.Collections.Specialized;
using System.Reflection;
using System.Windows.Data;
using System.Windows.Threading;

namespace TOTP.Behaviors;

public class ScrollOnNewOrUpdatedItemBehavior : Behavior<SfDataGrid>
{
    public string NewFlagPropertyName { get; set; } = "IsNewlyAdded";
    public string UpdatedFlagPropertyName { get; set; } = "IsUpdated";
    public string HighlightPropertyName { get; set; } = "IsHighlighted";
    public TimeSpan HighlightDuration { get; set; } = TimeSpan.FromMilliseconds(800);
    public bool AutoScroll { get; set; } = true;

    private INotifyCollectionChanged? _subscribed;

    protected override void OnAttached()
    {
        base.OnAttached();
        AssociatedObject.ItemsSourceChanged += OnItemsSourceChanged;
        TrySubscribe(GetNcc(AssociatedObject.ItemsSource, AssociatedObject.View));
    }

    protected override void OnDetaching()
    {
        AssociatedObject.ItemsSourceChanged -= OnItemsSourceChanged;
        TryUnsubscribe();
        base.OnDetaching();
    }

    private void OnItemsSourceChanged(object? sender, GridItemsSourceChangedEventArgs e)
    {
        TryUnsubscribe();
        TrySubscribe(GetNcc(e.NewItemsSource, e.NewView));
    }

    private static INotifyCollectionChanged? GetNcc(object? itemsSource, object? viewObj)
    {
        return itemsSource as INotifyCollectionChanged
            ?? viewObj as INotifyCollectionChanged
            ?? CollectionViewSource.GetDefaultView(itemsSource);
    }

    private void TrySubscribe(INotifyCollectionChanged? ncc)
    {
        if (ncc == null) return;
        _subscribed = ncc;
        _subscribed.CollectionChanged += OnCollectionChanged;
    }

    private void TryUnsubscribe()
    {
        if (_subscribed != null)
            _subscribed.CollectionChanged -= OnCollectionChanged;
        _subscribed = null;
    }

    private void OnCollectionChanged(object? sender, NotifyCollectionChangedEventArgs e)
    {
        if (e.NewItems == null) return;

        foreach (var item in e.NewItems)
        {
            if (item == null) continue;

            if (e.Action != NotifyCollectionChangedAction.Add && e.Action != NotifyCollectionChangedAction.Replace)
                return;

            var grid = AssociatedObject;
            if (grid == null) continue;

            grid.Dispatcher.BeginInvoke(new Action(() =>
            {
                if (AutoScroll)
                    DoScroll(grid, item, () => DoGlowOnce(item));
                else
                    DoGlowOnce(item);

            }), DispatcherPriority.Loaded);
        }
    }

    private void DoScroll(SfDataGrid grid, object item, Action onFinally)
    {
        int rowIndex = grid.ResolveToRowIndex(item);
        if (rowIndex < 0) return;// cancel if not found (filter applied)
        var rci = new RowColumnIndex(rowIndex, 0);
        grid.ScrollInView(rci);
        onFinally?.Invoke();
    }

    private void DoGlowOnce(object item)
    {
        TrySetBool(item, HighlightPropertyName, true);
        //TrySetBool(item, NewFlagPropertyName, false);
        //TrySetBool(item, UpdatedFlagPropertyName, false);

        var disp = AssociatedObject?.Dispatcher;
        if (disp == null) return;

        var t = new DispatcherTimer(DispatcherPriority.Background, disp)
        {
            Interval = HighlightDuration
        };
        t.Tick += (_, __) =>
        {
            try { TrySetBool(item, HighlightPropertyName, false); } catch { }
            t.Stop();
        };
        t.Start();
    }

    private static bool TryGetBool(object obj, string propName, out bool value)
    {
        value = false;
        var p = obj.GetType().GetProperty(propName, BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
        if (p == null || p.PropertyType != typeof(bool)) return false;
        value = (bool)(p.GetValue(obj) ?? false);
        return true;
    }

    private static void TrySetBool(object obj, string propName, bool val)
    {
        var p = obj.GetType().GetProperty(propName, BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
        if (p == null || p.PropertyType != typeof(bool) || !p.CanWrite) return;
        p.SetValue(obj, val);
    }
}
