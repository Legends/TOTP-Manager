using System.Windows;
using System.Windows.Controls;
using TOTP.Services.Interfaces;

namespace TOTP.Services;

public sealed class PasswordPromptService : IPasswordPromptService
{
    public string? Prompt(string title, string message)
    {
        var passwordBox = new PasswordBox
        {
            Margin = new Thickness(0, 8, 0, 12),
            MinWidth = 260
        };

        var okButton = new Button
        {
            Content = "OK",
            Width = 80,
            IsDefault = true,
            Margin = new Thickness(0, 0, 8, 0)
        };
        var cancelButton = new Button
        {
            Content = "Cancel",
            Width = 80,
            IsCancel = true
        };

        var buttonRow = new StackPanel
        {
            Orientation = Orientation.Horizontal,
            HorizontalAlignment = HorizontalAlignment.Right
        };
        buttonRow.Children.Add(okButton);
        buttonRow.Children.Add(cancelButton);

        var root = new StackPanel { Margin = new Thickness(16) };
        root.Children.Add(new TextBlock
        {
            Text = message,
            TextWrapping = TextWrapping.Wrap
        });
        root.Children.Add(passwordBox);
        root.Children.Add(buttonRow);

        var dialog = new Window
        {
            Title = title,
            Owner = Application.Current?.MainWindow,
            WindowStartupLocation = WindowStartupLocation.CenterOwner,
            WindowStyle = WindowStyle.SingleBorderWindow,
            ResizeMode = ResizeMode.NoResize,
            SizeToContent = SizeToContent.WidthAndHeight,
            ShowInTaskbar = false,
            Content = root
        };

        okButton.Click += (_, _) => dialog.DialogResult = true;

        var result = dialog.ShowDialog();
        if (result != true || string.IsNullOrWhiteSpace(passwordBox.Password))
        {
            return null;
        }

        return passwordBox.Password;
    }
}
