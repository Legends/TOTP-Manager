using Avalonia.Controls;
using TOTP.Avalonia.Desktop.Presentation;
using TOTP.Avalonia.Desktop.Platform;

namespace TOTP.Avalonia.Desktop;

public partial class MainWindow : Window
{
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
        if (DataContext is MainWindowViewModel viewModel)
            viewModel.InitializeCommand.Execute(null);
    }
}
