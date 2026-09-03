using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Security.Cryptography;
using System.Windows.Input;
using Avalonia.Threading;
using TOTP.Avalonia.Mobile.Localization;
using TOTP.Core.Models;
using TOTP.Core.Security.Interfaces;
using TOTP.Core.Security.Models;
using TOTP.Core.Services.Interfaces;
using TOTP.Core.Validation;

namespace TOTP.Avalonia.Mobile.Presentation;

public sealed class MobileShellViewModel :
    INotifyPropertyChanged,
    IMobileLifecycleSink,
    IDisposable
{
    private static readonly TimeSpan BackgroundLockGracePeriod = TimeSpan.FromSeconds(30);

    private readonly IAuthorizationService _authorization;
    private readonly IPasswordValidationService _passwordValidation;
    private readonly IAccountManager _accountManager;
    private readonly IAccountTotpService _accountTotp;
    private readonly IAsyncClipboardService _clipboard;
    private readonly ISettingsService _settings;
    private readonly IPlatformApplicationPaths _paths;
    private readonly MobileStringCatalog _strings;
    private readonly TimeProvider _timeProvider;
    private readonly MobileAsyncCommand _initializeCommand;
    private readonly MobileAsyncCommand _configureCommand;
    private readonly MobileAsyncCommand _unlockCommand;
    private readonly MobileAsyncCommand _biometricUnlockCommand;
    private readonly MobileAsyncCommand _beginBiometricEnrollmentCommand;
    private readonly MobileAsyncCommand _enableBiometricCommand;
    private readonly MobileAsyncCommand _cancelBiometricEnrollmentCommand;
    private readonly MobileAsyncCommand _lockCommand;
    private readonly MobileAsyncCommand _beginAddCommand;
    private readonly MobileAsyncCommand _beginEditCommand;
    private readonly MobileAsyncCommand _saveAccountCommand;
    private readonly MobileAsyncCommand _cancelEditCommand;
    private readonly MobileAsyncCommand _beginDeleteCommand;
    private readonly MobileAsyncCommand _confirmDeleteCommand;
    private readonly MobileAsyncCommand _cancelDeleteCommand;
    private readonly MobileAsyncCommand _copyCodeCommand;

    private MobileScreen _screen = MobileScreen.Starting;
    private bool _isBusy;
    private bool _startupFailed;
    private string _notificationText = string.Empty;
    private NotificationSeverity _notificationSeverity = NotificationSeverity.Information;
    private string _setupPassword = string.Empty;
    private string _setupConfirmation = string.Empty;
    private string _unlockPassword = string.Empty;
    private bool _isBiometricAvailable;
    private bool _isBiometricEnabled;
    private bool _isBiometricEnrollmentVisible;
    private string _biometricRecoveryPassword = string.Empty;
    private MobileAccountItem? _selectedAccount;
    private string _selectedCode = string.Empty;
    private int _remainingSeconds;
    private int _periodSeconds = 30;
    private bool _isEditorVisible;
    private bool _isDeleteConfirmationVisible;
    private Guid? _editingAccountId;
    private string _editorIssuer = string.Empty;
    private string _editorAccountName = string.Empty;
    private string _editorSecret = string.Empty;
    private CancellationTokenSource? _codeLifetime;
    private long? _backgroundedAtTimestamp;
    private ITimer? _backgroundLockTimer;
    private bool _automaticBiometricUnlockPending;
    private bool _disposed;

    public MobileShellViewModel(
        IAuthorizationService authorization,
        IPasswordValidationService passwordValidation,
        IAccountManager accountManager,
        IAccountTotpService accountTotp,
        IAsyncClipboardService clipboard,
        ISettingsService settings,
        IPlatformApplicationPaths paths,
        MobileStringCatalog strings,
        TimeProvider timeProvider)
    {
        _authorization = authorization ?? throw new ArgumentNullException(nameof(authorization));
        _passwordValidation = passwordValidation
            ?? throw new ArgumentNullException(nameof(passwordValidation));
        _accountManager = accountManager ?? throw new ArgumentNullException(nameof(accountManager));
        _accountTotp = accountTotp ?? throw new ArgumentNullException(nameof(accountTotp));
        _clipboard = clipboard ?? throw new ArgumentNullException(nameof(clipboard));
        _settings = settings ?? throw new ArgumentNullException(nameof(settings));
        _paths = paths ?? throw new ArgumentNullException(nameof(paths));
        _strings = strings ?? throw new ArgumentNullException(nameof(strings));
        _timeProvider = timeProvider ?? throw new ArgumentNullException(nameof(timeProvider));

        _initializeCommand = new MobileAsyncCommand(InitializeAsync, () => !IsBusy);
        _configureCommand = new MobileAsyncCommand(ConfigureAsync, () => IsSetupVisible && !IsBusy);
        _unlockCommand = new MobileAsyncCommand(
            UnlockAsync,
            () => IsUnlockVisible && !IsBusy && UnlockPassword.Length > 0);
        _biometricUnlockCommand = new MobileAsyncCommand(
            BiometricUnlockAsync,
            () => IsBiometricUnlockVisible && !IsBusy);
        _beginBiometricEnrollmentCommand = new MobileAsyncCommand(
            BeginBiometricEnrollmentAsync,
            () => IsBiometricSetupAvailable && !IsBusy);
        _enableBiometricCommand = new MobileAsyncCommand(
            EnableBiometricAsync,
            () => IsBiometricEnrollmentVisible
                && !IsBusy
                && BiometricRecoveryPassword.Length > 0);
        _cancelBiometricEnrollmentCommand = new MobileAsyncCommand(
            CancelBiometricEnrollmentAsync,
            () => IsBiometricEnrollmentVisible && !IsBusy);
        _lockCommand = new MobileAsyncCommand(LockAsync, () => IsAccountsVisible && !IsBusy);
        _beginAddCommand = new MobileAsyncCommand(BeginAddAsync, CanEditAccounts);
        _beginEditCommand = new MobileAsyncCommand(
            BeginEditAsync,
            () => CanEditAccounts() && SelectedAccount is not null);
        _saveAccountCommand = new MobileAsyncCommand(
            SaveAccountAsync,
            () => IsEditorVisible && !IsBusy);
        _cancelEditCommand = new MobileAsyncCommand(
            CancelEditAsync,
            () => IsEditorVisible && !IsBusy);
        _beginDeleteCommand = new MobileAsyncCommand(
            BeginDeleteAsync,
            () => CanEditAccounts() && SelectedAccount is not null);
        _confirmDeleteCommand = new MobileAsyncCommand(
            ConfirmDeleteAsync,
            () => IsDeleteConfirmationVisible && !IsBusy);
        _cancelDeleteCommand = new MobileAsyncCommand(
            CancelDeleteAsync,
            () => IsDeleteConfirmationVisible && !IsBusy);
        _copyCodeCommand = new MobileAsyncCommand(
            CopyCodeAsync,
            () => IsAccountsVisible && !IsBusy && SelectedCode.Length > 0);
    }

    public event PropertyChangedEventHandler? PropertyChanged;

    public ObservableCollection<MobileAccountItem> Accounts { get; } = [];

    public ICommand InitializeCommand => _initializeCommand;
    public ICommand ConfigureCommand => _configureCommand;
    public ICommand UnlockCommand => _unlockCommand;
    public ICommand BiometricUnlockCommand => _biometricUnlockCommand;
    public ICommand BeginBiometricEnrollmentCommand => _beginBiometricEnrollmentCommand;
    public ICommand EnableBiometricCommand => _enableBiometricCommand;
    public ICommand CancelBiometricEnrollmentCommand => _cancelBiometricEnrollmentCommand;
    public ICommand LockCommand => _lockCommand;
    public ICommand BeginAddCommand => _beginAddCommand;
    public ICommand BeginEditCommand => _beginEditCommand;
    public ICommand SaveAccountCommand => _saveAccountCommand;
    public ICommand CancelEditCommand => _cancelEditCommand;
    public ICommand BeginDeleteCommand => _beginDeleteCommand;
    public ICommand ConfirmDeleteCommand => _confirmDeleteCommand;
    public ICommand CancelDeleteCommand => _cancelDeleteCommand;
    public ICommand CopyCodeCommand => _copyCodeCommand;

    public bool IsStartingVisible => _screen == MobileScreen.Starting;
    public bool IsSetupVisible => _screen == MobileScreen.Setup;
    public bool IsUnlockVisible => _screen == MobileScreen.Unlock;
    public bool IsAccountsVisible => _screen == MobileScreen.Accounts;
    public bool IsAccountListVisible => IsAccountsVisible && !IsEditorVisible;
    public bool HasAccounts => Accounts.Count > 0;
    public bool HasNoAccounts => !HasAccounts;
    public bool HasSelectedAccount => SelectedAccount is not null;
    public bool HasNoSelectedAccount => !HasSelectedAccount;
    public bool CanRetry => _startupFailed && !IsBusy;
    public bool IsBiometricUnlockVisible =>
        IsUnlockVisible && IsBiometricEnabled && IsBiometricAvailable;
    public bool IsBiometricSetupAvailable =>
        IsAccountListVisible && IsBiometricAvailable && !IsBiometricEnabled;
    public bool IsBiometricEnrollmentStartVisible =>
        IsBiometricSetupAvailable && !IsBiometricEnrollmentVisible;

    public bool IsBusy
    {
        get => _isBusy;
        private set
        {
            if (!SetField(ref _isBusy, value)) return;
            OnPropertyChanged(nameof(CanRetry));
            NotifyCommands();
        }
    }

    public string NotificationText
    {
        get => _notificationText;
        private set => SetField(ref _notificationText, value);
    }

    public NotificationSeverity NotificationSeverity
    {
        get => _notificationSeverity;
        private set => SetField(ref _notificationSeverity, value);
    }

    public string SetupPassword
    {
        get => _setupPassword;
        set
        {
            if (!SetField(ref _setupPassword, value ?? string.Empty)) return;
            ClearErrorNotification();
        }
    }

    public string SetupConfirmation
    {
        get => _setupConfirmation;
        set
        {
            if (!SetField(ref _setupConfirmation, value ?? string.Empty)) return;
            ClearErrorNotification();
        }
    }

    public string UnlockPassword
    {
        get => _unlockPassword;
        set
        {
            if (!SetField(ref _unlockPassword, value ?? string.Empty)) return;
            ClearErrorNotification();
            _unlockCommand.NotifyCanExecuteChanged();
        }
    }

    public bool IsBiometricAvailable
    {
        get => _isBiometricAvailable;
        private set
        {
            if (!SetField(ref _isBiometricAvailable, value)) return;
            OnPropertyChanged(nameof(IsBiometricSetupAvailable));
            OnPropertyChanged(nameof(IsBiometricEnrollmentStartVisible));
            NotifyCommands();
        }
    }

    public bool IsBiometricEnabled
    {
        get => _isBiometricEnabled;
        private set
        {
            if (!SetField(ref _isBiometricEnabled, value)) return;
            OnPropertyChanged(nameof(IsBiometricUnlockVisible));
            OnPropertyChanged(nameof(IsBiometricSetupAvailable));
            OnPropertyChanged(nameof(IsBiometricEnrollmentStartVisible));
            NotifyCommands();
        }
    }

    public bool IsBiometricEnrollmentVisible
    {
        get => _isBiometricEnrollmentVisible;
        private set
        {
            if (!SetField(ref _isBiometricEnrollmentVisible, value)) return;
            OnPropertyChanged(nameof(IsBiometricEnrollmentStartVisible));
            NotifyCommands();
        }
    }

    public string BiometricRecoveryPassword
    {
        get => _biometricRecoveryPassword;
        set
        {
            if (!SetField(ref _biometricRecoveryPassword, value ?? string.Empty)) return;
            ClearErrorNotification();
            _enableBiometricCommand.NotifyCanExecuteChanged();
        }
    }

    public MobileAccountItem? SelectedAccount
    {
        get => _selectedAccount;
        set
        {
            if (!SetField(ref _selectedAccount, value)) return;
            OnPropertyChanged(nameof(HasSelectedAccount));
            OnPropertyChanged(nameof(HasNoSelectedAccount));
            OnPropertyChanged(nameof(DeletePrompt));
            NotifyCommands();
            StartCodeRefresh(value);
        }
    }

    public string SelectedCode
    {
        get => _selectedCode;
        private set
        {
            if (!SetField(ref _selectedCode, value)) return;
            _copyCodeCommand.NotifyCanExecuteChanged();
        }
    }

    public int RemainingSeconds
    {
        get => _remainingSeconds;
        private set => SetField(ref _remainingSeconds, value);
    }

    public int PeriodSeconds
    {
        get => _periodSeconds;
        private set => SetField(ref _periodSeconds, value);
    }

    public bool IsEditorVisible
    {
        get => _isEditorVisible;
        private set
        {
            if (!SetField(ref _isEditorVisible, value)) return;
            OnPropertyChanged(nameof(IsAccountListVisible));
            OnPropertyChanged(nameof(EditorTitle));
            OnPropertyChanged(nameof(EditorSecretPlaceholder));
            NotifyCommands();
        }
    }

    public bool IsDeleteConfirmationVisible
    {
        get => _isDeleteConfirmationVisible;
        private set
        {
            if (!SetField(ref _isDeleteConfirmationVisible, value)) return;
            NotifyCommands();
        }
    }

    public string EditorIssuer
    {
        get => _editorIssuer;
        set
        {
            if (!SetField(ref _editorIssuer, value ?? string.Empty)) return;
            ClearErrorNotification();
        }
    }

    public string EditorAccountName
    {
        get => _editorAccountName;
        set
        {
            if (!SetField(ref _editorAccountName, value ?? string.Empty)) return;
            ClearErrorNotification();
        }
    }

    public string EditorSecret
    {
        get => _editorSecret;
        set
        {
            if (!SetField(ref _editorSecret, value ?? string.Empty)) return;
            ClearErrorNotification();
        }
    }

    public string EditorTitle => Get(_editingAccountId.HasValue
        ? MobileStringKeys.EditorEditTitle
        : MobileStringKeys.EditorAddTitle);

    public string EditorSecretPlaceholder => Get(_editingAccountId.HasValue
        ? MobileStringKeys.SecretOptionalOnEdit
        : MobileStringKeys.Secret);

    public string DeletePrompt => string.Format(
        Get(MobileStringKeys.DeleteAccountPrompt),
        SelectedAccount?.DisplayName ?? string.Empty);

    public string StartingText => Get(MobileStringKeys.Starting);
    public string RetryText => Get(MobileStringKeys.Retry);
    public string SetupTitle => Get(MobileStringKeys.SetupTitle);
    public string SetupDescription => Get(MobileStringKeys.SetupDescription);
    public string MasterPasswordText => Get(MobileStringKeys.MasterPassword);
    public string ConfirmPasswordText => Get(MobileStringKeys.ConfirmPassword);
    public string CreateVaultText => Get(MobileStringKeys.CreateVault);
    public string UnlockTitle => Get(MobileStringKeys.UnlockTitle);
    public string UnlockDescription => Get(MobileStringKeys.UnlockDescription);
    public string UnlockText => Get(MobileStringKeys.Unlock);
    public string AccountsTitle => Get(MobileStringKeys.AccountsTitle);
    public string NoAccountsText => Get(MobileStringKeys.NoAccounts);
    public string SelectAccountText => Get(MobileStringKeys.SelectAccount);
    public string AddAccountText => Get(MobileStringKeys.AddAccount);
    public string EditAccountText => Get(MobileStringKeys.EditAccount);
    public string DeleteAccountText => Get(MobileStringKeys.DeleteAccount);
    public string LockText => Get(MobileStringKeys.Lock);
    public string IssuerText => Get(MobileStringKeys.Issuer);
    public string AccountNameText => Get(MobileStringKeys.AccountName);
    public string SaveText => Get(MobileStringKeys.Save);
    public string CancelText => Get(MobileStringKeys.Cancel);
    public string CopyCodeText => Get(MobileStringKeys.CopyCode);
    public string DeleteConfirmTitle => Get(MobileStringKeys.DeleteConfirmTitle);
    public string DeleteText => Get(MobileStringKeys.Delete);
    public string BiometricUnlockText => Get(MobileStringKeys.BiometricUnlock);
    public string BiometricSetupTitle => Get(MobileStringKeys.BiometricSetupTitle);
    public string BiometricSetupDescription => Get(MobileStringKeys.BiometricSetupDescription);
    public string BiometricEnableText => Get(MobileStringKeys.BiometricEnable);
    public string BiometricEnabledText => Get(MobileStringKeys.BiometricEnabled);

    public async Task InitializeAsync()
    {
        if (IsBusy || _disposed) return;

        IsBusy = true;
        _startupFailed = false;
        SetScreen(MobileScreen.Starting);
        SetNotification(Get(MobileStringKeys.Starting), NotificationSeverity.Information);
        try
        {
            var settingsResult = await _settings.LoadAsync();
            if (settingsResult.IsFailed)
            {
                FailStartup();
                return;
            }

            await _authorization.InitializeAsync();
            IsBiometricAvailable = await _authorization.IsHelloAvailableAsync();
            IsBiometricEnabled = _authorization.State.ConfiguredGate
                == AuthorizationGateKind.Hello;
            if (!_authorization.State.IsConfigured
                && File.Exists(_paths.AuthorizationEnvelopeFilePath))
            {
                FailStartup();
                return;
            }

            ClearNotification();
            SetScreen(_authorization.State.IsConfigured
                ? MobileScreen.Unlock
                : MobileScreen.Setup);
        }
        catch (Exception)
        {
            FailStartup();
        }
        finally
        {
            IsBusy = false;
            TryStartAutomaticBiometricUnlock();
        }
    }

    public async Task ConfigureAsync()
    {
        if (!IsSetupVisible || IsBusy) return;

        var password = SetupPassword;
        var confirmation = SetupConfirmation;
        SetupPassword = string.Empty;
        SetupConfirmation = string.Empty;

        if (string.IsNullOrWhiteSpace(password)
            || string.IsNullOrWhiteSpace(confirmation))
        {
            SetError(MobileStringKeys.PasswordRequired);
            return;
        }

        if (password.Length < _passwordValidation.MinimumLength)
        {
            SetNotification(
                string.Format(
                    Get(MobileStringKeys.PasswordMinimumLength),
                    _passwordValidation.MinimumLength),
                NotificationSeverity.Error);
            return;
        }

        if (!string.Equals(password, confirmation, StringComparison.Ordinal))
        {
            SetError(MobileStringKeys.PasswordMismatch);
            return;
        }

        IsBusy = true;
        try
        {
            var result = await _authorization.ConfigurePasswordAsync(password, confirmation);
            if (result != AuthorizationResult.Success)
            {
                SetError(result == AuthorizationResult.ExistingVaultConflict
                    ? MobileStringKeys.ExistingVaultConflict
                    : MobileStringKeys.SetupFailed);
                return;
            }

            SetScreen(MobileScreen.Accounts);
            await LoadAccountsAsync();
        }
        catch (Exception)
        {
            SetError(MobileStringKeys.SetupFailed);
        }
        finally
        {
            password = string.Empty;
            confirmation = string.Empty;
            IsBusy = false;
        }
    }

    public async Task UnlockAsync()
    {
        if (!IsUnlockVisible || IsBusy || UnlockPassword.Length == 0) return;

        var password = UnlockPassword;
        UnlockPassword = string.Empty;
        IsBusy = true;
        try
        {
            var result = await _authorization.TryUnlockWithPasswordAsync(password);
            if (result != AuthorizationResult.Success)
            {
                SetError(result == AuthorizationResult.InvalidCredentials
                    ? MobileStringKeys.UnlockRejected
                    : MobileStringKeys.UnlockFailed);
                return;
            }

            SetScreen(MobileScreen.Accounts);
            await LoadAccountsAsync();
        }
        catch (Exception)
        {
            SetError(MobileStringKeys.UnlockFailed);
        }
        finally
        {
            password = string.Empty;
            IsBusy = false;
        }
    }

    public async Task BiometricUnlockAsync()
    {
        if (!IsBiometricUnlockVisible || IsBusy) return;

        IsBusy = true;
        ClearNotification();
        try
        {
            var result = await _authorization.TryUnlockWithHelloAsync();
            if (result == AuthorizationResult.Success)
            {
                ClearNotification();
                SetScreen(MobileScreen.Accounts);
                await LoadAccountsAsync();
                return;
            }

            if (result == AuthorizationResult.Cancelled) return;
            SetError(result switch
            {
                AuthorizationResult.PasswordRequired =>
                    MobileStringKeys.BiometricRecoveryRequired,
                AuthorizationResult.TooManyAttempts =>
                    MobileStringKeys.BiometricRetriesExhausted,
                AuthorizationResult.DisabledByPolicy =>
                    MobileStringKeys.BiometricDisabledByPolicy,
                _ => MobileStringKeys.BiometricUnlockFailed
            });
        }
        catch (Exception)
        {
            SetError(MobileStringKeys.BiometricUnlockFailed);
        }
        finally
        {
            IsBusy = false;
        }
    }

    public Task BeginBiometricEnrollmentAsync()
    {
        if (!IsBiometricSetupAvailable || IsBusy) return Task.CompletedTask;
        BiometricRecoveryPassword = string.Empty;
        IsBiometricEnrollmentVisible = true;
        ClearNotification();
        return Task.CompletedTask;
    }

    public async Task EnableBiometricAsync()
    {
        if (!IsBiometricEnrollmentVisible
            || IsBusy
            || BiometricRecoveryPassword.Length == 0)
        {
            return;
        }

        var recoveryPassword = BiometricRecoveryPassword;
        BiometricRecoveryPassword = string.Empty;
        IsBusy = true;
        try
        {
            var result = await _authorization.ConfigureHelloAsync(recoveryPassword);
            if (result == AuthorizationResult.Cancelled) return;
            if (result != AuthorizationResult.Success)
            {
                SetError(result switch
                {
                    AuthorizationResult.InvalidCredentials => MobileStringKeys.UnlockRejected,
                    AuthorizationResult.DisabledByPolicy =>
                        MobileStringKeys.BiometricDisabledByPolicy,
                    AuthorizationResult.TooManyAttempts =>
                        MobileStringKeys.BiometricRetriesExhausted,
                    _ => MobileStringKeys.BiometricEnableFailed
                });
                return;
            }

            IsBiometricEnabled = true;
            IsBiometricEnrollmentVisible = false;
            BiometricRecoveryPassword = string.Empty;
            SetSuccess(MobileStringKeys.BiometricEnabled);
        }
        catch (Exception)
        {
            SetError(MobileStringKeys.BiometricEnableFailed);
        }
        finally
        {
            recoveryPassword = string.Empty;
            IsBusy = false;
        }
    }

    public Task CancelBiometricEnrollmentAsync()
    {
        if (IsBusy) return Task.CompletedTask;
        BiometricRecoveryPassword = string.Empty;
        IsBiometricEnrollmentVisible = false;
        ClearNotification();
        return Task.CompletedTask;
    }

    public Task LockAsync()
    {
        LockCore();
        return Task.CompletedTask;
    }

    public Task BeginAddAsync()
    {
        if (!CanEditAccounts()) return Task.CompletedTask;
        ClearEditor();
        IsDeleteConfirmationVisible = false;
        IsEditorVisible = true;
        ClearNotification();
        return Task.CompletedTask;
    }

    public Task BeginEditAsync()
    {
        if (!CanEditAccounts() || SelectedAccount is null) return Task.CompletedTask;
        _editingAccountId = SelectedAccount.Id;
        EditorIssuer = SelectedAccount.Issuer;
        EditorAccountName = SelectedAccount.AccountName;
        EditorSecret = string.Empty;
        IsDeleteConfirmationVisible = false;
        IsEditorVisible = true;
        ClearNotification();
        return Task.CompletedTask;
    }

    public async Task SaveAccountAsync()
    {
        if (!IsEditorVisible || IsBusy) return;

        var issuer = EditorIssuer.Trim();
        var accountName = EditorAccountName.Trim();
        var enteredSecret = EditorSecret;
        EditorSecret = string.Empty;
        if (issuer.Length == 0)
        {
            SetError(MobileStringKeys.IssuerRequired);
            return;
        }

        IsBusy = true;
        string? secret = null;
        try
        {
            var loaded = await _accountManager.GetAllOtpEntriesSortedAsync();
            if (loaded.IsFailed)
            {
                SetError(MobileStringKeys.AccountSaveFailed);
                return;
            }

            var existing = _editingAccountId.HasValue
                ? loaded.Value.FirstOrDefault(account => account.ID == _editingAccountId.Value)
                : null;
            if (_editingAccountId.HasValue && existing is null)
            {
                SetError(MobileStringKeys.AccountSaveFailed);
                return;
            }

            secret = enteredSecret.Length > 0
                ? SecretValidation.NormalizeBase32Secret(enteredSecret)
                : existing?.Secret;
            if (string.IsNullOrWhiteSpace(secret))
            {
                SetError(MobileStringKeys.SecretRequired);
                return;
            }

            if (!SecretValidation.IsValidBase32Secret(secret))
            {
                SetError(MobileStringKeys.SecretInvalid);
                return;
            }

            if (loaded.Value.Any(account =>
                    account.ID != _editingAccountId
                    && string.Equals(account.Issuer.Trim(), issuer, StringComparison.OrdinalIgnoreCase)
                    && string.Equals(
                        (account.AccountName ?? string.Empty).Trim(),
                        accountName,
                        StringComparison.OrdinalIgnoreCase)))
            {
                SetError(MobileStringKeys.DuplicateAccount);
                return;
            }

            var updated = new Account(
                _editingAccountId ?? Guid.NewGuid(),
                issuer,
                secret,
                accountName.Length == 0 ? null : accountName);
            var saved = existing is null
                ? await _accountManager.AddNewAsync(updated)
                : await _accountManager.UpdateAsync(existing, updated);
            if (saved.IsFailed)
            {
                SetError(MobileStringKeys.AccountSaveFailed);
                return;
            }

            var savedId = updated.ID;
            ClearEditor();
            IsEditorVisible = false;
            await LoadAccountsAsync(savedId);
            SetSuccess(MobileStringKeys.AccountSaved);
        }
        catch (Exception)
        {
            SetError(MobileStringKeys.AccountSaveFailed);
        }
        finally
        {
            enteredSecret = string.Empty;
            secret = null;
            IsBusy = false;
        }
    }

    public Task CancelEditAsync()
    {
        if (IsBusy) return Task.CompletedTask;
        ClearEditor();
        IsEditorVisible = false;
        ClearNotification();
        return Task.CompletedTask;
    }

    public Task BeginDeleteAsync()
    {
        if (!CanEditAccounts() || SelectedAccount is null) return Task.CompletedTask;
        IsDeleteConfirmationVisible = true;
        OnPropertyChanged(nameof(DeletePrompt));
        ClearNotification();
        return Task.CompletedTask;
    }

    public async Task ConfirmDeleteAsync()
    {
        if (!IsDeleteConfirmationVisible || SelectedAccount is null || IsBusy) return;

        var accountId = SelectedAccount.Id;
        IsBusy = true;
        try
        {
            var loaded = await _accountManager.GetAllOtpEntriesSortedAsync();
            var account = loaded.IsSuccess
                ? loaded.Value.FirstOrDefault(value => value.ID == accountId)
                : null;
            if (account is null || (await _accountManager.DeleteAsync(account)).IsFailed)
            {
                SetError(MobileStringKeys.AccountDeleteFailed);
                return;
            }

            IsDeleteConfirmationVisible = false;
            await LoadAccountsAsync();
            SetSuccess(MobileStringKeys.AccountDeleted);
        }
        catch (Exception)
        {
            SetError(MobileStringKeys.AccountDeleteFailed);
        }
        finally
        {
            IsBusy = false;
        }
    }

    public Task CancelDeleteAsync()
    {
        if (!IsBusy) IsDeleteConfirmationVisible = false;
        return Task.CompletedTask;
    }

    public async Task CopyCodeAsync()
    {
        if (SelectedCode.Length == 0 || IsBusy) return;

        var code = SelectedCode;
        try
        {
            var seconds = Math.Max(1, _settings.Current.ClearClipboardSeconds);
            var result = _settings.Current.ClearClipboardEnabled
                ? await _clipboard.CopyAndScheduleClearAsync(code, TimeSpan.FromSeconds(seconds))
                : await _clipboard.CopyAsync(code);
            if (result.IsFailed)
            {
                SetError(MobileStringKeys.CodeCopyFailed);
                return;
            }

            if (_settings.Current.ClearClipboardEnabled)
            {
                SetNotification(
                    string.Format(Get(MobileStringKeys.CodeCopiedWithClear), seconds),
                    NotificationSeverity.Success);
            }
            else
            {
                SetSuccess(MobileStringKeys.CodeCopied);
            }
        }
        catch (Exception)
        {
            SetError(MobileStringKeys.CodeCopyFailed);
        }
        finally
        {
            code = string.Empty;
        }
    }

    public void OnEnteredBackground(bool lockImmediately)
    {
        if (!_authorization.State.IsUnlocked || _disposed) return;

        CancelCodeRefresh();
        if (lockImmediately)
        {
            LockCore();
            return;
        }

        CancelBackgroundLockTimer();
        _backgroundedAtTimestamp = _timeProvider.GetTimestamp();
        _backgroundLockTimer = _timeProvider.CreateTimer(
            static state => ((MobileShellViewModel)state!).PostBackgroundLockCheck(),
            this,
            BackgroundLockGracePeriod,
            Timeout.InfiniteTimeSpan);
    }

    public void OnReturnedToForeground()
    {
        if (_disposed) return;
        if (!_backgroundedAtTimestamp.HasValue)
        {
            RequestAutomaticBiometricUnlock();
            return;
        }

        var elapsed = _timeProvider.GetElapsedTime(_backgroundedAtTimestamp.Value);
        _backgroundedAtTimestamp = null;
        CancelBackgroundLockTimer();
        if (elapsed >= BackgroundLockGracePeriod)
        {
            LockCore();
            RequestAutomaticBiometricUnlock();
            return;
        }

        if (_authorization.State.IsUnlocked)
            StartCodeRefresh(SelectedAccount);
    }

    public void Dispose()
    {
        if (_disposed) return;
        _disposed = true;
        CancelBackgroundLockTimer();
        CancelCodeRefresh();
        ClearEditor();
        Accounts.Clear();
        _authorization.Lock();
    }

    private async Task LoadAccountsAsync(Guid? selectedId = null)
    {
        var loaded = await _accountManager.GetAllOtpEntriesSortedAsync();
        if (loaded.IsFailed)
        {
            SetError(MobileStringKeys.LoadingAccountsFailed);
            return;
        }

        Accounts.Clear();
        foreach (var account in loaded.Value)
        {
            Accounts.Add(new MobileAccountItem(
                account.ID,
                account.Issuer,
                account.AccountName ?? string.Empty));
        }

        OnPropertyChanged(nameof(HasAccounts));
        OnPropertyChanged(nameof(HasNoAccounts));
        SelectedAccount = selectedId.HasValue
            ? Accounts.FirstOrDefault(account => account.Id == selectedId.Value)
            : Accounts.FirstOrDefault();
        NotifyCommands();
    }

    private void StartCodeRefresh(MobileAccountItem? account)
    {
        CancelCodeRefresh();
        SelectedCode = string.Empty;
        RemainingSeconds = 0;
        PeriodSeconds = 30;
        if (account is null || !IsAccountsVisible) return;

        var lifetime = new CancellationTokenSource();
        _codeLifetime = lifetime;
        _ = RunCodeRefreshAsync(account.Id, lifetime);
    }

    private async Task RunCodeRefreshAsync(Guid accountId, CancellationTokenSource lifetime)
    {
        try
        {
            while (!lifetime.IsCancellationRequested)
            {
                var generated = await _accountTotp.GenerateAsync(accountId);
                if (generated.IsFailed
                    || SelectedAccount?.Id != accountId
                    || lifetime.IsCancellationRequested)
                {
                    SelectedCode = string.Empty;
                    RemainingSeconds = 0;
                    SetError(MobileStringKeys.CodeUnavailable);
                    return;
                }

                SelectedCode = generated.Value.Code;
                RemainingSeconds = Math.Max(1, generated.Value.RemainingSeconds);
                PeriodSeconds = Math.Max(RemainingSeconds, generated.Value.PeriodSeconds);
                while (RemainingSeconds > 1 && !lifetime.IsCancellationRequested)
                {
                    await Task.Delay(TimeSpan.FromSeconds(1), lifetime.Token);
                    RemainingSeconds--;
                }

                if (!lifetime.IsCancellationRequested)
                    await Task.Delay(TimeSpan.FromSeconds(1), lifetime.Token);
            }
        }
        catch (OperationCanceledException) when (lifetime.IsCancellationRequested)
        {
        }
        catch (Exception)
        {
            SelectedCode = string.Empty;
            RemainingSeconds = 0;
            SetError(MobileStringKeys.CodeUnavailable);
        }
        finally
        {
            if (ReferenceEquals(_codeLifetime, lifetime)) _codeLifetime = null;
            lifetime.Dispose();
        }
    }

    private void LockCore()
    {
        if (_disposed) return;
        _backgroundedAtTimestamp = null;
        CancelBackgroundLockTimer();
        _authorization.Lock();
        CancelCodeRefresh();
        ClearEditor();
        IsEditorVisible = false;
        IsDeleteConfirmationVisible = false;
        SelectedAccount = null;
        Accounts.Clear();
        OnPropertyChanged(nameof(HasAccounts));
        OnPropertyChanged(nameof(HasNoAccounts));
        UnlockPassword = string.Empty;
        BiometricRecoveryPassword = string.Empty;
        IsBiometricEnrollmentVisible = false;
        ClearNotification();
        SetScreen(_authorization.State.IsConfigured
            ? MobileScreen.Unlock
            : MobileScreen.Setup);
    }

    private void RequestAutomaticBiometricUnlock()
    {
        _automaticBiometricUnlockPending = true;
        TryStartAutomaticBiometricUnlock();
    }

    private void TryStartAutomaticBiometricUnlock()
    {
        if (!_automaticBiometricUnlockPending
            || !IsBiometricUnlockVisible
            || IsBusy
            || _disposed)
        {
            return;
        }

        _automaticBiometricUnlockPending = false;
        _ = BiometricUnlockAsync();
    }

    private void CancelCodeRefresh()
    {
        var lifetime = _codeLifetime;
        _codeLifetime = null;
        lifetime?.Cancel();
        SelectedCode = string.Empty;
        RemainingSeconds = 0;
    }

    private void PostBackgroundLockCheck()
    {
        Dispatcher.UIThread.Post(() =>
        {
            if (_disposed || !_backgroundedAtTimestamp.HasValue) return;
            if (_timeProvider.GetElapsedTime(_backgroundedAtTimestamp.Value)
                >= BackgroundLockGracePeriod)
            {
                LockCore();
            }
        });
    }

    private void CancelBackgroundLockTimer()
    {
        _backgroundLockTimer?.Dispose();
        _backgroundLockTimer = null;
    }

    private void ClearEditor()
    {
        _editingAccountId = null;
        EditorIssuer = string.Empty;
        EditorAccountName = string.Empty;
        EditorSecret = string.Empty;
        OnPropertyChanged(nameof(EditorTitle));
        OnPropertyChanged(nameof(EditorSecretPlaceholder));
    }

    private bool CanEditAccounts() =>
        IsAccountsVisible && !IsBusy && !IsEditorVisible && !IsDeleteConfirmationVisible;

    private void FailStartup()
    {
        _startupFailed = true;
        SetScreen(MobileScreen.Starting);
        SetError(MobileStringKeys.StartupFailed);
        OnPropertyChanged(nameof(CanRetry));
    }

    private void SetScreen(MobileScreen screen)
    {
        if (_screen == screen) return;
        _screen = screen;
        OnPropertyChanged(nameof(IsStartingVisible));
        OnPropertyChanged(nameof(IsSetupVisible));
        OnPropertyChanged(nameof(IsUnlockVisible));
        OnPropertyChanged(nameof(IsAccountsVisible));
        OnPropertyChanged(nameof(IsAccountListVisible));
        OnPropertyChanged(nameof(IsBiometricUnlockVisible));
        OnPropertyChanged(nameof(IsBiometricSetupAvailable));
        OnPropertyChanged(nameof(IsBiometricEnrollmentStartVisible));
        NotifyCommands();
    }

    private void SetError(string key) =>
        SetNotification(Get(key), NotificationSeverity.Error);

    private void SetSuccess(string key) =>
        SetNotification(Get(key), NotificationSeverity.Success);

    private void SetNotification(string text, NotificationSeverity severity)
    {
        NotificationSeverity = severity;
        NotificationText = text;
    }

    private void ClearNotification()
    {
        NotificationText = string.Empty;
        NotificationSeverity = NotificationSeverity.Information;
    }

    private void ClearErrorNotification()
    {
        if (NotificationSeverity == NotificationSeverity.Error) ClearNotification();
    }

    private string Get(string key) => _strings.Get(key);

    private void NotifyCommands()
    {
        _initializeCommand.NotifyCanExecuteChanged();
        _configureCommand.NotifyCanExecuteChanged();
        _unlockCommand.NotifyCanExecuteChanged();
        _biometricUnlockCommand.NotifyCanExecuteChanged();
        _beginBiometricEnrollmentCommand.NotifyCanExecuteChanged();
        _enableBiometricCommand.NotifyCanExecuteChanged();
        _cancelBiometricEnrollmentCommand.NotifyCanExecuteChanged();
        _lockCommand.NotifyCanExecuteChanged();
        _beginAddCommand.NotifyCanExecuteChanged();
        _beginEditCommand.NotifyCanExecuteChanged();
        _saveAccountCommand.NotifyCanExecuteChanged();
        _cancelEditCommand.NotifyCanExecuteChanged();
        _beginDeleteCommand.NotifyCanExecuteChanged();
        _confirmDeleteCommand.NotifyCanExecuteChanged();
        _cancelDeleteCommand.NotifyCanExecuteChanged();
        _copyCodeCommand.NotifyCanExecuteChanged();
    }

    private bool SetField<T>(
        ref T field,
        T value,
        [CallerMemberName] string? propertyName = null)
    {
        if (EqualityComparer<T>.Default.Equals(field, value)) return false;
        field = value;
        OnPropertyChanged(propertyName);
        return true;
    }

    private void OnPropertyChanged([CallerMemberName] string? propertyName = null) =>
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));

    private enum MobileScreen
    {
        Starting,
        Setup,
        Unlock,
        Accounts
    }
}
