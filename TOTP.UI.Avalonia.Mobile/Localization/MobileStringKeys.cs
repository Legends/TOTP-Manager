namespace TOTP.Avalonia.Mobile.Localization;

public static class MobileStringKeys
{
    public const string Starting = nameof(Starting);
    public const string StartupFailed = nameof(StartupFailed);
    public const string Retry = nameof(Retry);
    public const string SetupTitle = nameof(SetupTitle);
    public const string SetupDescription = nameof(SetupDescription);
    public const string MasterPassword = nameof(MasterPassword);
    public const string ConfirmPassword = nameof(ConfirmPassword);
    public const string CreateVault = nameof(CreateVault);
    public const string PasswordRequired = nameof(PasswordRequired);
    public const string PasswordMinimumLength = nameof(PasswordMinimumLength);
    public const string PasswordMismatch = nameof(PasswordMismatch);
    public const string SetupFailed = nameof(SetupFailed);
    public const string ExistingVaultConflict = nameof(ExistingVaultConflict);
    public const string UnlockTitle = nameof(UnlockTitle);
    public const string UnlockDescription = nameof(UnlockDescription);
    public const string Unlock = nameof(Unlock);
    public const string UnlockRejected = nameof(UnlockRejected);
    public const string UnlockFailed = nameof(UnlockFailed);
    public const string AccountsTitle = nameof(AccountsTitle);
    public const string NoAccounts = nameof(NoAccounts);
    public const string SelectAccount = nameof(SelectAccount);
    public const string AddAccount = nameof(AddAccount);
    public const string EditAccount = nameof(EditAccount);
    public const string DeleteAccount = nameof(DeleteAccount);
    public const string Lock = nameof(Lock);
    public const string Issuer = nameof(Issuer);
    public const string AccountName = nameof(AccountName);
    public const string Secret = nameof(Secret);
    public const string SecretOptionalOnEdit = nameof(SecretOptionalOnEdit);
    public const string Save = nameof(Save);
    public const string Cancel = nameof(Cancel);
    public const string IssuerRequired = nameof(IssuerRequired);
    public const string SecretRequired = nameof(SecretRequired);
    public const string SecretInvalid = nameof(SecretInvalid);
    public const string DuplicateAccount = nameof(DuplicateAccount);
    public const string AccountSaved = nameof(AccountSaved);
    public const string AccountSaveFailed = nameof(AccountSaveFailed);
    public const string DeleteAccountPrompt = nameof(DeleteAccountPrompt);
    public const string Delete = nameof(Delete);
    public const string AccountDeleted = nameof(AccountDeleted);
    public const string AccountDeleteFailed = nameof(AccountDeleteFailed);
    public const string CodeUnavailable = nameof(CodeUnavailable);
    public const string CopyCode = nameof(CopyCode);
    public const string CodeCopied = nameof(CodeCopied);
    public const string CodeCopiedWithClear = nameof(CodeCopiedWithClear);
    public const string CodeCopyFailed = nameof(CodeCopyFailed);
    public const string LoadingAccountsFailed = nameof(LoadingAccountsFailed);
    public const string EditorAddTitle = nameof(EditorAddTitle);
    public const string EditorEditTitle = nameof(EditorEditTitle);
    public const string DeleteConfirmTitle = nameof(DeleteConfirmTitle);

    public static IReadOnlyList<string> All { get; } =
    [
        Starting,
        StartupFailed,
        Retry,
        SetupTitle,
        SetupDescription,
        MasterPassword,
        ConfirmPassword,
        CreateVault,
        PasswordRequired,
        PasswordMinimumLength,
        PasswordMismatch,
        SetupFailed,
        ExistingVaultConflict,
        UnlockTitle,
        UnlockDescription,
        Unlock,
        UnlockRejected,
        UnlockFailed,
        AccountsTitle,
        NoAccounts,
        SelectAccount,
        AddAccount,
        EditAccount,
        DeleteAccount,
        Lock,
        Issuer,
        AccountName,
        Secret,
        SecretOptionalOnEdit,
        Save,
        Cancel,
        IssuerRequired,
        SecretRequired,
        SecretInvalid,
        DuplicateAccount,
        AccountSaved,
        AccountSaveFailed,
        DeleteAccountPrompt,
        Delete,
        AccountDeleted,
        AccountDeleteFailed,
        CodeUnavailable,
        CopyCode,
        CodeCopied,
        CodeCopiedWithClear,
        CodeCopyFailed,
        LoadingAccountsFailed,
        EditorAddTitle,
        EditorEditTitle,
        DeleteConfirmTitle
    ];
}
