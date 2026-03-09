using System.Windows;
using System.Windows.Controls;

namespace TOTP.Views;

public partial class PasswordUnlockView : UserControl
{
    public static readonly DependencyProperty AutoFocusProperty =
        DependencyProperty.Register(
            nameof(AutoFocus),
            typeof(bool),
            typeof(PasswordUnlockView),
            new PropertyMetadata(false));

    public bool AutoFocus
    {
        get => (bool)GetValue(AutoFocusProperty);
        set => SetValue(AutoFocusProperty, value);
    }

    public PasswordUnlockView()
    {
        InitializeComponent();
    }
}
