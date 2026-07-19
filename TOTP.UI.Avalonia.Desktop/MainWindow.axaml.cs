using Avalonia.Controls;
using Avalonia;
using Avalonia.Input;
using Avalonia.Interactivity;
using Avalonia.Threading;
using Avalonia.VisualTree;
using TOTP.Avalonia.Desktop.Presentation;
using TOTP.Avalonia.Desktop.Platform;

namespace TOTP.Avalonia.Desktop;

public partial class MainWindow : Window
{
    private bool _initialized;

    public AvaloniaClipboardAccessor? ClipboardAccessor { get; init; }
    public AvaloniaStorageProviderAccessor? StorageProviderAccessor { get; init; }
    public AvaloniaWindowCoordinator? WindowCoordinator { get; init; }

    public MainWindow()
    {
        InitializeComponent();
    }

    protected override void OnOpened(EventArgs e)
    {
        base.OnOpened(e);
        WindowCoordinator?.RegisterMainWindow(this);
        if (Clipboard is not null)
            ClipboardAccessor?.Set(Clipboard);
        StorageProviderAccessor?.Set(StorageProvider);
        if (!_initialized && DataContext is MainWindowViewModel viewModel)
        {
            _initialized = true;
            viewModel.InitializeCommand.Execute(null);
        }
    }

    protected override void OnClosed(EventArgs e)
    {
        WindowCoordinator?.UnregisterMainWindow(this);
        base.OnClosed(e);
    }

    protected override void OnClosing(WindowClosingEventArgs e)
    {
        if (DataContext is MainWindowViewModel viewModel)
            viewModel.PrepareForShutdown();
        base.OnClosing(e);
    }

    protected override void OnPropertyChanged(AvaloniaPropertyChangedEventArgs change)
    {
        base.OnPropertyChanged(change);
        if (change.Property == WindowStateProperty
            && change.NewValue is WindowState.Minimized
            && DataContext is MainWindowViewModel viewModel)
        {
            _ = viewModel.HandleWindowMinimizedAsync();
        }
    }

    protected override void OnKeyDown(KeyEventArgs e)
    {
        if (e.Key == Key.F
            && e.KeyModifiers.HasFlag(KeyModifiers.Control)
            && DataContext is MainWindowViewModel viewModel
            && viewModel.IsShellVisible)
        {
            FocusAccountSearch();
            e.Handled = true;
        }

        base.OnKeyDown(e);
    }

    private void FocusAccountSearch(object? sender, RoutedEventArgs e) =>
        FocusAccountSearch();

    private void FocusAccountSearch()
    {
        if (DataContext is not MainWindowViewModel viewModel || !viewModel.IsShellVisible)
            return;
        if (viewModel.ShowAccountsCommand.CanExecute(null))
            viewModel.ShowAccountsCommand.Execute(null);
        Dispatcher.UIThread.Post(
            () => this.GetVisualDescendants()
                .OfType<TextBox>()
                .FirstOrDefault(control => control.Name == "AccountSearchBox")
                ?.Focus(),
            DispatcherPriority.Input);
    }
}
