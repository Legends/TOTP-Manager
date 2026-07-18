using Avalonia.Controls;
using TOTP.Avalonia.Desktop.Presentation;
using TOTP.Avalonia.Desktop.Platform;

namespace TOTP.Avalonia.Desktop;

public partial class MainWindow : Window
{
    private bool _initialized;

    public AvaloniaClipboardAccessor? ClipboardAccessor { get; init; }
    public AvaloniaStorageProviderAccessor? StorageProviderAccessor { get; init; }

    public MainWindow()
    {
        InitializeComponent();
    }

    protected override void OnOpened(EventArgs e)
    {
        base.OnOpened(e);
        if (Clipboard is not null)
            ClipboardAccessor?.Set(Clipboard);
        StorageProviderAccessor?.Set(StorageProvider);
        if (!_initialized && DataContext is MainWindowViewModel viewModel)
        {
            _initialized = true;
            viewModel.InitializeCommand.Execute(null);
        }
    }

    protected override void OnClosing(WindowClosingEventArgs e)
    {
        if (DataContext is MainWindowViewModel viewModel)
            viewModel.PrepareForShutdown();
        base.OnClosing(e);
    }
}
