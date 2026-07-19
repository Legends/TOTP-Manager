using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows.Input;
using TOTP.Core.Security.Interfaces;
using TOTP.Core.Services.Interfaces;
using TOTP.Avalonia.Desktop.Startup;
using TOTP.Avalonia.Desktop.Localization;
using TOTP.Infrastructure.Services;

namespace TOTP.Avalonia.Desktop.Presentation;

public sealed class MainWindowViewModel : INotifyPropertyChanged, IDisposable
{
    private readonly IAvaloniaStartupCoordinator _startupCoordinator;
    private readonly IAuthorizationService _authorizationService;
    private readonly IAvaloniaLocalizationService _localization;
    private readonly ISettingsService? _settingsService;
    private readonly SessionLockPolicyBackgroundService? _sessionLockPolicy;
    private readonly IUiScheduler? _uiScheduler;
    private readonly AsyncCommand _initializeCommand;
    private readonly AsyncCommand _lockCommand;
    private readonly AsyncCommand _showAccountsCommand;
    private readonly AsyncCommand _showToolsCommand;
    private readonly AsyncCommand _showSettingsCommand;
    private readonly CancellationTokenSource _lifetime = new();
    private string _statusText = "Starting TOTP Manager…";
    private NotificationSeverity _statusSeverity = NotificationSeverity.Information;
    private bool _isBusy;
    private bool _canRetry;
    private bool _isPasswordUnlockVisible;
    private bool _isPasswordSetupVisible;
    private bool _isShellVisible;
    private bool _isAccountListVisible;
    private bool _isToolsVisible;
    private bool _isSettingsVisible;
    private bool _shutdownPrepared;
    private bool _disposed;

    public MainWindowViewModel(
        IAvaloniaStartupCoordinator startupCoordinator,
        IAuthorizationService authorizationService,
        PasswordUnlockViewModel passwordUnlock,
        PasswordSetupViewModel passwordSetup,
        AccountListViewModel accountList,
        SettingsPageViewModel settingsPage,
        AuthorizationSettingsViewModel authorizationSettings,
        NativeFilePickerViewModel nativeFilePicker,
        CameraScannerViewModel cameraScanner,
        UpdateCheckViewModel updateCheck,
        DiagnosticsViewModel diagnostics,
        IAvaloniaLocalizationService localization,
        ISettingsService? settingsService = null,
        SessionLockPolicyBackgroundService? sessionLockPolicy = null,
        IUiScheduler? uiScheduler = null)
    {
        _startupCoordinator = startupCoordinator ?? throw new ArgumentNullException(nameof(startupCoordinator));
        _authorizationService = authorizationService ?? throw new ArgumentNullException(nameof(authorizationService));
        PasswordUnlock = passwordUnlock ?? throw new ArgumentNullException(nameof(passwordUnlock));
        PasswordSetup = passwordSetup ?? throw new ArgumentNullException(nameof(passwordSetup));
        AccountList = accountList ?? throw new ArgumentNullException(nameof(accountList));
        AccountList.EnableAutomaticCodeGenerationOnSelection();
        SettingsPage = settingsPage ?? throw new ArgumentNullException(nameof(settingsPage));
        AuthorizationSettings = authorizationSettings ?? throw new ArgumentNullException(nameof(authorizationSettings));
        NativeFilePicker = nativeFilePicker ?? throw new ArgumentNullException(nameof(nativeFilePicker));
        CameraScanner = cameraScanner ?? throw new ArgumentNullException(nameof(cameraScanner));
        UpdateCheck = updateCheck ?? throw new ArgumentNullException(nameof(updateCheck));
        Diagnostics = diagnostics ?? throw new ArgumentNullException(nameof(diagnostics));
        _localization = localization ?? throw new ArgumentNullException(nameof(localization));
        _settingsService = settingsService;
        _sessionLockPolicy = sessionLockPolicy;
        _uiScheduler = uiScheduler;
        if (_sessionLockPolicy is not null)
            _sessionLockPolicy.ApplicationLocked += OnPlatformSessionLocked;
        PasswordUnlock.Unlocked += OnUnlocked;
        PasswordSetup.Configured += OnConfigured;
        CameraScanner.AccountImported += OnAccountImported;
        NativeFilePicker.AccountsChanged += OnAccountImported;
        SettingsPage.SettingsSaved += OnSettingsSaved;
        _initializeCommand = new AsyncCommand(InitializeAsync, () => !_isBusy);
        _lockCommand = new AsyncCommand(LockAsync, () => _isShellVisible);
        _showAccountsCommand = new AsyncCommand(
            ShowAccountsAsync,
            () => _isShellVisible && !_isAccountListVisible);
        _showToolsCommand = new AsyncCommand(
            ShowToolsAsync,
            () => _isShellVisible && !_isToolsVisible);
        _showSettingsCommand = new AsyncCommand(
            ShowSettingsAsync,
            () => _isShellVisible && !_isSettingsVisible);
    }

    public event PropertyChangedEventHandler? PropertyChanged;

    public string StatusText
    {
        get => _statusText;
        private set => SetField(ref _statusText, value);
    }

    public NotificationSeverity StatusSeverity
    {
        get => _statusSeverity;
        private set => SetField(ref _statusSeverity, value);
    }

    public bool IsBusy
    {
        get => _isBusy;
        private set
        {
            if (!SetField(ref _isBusy, value)) return;
            _initializeCommand.NotifyCanExecuteChanged();
        }
    }

    public bool CanRetry
    {
        get => _canRetry;
        private set => SetField(ref _canRetry, value);
    }

    public ICommand InitializeCommand => _initializeCommand;

    public ICommand LockCommand => _lockCommand;

    public ICommand ShowAccountsCommand => _showAccountsCommand;

    public ICommand ShowToolsCommand => _showToolsCommand;

    public ICommand ShowSettingsCommand => _showSettingsCommand;

    public PasswordUnlockViewModel PasswordUnlock { get; }

    public PasswordSetupViewModel PasswordSetup { get; }

    public AccountListViewModel AccountList { get; }

    public SettingsPageViewModel SettingsPage { get; }

    public AuthorizationSettingsViewModel AuthorizationSettings { get; }

    public NativeFilePickerViewModel NativeFilePicker { get; }

    public CameraScannerViewModel CameraScanner { get; }

    public UpdateCheckViewModel UpdateCheck { get; }

    public DiagnosticsViewModel Diagnostics { get; }

    public bool IsPasswordUnlockVisible
    {
        get => _isPasswordUnlockVisible;
        private set => SetField(ref _isPasswordUnlockVisible, value);
    }

    public bool IsPasswordSetupVisible
    {
        get => _isPasswordSetupVisible;
        private set => SetField(ref _isPasswordSetupVisible, value);
    }

    public bool IsShellVisible
    {
        get => _isShellVisible;
        private set
        {
            if (!SetField(ref _isShellVisible, value)) return;
            NotifyShellCommands();
        }
    }

    public bool IsAccountListVisible
    {
        get => _isAccountListVisible;
        private set
        {
            if (!SetField(ref _isAccountListVisible, value)) return;
            _showAccountsCommand.NotifyCanExecuteChanged();
        }
    }

    public bool IsToolsVisible
    {
        get => _isToolsVisible;
        private set
        {
            if (!SetField(ref _isToolsVisible, value)) return;
            _showToolsCommand.NotifyCanExecuteChanged();
        }
    }

    public bool IsSettingsVisible
    {
        get => _isSettingsVisible;
        private set
        {
            if (!SetField(ref _isSettingsVisible, value)) return;
            _showSettingsCommand.NotifyCanExecuteChanged();
        }
    }

    public async Task InitializeAsync()
    {
        if (IsBusy) return;

        IsBusy = true;
        CanRetry = false;
        IsPasswordUnlockVisible = false;
        IsPasswordSetupVisible = false;
        IsShellVisible = false;
        IsAccountListVisible = false;
        IsToolsVisible = false;
        IsSettingsVisible = false;
        StatusText = "Starting TOTP Manager…";
        StatusSeverity = NotificationSeverity.Information;

        try
        {
            var outcome = await _startupCoordinator.InitializeAsync(_lifetime.Token);
            (StatusText, CanRetry, StatusSeverity) = outcome switch
            {
                AvaloniaStartupOutcome.ReadyForPasswordSetup =>
                    ("Create a master password to protect your authenticator.", false, NotificationSeverity.Information),
                AvaloniaStartupOutcome.ReadyForUnlock =>
                    ("Enter your master password to unlock your authenticator.", false, NotificationSeverity.Information),
                AvaloniaStartupOutcome.ReadyForPasswordFallback =>
                    (_localization.GetString(AvaloniaStringKeys.QuickUnlockFallback), false, NotificationSeverity.Warning),
                AvaloniaStartupOutcome.ReadyUnlocked =>
                    (_localization.GetString(AvaloniaStringKeys.VaultUnlocked), false, NotificationSeverity.Success),
                AvaloniaStartupOutcome.PreferencesUnavailable =>
                    ("Your preferences could not be loaded. Your encrypted data was not changed.", true, NotificationSeverity.Warning),
                _ =>
                    ("TOTP Manager could not start safely. Your encrypted data was not changed.", true, NotificationSeverity.Error)
            };
            IsPasswordUnlockVisible = outcome is AvaloniaStartupOutcome.ReadyForUnlock
                or AvaloniaStartupOutcome.ReadyForPasswordFallback;
            IsPasswordSetupVisible = outcome == AvaloniaStartupOutcome.ReadyForPasswordSetup;
            if (outcome == AvaloniaStartupOutcome.ReadyUnlocked)
                EnterAuthorizedShell();
        }
        catch (OperationCanceledException) when (_lifetime.IsCancellationRequested)
        {
            StatusText = "Startup cancelled.";
            StatusSeverity = NotificationSeverity.Information;
        }
        catch (Exception)
        {
            StatusText = "TOTP Manager could not start safely. Your encrypted data was not changed.";
            StatusSeverity = NotificationSeverity.Error;
            CanRetry = true;
        }
        finally
        {
            IsBusy = false;
        }
    }

    public void Dispose()
    {
        if (_disposed) return;
        _disposed = true;

        PrepareForShutdown();
        PasswordUnlock.Unlocked -= OnUnlocked;
        PasswordSetup.Configured -= OnConfigured;
        CameraScanner.AccountImported -= OnAccountImported;
        NativeFilePicker.AccountsChanged -= OnAccountImported;
        SettingsPage.SettingsSaved -= OnSettingsSaved;
        if (_sessionLockPolicy is not null)
            _sessionLockPolicy.ApplicationLocked -= OnPlatformSessionLocked;
        _lifetime.Cancel();
        _lifetime.Dispose();
        CameraScanner.Dispose();
        UpdateCheck.Dispose();
    }

    public void PrepareForShutdown()
    {
        if (_shutdownPrepared) return;
        _shutdownPrepared = true;

        _authorizationService.Lock();
        AccountList.Clear();
        PasswordSetup.Clear();
        AuthorizationSettings.ClearSensitiveInputs();
        CameraScanner.Clear();
        AuthorizationSettings.ClearSensitiveInputs();
        IsSettingsVisible = false;
        IsToolsVisible = false;
        IsShellVisible = false;
        IsAccountListVisible = false;
        IsPasswordUnlockVisible = false;
        IsPasswordSetupVisible = false;
        StatusText = "TOTP Manager is closing safely.";
        StatusSeverity = NotificationSeverity.Information;
    }

    private void OnUnlocked(object? sender, EventArgs e)
    {
        IsPasswordUnlockVisible = false;
        EnterAuthorizedShell();
        StatusText = _localization.GetString(AvaloniaStringKeys.VaultUnlocked);
        StatusSeverity = NotificationSeverity.Success;
    }

    private void OnConfigured(object? sender, EventArgs e)
    {
        IsPasswordSetupVisible = false;
        IsPasswordUnlockVisible = false;
        EnterAuthorizedShell();
        StatusText = _localization.GetString(AvaloniaStringKeys.VaultConfigured);
        StatusSeverity = NotificationSeverity.Success;
    }

    private void OnAccountImported(object? sender, EventArgs e) =>
        AccountList.LoadCommand.Execute(null);

    private void OnSettingsSaved(object? sender, EventArgs e) =>
        AccountList.NotifySettingsChanged();

    public Task LockAsync()
    {
        _authorizationService.Lock();
        ApplyLockedUiState();
        return Task.CompletedTask;
    }

    private void ApplyLockedUiState()
    {
        AccountList.Clear();
        CameraScanner.Clear();
        IsSettingsVisible = false;
        IsToolsVisible = false;
        IsShellVisible = false;
        IsAccountListVisible = false;
        IsPasswordUnlockVisible = true;
        IsPasswordSetupVisible = false;
        StatusText = "Vault locked. Enter your master password to continue.";
        StatusSeverity = NotificationSeverity.Information;
    }

    public Task HandleWindowMinimizedAsync()
    {
        if (!IsShellVisible || _settingsService?.Current.LockOnMinimize != true)
            return Task.CompletedTask;
        return LockAsync();
    }

    private void OnPlatformSessionLocked(object? sender, EventArgs args)
    {
        if (_uiScheduler is null)
        {
            ApplyLockedUiState();
            return;
        }

        _uiScheduler.Post(ApplyLockedUiState);
    }

    public Task ShowAccountsAsync()
    {
        if (IsShellVisible) SetActivePage(ShellPage.Accounts);
        return Task.CompletedTask;
    }

    public Task ShowToolsAsync()
    {
        if (IsShellVisible) SetActivePage(ShellPage.Tools);
        return Task.CompletedTask;
    }

    public async Task ShowSettingsAsync()
    {
        if (!IsShellVisible) return;
        SetActivePage(ShellPage.Settings);
        await AuthorizationSettings.RefreshAsync();
    }

    private void SetActivePage(ShellPage page)
    {
        if (IsAccountListVisible && page != ShellPage.Accounts)
            AccountList.ClearSensitiveOutput();
        if (IsToolsVisible && page != ShellPage.Tools)
            CameraScanner.Clear();
        if (IsSettingsVisible && page != ShellPage.Settings)
            AuthorizationSettings.ClearSensitiveInputs();

        IsAccountListVisible = page == ShellPage.Accounts;
        IsToolsVisible = page == ShellPage.Tools;
        IsSettingsVisible = page == ShellPage.Settings;
        if (IsSettingsVisible) SettingsPage.Reload();
        if (IsAccountListVisible && AccountList.SelectedAccount is not null)
            AccountList.GenerateCommand.Execute(null);
    }

    private void EnterAuthorizedShell()
    {
        IsShellVisible = true;
        SetActivePage(ShellPage.Accounts);
        AccountList.LoadCommand.Execute(null);
    }

    private void NotifyShellCommands()
    {
        _lockCommand.NotifyCanExecuteChanged();
        _showAccountsCommand.NotifyCanExecuteChanged();
        _showToolsCommand.NotifyCanExecuteChanged();
        _showSettingsCommand.NotifyCanExecuteChanged();
    }

    private bool SetField<T>(ref T field, T value, [CallerMemberName] string? propertyName = null)
    {
        if (EqualityComparer<T>.Default.Equals(field, value)) return false;
        field = value;
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        return true;
    }
}
