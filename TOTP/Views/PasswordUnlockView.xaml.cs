using System.Windows;
using System.Windows.Controls;
using TOTP.UserControls;

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
        //Loaded += (_, __) => System.Diagnostics.Debug.WriteLine($"PasswordUnlockView.AutoFocus={AutoFocus}");
    }
}
