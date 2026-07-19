using Avalonia;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Interactivity;
using Avalonia.Threading;
using TOTP.Avalonia.Desktop.Presentation;

namespace TOTP.Avalonia.Desktop.Controls;

public sealed class ContextPreservingAccountListBox : ListBox
{
    protected override Type StyleKeyOverride => typeof(ListBox);

    public static readonly DirectProperty<ContextPreservingAccountListBox,
        AccountListItemViewModel?> ContextAccountProperty =
        AvaloniaProperty.RegisterDirect<ContextPreservingAccountListBox,
            AccountListItemViewModel?>(
            nameof(ContextAccount),
            control => control.ContextAccount,
            (control, value) => control.ContextAccount = value,
            defaultBindingMode: global::Avalonia.Data.BindingMode.TwoWay);

    private AccountListItemViewModel? _contextAccount;

    public ContextPreservingAccountListBox()
    {
        AddHandler(ContextRequestedEvent, OnContextRequested);
        AddHandler(
            PointerPressedEvent,
            OnPreviewPointerPressed,
            RoutingStrategies.Tunnel,
            handledEventsToo: true);
        AddHandler(
            PointerReleasedEvent,
            OnPreviewPointerReleased,
            RoutingStrategies.Tunnel,
            handledEventsToo: true);
    }

    public AccountListItemViewModel? ContextAccount
    {
        get => _contextAccount;
        set => SetAndRaise(ContextAccountProperty, ref _contextAccount, value);
    }

    private void OnPreviewPointerPressed(object? sender, PointerPressedEventArgs e)
    {
        if (e.GetCurrentPoint(this).Properties.IsRightButtonPressed)
        {
            ContextAccount = (e.Source as StyledElement)?.DataContext
                as AccountListItemViewModel;
            e.Handled = true;
            return;
        }

        ContextAccount = null;
    }

    private void OnPreviewPointerReleased(object? sender, PointerReleasedEventArgs e)
    {
        if (e.InitialPressMouseButton == MouseButton.Right)
        {
            if (ContextAccount is not null) ContextMenu?.Open(this);
            e.Handled = true;
            return;
        }

    }

    private void OnContextRequested(object? sender, ContextRequestedEventArgs e)
    {
        ContextAccount ??= SelectedItem as AccountListItemViewModel;
    }

    protected override void OnPropertyChanged(AvaloniaPropertyChangedEventArgs change)
    {
        if (change.Property == ContextMenuProperty)
        {
            if (change.OldValue is ContextMenu previous) previous.Closed -= OnContextMenuClosed;
            if (change.NewValue is ContextMenu current) current.Closed += OnContextMenuClosed;
        }

        if (change.Property == SelectedItemProperty
            && change.NewValue is AccountListItemViewModel selectedAccount)
        {
            Dispatcher.UIThread.Post(
                () => ScrollIntoView(selectedAccount),
                DispatcherPriority.Loaded);
        }

        base.OnPropertyChanged(change);
    }

    private void OnContextMenuClosed(object? sender, RoutedEventArgs e) =>
        ContextAccount = null;
}
