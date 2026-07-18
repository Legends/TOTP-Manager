using Avalonia.Controls;
using TOTP.Avalonia.Desktop.Presentation;

namespace TOTP.Avalonia.Desktop;

public partial class MainWindow : Window
{
    public MainWindow()
    {
        InitializeComponent();
    }

    protected override void OnOpened(EventArgs e)
    {
        base.OnOpened(e);
        if (DataContext is MainWindowViewModel viewModel)
            viewModel.InitializeCommand.Execute(null);
    }
}
