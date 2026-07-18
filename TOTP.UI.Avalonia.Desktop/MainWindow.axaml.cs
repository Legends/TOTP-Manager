using Avalonia.Controls;
using Avalonia;
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
}
