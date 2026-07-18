using Avalonia;
using Avalonia.Controls.Primitives;

namespace TOTP.Avalonia.Shared.Controls;

public sealed class AccountRow : TemplatedControl
{
    private string _accessibleName = "Account";

    public static readonly StyledProperty<string> IssuerProperty =
        AvaloniaProperty.Register<AccountRow, string>(nameof(Issuer), string.Empty);

    public static readonly StyledProperty<string> AccountNameProperty =
        AvaloniaProperty.Register<AccountRow, string>(nameof(AccountName), string.Empty);

    public static readonly DirectProperty<AccountRow, string> AccessibleNameProperty =
        AvaloniaProperty.RegisterDirect<AccountRow, string>(
            nameof(AccessibleName),
            static control => control.AccessibleName);

    static AccountRow()
    {
        IssuerProperty.Changed.AddClassHandler<AccountRow>(
            static (control, _) => control.UpdateAccessibleName());
        AccountNameProperty.Changed.AddClassHandler<AccountRow>(
            static (control, _) => control.UpdateAccessibleName());
    }

    public string Issuer
    {
        get => GetValue(IssuerProperty);
        set => SetValue(IssuerProperty, value ?? string.Empty);
    }

    public string AccountName
    {
        get => GetValue(AccountNameProperty);
        set => SetValue(AccountNameProperty, value ?? string.Empty);
    }

    public string AccessibleName
    {
        get => _accessibleName;
        private set => SetAndRaise(AccessibleNameProperty, ref _accessibleName, value);
    }

    private void UpdateAccessibleName()
    {
        var issuer = Issuer.Trim();
        var accountName = AccountName.Trim();
        AccessibleName = (issuer.Length, accountName.Length) switch
        {
            (> 0, > 0) => $"{issuer}, {accountName}",
            (> 0, 0) => issuer,
            (0, > 0) => accountName,
            _ => "Account"
        };
    }
}
