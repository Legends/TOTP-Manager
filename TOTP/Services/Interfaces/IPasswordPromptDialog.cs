using System.Windows;

namespace TOTP.Services.Interfaces;

public interface IPasswordPromptDialog
{
    object? DataContext { get; set; }
    Window? Owner { get; set; }
    bool? ShowDialog();
}
