using System.Windows;
using TOTP.Interfaces;

namespace TOTP.Services;

public class MessageService : IMessageService
{
    public void ShowMessage(string message, string caption = "Info")
    {
        MessageBox.Show(message, caption);
    }

    public bool ShowMessageDialog(string message, string caption = "Info")
    {
        var result = MessageBox.Show(message, caption, MessageBoxButton.YesNo, MessageBoxImage.Question);
        return result == MessageBoxResult.Yes;
    }
}