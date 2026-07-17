using System.Windows;
using TOTP.Services.Interfaces;
using TOTP.Views;

namespace TOTP.Services;

public sealed class PasswordPromptDialogFactory : IPasswordPromptDialogFactory
{
    public IPasswordPromptDialog CreateExportPasswordPromptDialog()
        => Create(new ExportPasswordPromptWindow());

    public IPasswordPromptDialog CreatePasswordPromptDialog()
        => Create(new PasswordPromptWindow());

    private static IPasswordPromptDialog Create(Window window)
    {
        window.Owner = Application.Current?.MainWindow;
        return new WindowDialogAdapter(window);
    }

    private sealed class WindowDialogAdapter(Window window) : IPasswordPromptDialog
    {
        public object? DataContext
        {
            get => window.DataContext;
            set => window.DataContext = value;
        }

        public bool? ShowDialog() => window.ShowDialog();
    }
}
