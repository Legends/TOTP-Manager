namespace TOTP.Avalonia.Desktop.Presentation;

internal static class AccountListFilter
{
    public static IReadOnlyList<AccountListItemViewModel> Apply(
        IReadOnlyList<AccountListItemViewModel> accounts,
        string? searchText)
    {
        ArgumentNullException.ThrowIfNull(accounts);
        var query = searchText?.Trim() ?? string.Empty;
        return query.Length == 0
            ? accounts
            : accounts
                .Where(account =>
                    account.Issuer.Contains(query, StringComparison.OrdinalIgnoreCase)
                    || account.AccountName.Contains(query, StringComparison.OrdinalIgnoreCase))
                .ToArray();
    }
}
