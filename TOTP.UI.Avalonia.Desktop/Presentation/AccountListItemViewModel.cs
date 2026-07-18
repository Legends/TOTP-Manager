namespace TOTP.Avalonia.Desktop.Presentation;

public sealed record AccountListItemViewModel(
    Guid Id,
    string Issuer,
    string AccountName);
