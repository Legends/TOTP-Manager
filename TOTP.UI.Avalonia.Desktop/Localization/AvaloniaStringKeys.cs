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

    public static IReadOnlyList<string> All { get; } =
    [
        AppTitle, AppHeading, StartingSafely, Retry, MasterPassword, MasterPasswordHelp,
        Unlock, Lock, Accounts, Tools, Settings, ChooseImportFile, VerifyAppcast,
        ScanQrCamera, CancelScan, SecuritySettings, Language, IdleTimeout,
        IdleTimeoutHelp, LockWhenMinimized, SaveSettings, SearchAccounts, GenerateCode,
        ShowQrCode, CopyTimedClear, Password, PasswordAuthorizationHelp,
        ValidatingSecurely, NavAccounts, NavTools, NavSettings, AccountsList,
        GeneratedCode, GeneratedQr, QrPrivacyNotice, CameraPreview
    ];
}
