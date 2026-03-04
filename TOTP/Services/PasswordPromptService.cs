using System.Windows;
using TOTP.Services.Interfaces;
using TOTP.ViewModels;
using TOTP.Views;

namespace TOTP.Services;

public sealed class PasswordPromptService : IPasswordPromptService
{
    public string? Prompt(string title, string message)
    {
        var viewModel = new PasswordPromptViewModel(title, message);

        var dialog = new PasswordPromptWindow
        {
            DataContext = viewModel,
            Owner = Application.Current?.MainWindow,
            WindowStyle = WindowStyle.ToolWindow
        };

        var result = dialog.ShowDialog();
        if (result != true || string.IsNullOrWhiteSpace(viewModel.Password))
        {
            return null;
        }

        return viewModel.Password;
    }
}
