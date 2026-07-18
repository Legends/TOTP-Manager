namespace TOTP.Avalonia.Desktop.Localization;

public static class AvaloniaStringKeys
{
    public const string AppTitle = nameof(AppTitle);
    public const string AppHeading = nameof(AppHeading);
    public const string StartingSafely = nameof(StartingSafely);
    public const string Retry = nameof(Retry);
    public const string MasterPassword = nameof(MasterPassword);
    public const string MasterPasswordHelp = nameof(MasterPasswordHelp);
    public const string Unlock = nameof(Unlock);
    public const string Lock = nameof(Lock);
    public const string Accounts = nameof(Accounts);
    public const string Tools = nameof(Tools);
    public const string Settings = nameof(Settings);
    public const string ChooseImportFile = nameof(ChooseImportFile);
    public const string VerifyAppcast = nameof(VerifyAppcast);
    public const string ScanQrCamera = nameof(ScanQrCamera);
    public const string CancelScan = nameof(CancelScan);
    public const string SecuritySettings = nameof(SecuritySettings);
    public const string Language = nameof(Language);
    public const string IdleTimeout = nameof(IdleTimeout);
    public const string IdleTimeoutHelp = nameof(IdleTimeoutHelp);
    public const string LockWhenMinimized = nameof(LockWhenMinimized);
    public const string SaveSettings = nameof(SaveSettings);
    public const string SearchAccounts = nameof(SearchAccounts);
    public const string GenerateCode = nameof(GenerateCode);
    public const string ShowQrCode = nameof(ShowQrCode);
    public const string CopyTimedClear = nameof(CopyTimedClear);
    public const string Password = nameof(Password);
    public const string PasswordAuthorizationHelp = nameof(PasswordAuthorizationHelp);
    public const string ValidatingSecurely = nameof(ValidatingSecurely);
    public const string NavAccounts = nameof(NavAccounts);
    public const string NavTools = nameof(NavTools);
    public const string NavSettings = nameof(NavSettings);
    public const string AccountsList = nameof(AccountsList);
    public const string GeneratedCode = nameof(GeneratedCode);
    public const string GeneratedQr = nameof(GeneratedQr);
    public const string QrPrivacyNotice = nameof(QrPrivacyNotice);
    public const string CameraPreview = nameof(CameraPreview);
    public const string CreatePasswordHeading = nameof(CreatePasswordHeading);
    public const string PasswordSetupHelp = nameof(PasswordSetupHelp);
    public const string NewPassword = nameof(NewPassword);
    public const string ConfirmPassword = nameof(ConfirmPassword);
    public const string CreateVault = nameof(CreateVault);
    public const string PasswordRequired = nameof(PasswordRequired);
    public const string PasswordMinimumLength = nameof(PasswordMinimumLength);
    public const string PasswordMismatch = nameof(PasswordMismatch);
    public const string PasswordSetupFailed = nameof(PasswordSetupFailed);
    public const string ExistingVaultConflict = nameof(ExistingVaultConflict);
    public const string VaultConfigured = nameof(VaultConfigured);
    public const string VaultUnlocked = nameof(VaultUnlocked);
    public const string QuickUnlockFallback = nameof(QuickUnlockFallback);
    public const string QuickUnlock = nameof(QuickUnlock);
    public const string QuickUnlockRecoveryNotice = nameof(QuickUnlockRecoveryNotice);
    public const string QuickUnlockAvailable = nameof(QuickUnlockAvailable);
    public const string QuickUnlockUnavailable = nameof(QuickUnlockUnavailable);
    public const string EnableQuickUnlock = nameof(EnableQuickUnlock);
    public const string QuickUnlockRecoveryPrompt = nameof(QuickUnlockRecoveryPrompt);
    public const string QuickUnlockEnrollmentCancelled = nameof(QuickUnlockEnrollmentCancelled);
    public const string QuickUnlockEnrollmentFailed = nameof(QuickUnlockEnrollmentFailed);
    public const string QuickUnlockEnabled = nameof(QuickUnlockEnabled);
    public const string UsePasswordAtStartup = nameof(UsePasswordAtStartup);
    public const string PasswordPreferencePrompt = nameof(PasswordPreferencePrompt);
    public const string PasswordPreferenceUnchanged = nameof(PasswordPreferenceUnchanged);
    public const string PasswordPreferenceFailed = nameof(PasswordPreferenceFailed);
    public const string PasswordPreferred = nameof(PasswordPreferred);
    public const string PasswordVerificationFailed = nameof(PasswordVerificationFailed);
    public const string Enable = nameof(Enable);
    public const string Confirm = nameof(Confirm);
    public const string Cancel = nameof(Cancel);
    public const string Refresh = nameof(Refresh);
    public const string ChangeMasterPassword = nameof(ChangeMasterPassword);
    public const string CurrentPasswordPrompt = nameof(CurrentPasswordPrompt);
    public const string PasswordChangeCancelled = nameof(PasswordChangeCancelled);
    public const string PasswordChangeFailed = nameof(PasswordChangeFailed);
    public const string PasswordChanged = nameof(PasswordChanged);
    public const string RecoveryAndCompatibility = nameof(RecoveryAndCompatibility);
    public const string PortableEnvelopeStatus = nameof(PortableEnvelopeStatus);
    public const string AutomaticRollbackStatus = nameof(AutomaticRollbackStatus);
    public const string LegacyMigrationStatus = nameof(LegacyMigrationStatus);
    public const string AddAccount = nameof(AddAccount);
    public const string EditAccount = nameof(EditAccount);
    public const string DeleteAccount = nameof(DeleteAccount);
    public const string DeleteAccountPrompt = nameof(DeleteAccountPrompt);
    public const string Delete = nameof(Delete);
    public const string Issuer = nameof(Issuer);
    public const string AccountName = nameof(AccountName);
    public const string Secret = nameof(Secret);
    public const string SaveAccount = nameof(SaveAccount);
    public const string CancelEdit = nameof(CancelEdit);
    public const string AccountIssuerRequired = nameof(AccountIssuerRequired);
    public const string AccountSecretInvalid = nameof(AccountSecretInvalid);
    public const string AccountDuplicate = nameof(AccountDuplicate);
    public const string AccountSaveFailed = nameof(AccountSaveFailed);
    public const string AccountSaved = nameof(AccountSaved);
    public const string AccountEditLoadFailed = nameof(AccountEditLoadFailed);
    public const string AccountDeleteFailed = nameof(AccountDeleteFailed);
    public const string AccountDeleted = nameof(AccountDeleted);
    public const string CodeAutoRefreshReady = nameof(CodeAutoRefreshReady);
    public const string CodeRefreshed = nameof(CodeRefreshed);
    public const string CodeRefreshFailed = nameof(CodeRefreshFailed);
    public const string CodeCopiedWithClear = nameof(CodeCopiedWithClear);
    public const string ClipboardSafeCopyUnavailable = nameof(ClipboardSafeCopyUnavailable);
    public const string CodeRemainingTime = nameof(CodeRemainingTime);

    public static IReadOnlyList<string> All { get; } =
    [
        AppTitle, AppHeading, StartingSafely, Retry, MasterPassword, MasterPasswordHelp,
        Unlock, Lock, Accounts, Tools, Settings, ChooseImportFile, VerifyAppcast,
        ScanQrCamera, CancelScan, SecuritySettings, Language, IdleTimeout,
        IdleTimeoutHelp, LockWhenMinimized, SaveSettings, SearchAccounts, GenerateCode,
        ShowQrCode, CopyTimedClear, Password, PasswordAuthorizationHelp,
        ValidatingSecurely, NavAccounts, NavTools, NavSettings, AccountsList,
        GeneratedCode, GeneratedQr, QrPrivacyNotice, CameraPreview, CreatePasswordHeading,
        PasswordSetupHelp, NewPassword, ConfirmPassword, CreateVault, PasswordRequired,
        PasswordMinimumLength, PasswordMismatch, PasswordSetupFailed, ExistingVaultConflict,
        VaultConfigured, VaultUnlocked, QuickUnlockFallback, QuickUnlock,
        QuickUnlockRecoveryNotice, QuickUnlockAvailable, QuickUnlockUnavailable,
        EnableQuickUnlock, QuickUnlockRecoveryPrompt, QuickUnlockEnrollmentCancelled,
        QuickUnlockEnrollmentFailed, QuickUnlockEnabled, UsePasswordAtStartup,
        PasswordPreferencePrompt, PasswordPreferenceUnchanged, PasswordPreferenceFailed,
        PasswordPreferred, PasswordVerificationFailed, Enable, Confirm, Cancel, Refresh,
        ChangeMasterPassword, CurrentPasswordPrompt, PasswordChangeCancelled,
        PasswordChangeFailed, PasswordChanged, RecoveryAndCompatibility,
        PortableEnvelopeStatus, AutomaticRollbackStatus, LegacyMigrationStatus,
        AddAccount, EditAccount, DeleteAccount, DeleteAccountPrompt, Delete, Issuer,
        AccountName, Secret, SaveAccount, CancelEdit, AccountIssuerRequired,
        AccountSecretInvalid, AccountDuplicate, AccountSaveFailed, AccountSaved,
        AccountEditLoadFailed, AccountDeleteFailed, AccountDeleted, CodeAutoRefreshReady,
        CodeRefreshed, CodeRefreshFailed, CodeCopiedWithClear,
        ClipboardSafeCopyUnavailable, CodeRemainingTime
    ];
}
