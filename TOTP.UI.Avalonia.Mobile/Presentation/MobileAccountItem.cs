namespace TOTP.Avalonia.Mobile.Presentation;

public sealed record MobileAccountItem(Guid Id, string Issuer, string AccountName)
{
    public bool HasAccountName => AccountName.Length > 0;

    public string DisplayName => HasAccountName
        ? $"{Issuer} · {AccountName}"
        : Issuer;
}
