using System.Windows;
using TOTP.Services.Interfaces;
using TOTP.Views;

namespace TOTP.Services;

public sealed class PasswordPromptDialogFactory : IPasswordPromptDialogFactory
{
    public IPasswordPromptDialog CreateExportPasswordPromptDialog()
        => new WindowDialogAdapter(new ExportPasswordPromptWindow());

    public IPasswordPromptDialog CreatePasswordPromptDialog()
        => new WindowDialogAdapter(new PasswordPromptWindow());

    private sealed class WindowDialogAdapter(Window window) : IPasswordPromptDialog
    {
        public object? DataContext
        {
            get => window.DataContext;
            set => window.DataContext = value;
        }

        public Window? Owner
        {
            get => window.Owner;
            set => window.Owner = value;
        }

        public bool? ShowDialog() => window.ShowDialog();
    }
}
