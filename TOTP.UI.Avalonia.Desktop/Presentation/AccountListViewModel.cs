using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows.Input;
using Avalonia.Media;
using TOTP.Avalonia.Desktop.Platform;
using TOTP.Avalonia.Desktop.Localization;
using TOTP.Avalonia.Desktop.Presentation.Dialogs;
using TOTP.Core.Models;
using TOTP.Core.Security.Interfaces;
using TOTP.Core.Services.Interfaces;
using TOTP.Core.Validation;

namespace TOTP.Avalonia.Desktop.Presentation;

public sealed class AccountListViewModel : INotifyPropertyChanged, IDisposable
{
    private readonly IAccountManager _accountManager;
    private readonly IAccountTotpService _accountTotpService;
    private readonly IAsyncClipboardService _clipboardService;
    private readonly IAccountQrCodeService _accountQrCodeService;
    private readonly IAvaloniaQrImageFactory _qrImageFactory;
    private readonly IAvaloniaDialogService _dialogs;
    private readonly IAvaloniaLocalizationService _localization;
    private readonly TimeSpan _countdownTickInterval;
    private readonly ISettingsService? _settingsService;
    private readonly IAvaloniaQrPreviewDialogService? _qrPreviewDialogs;
    private readonly TimeSpan _transientMessageDuration;
    private readonly AsyncCommand _loadCommand;
    private readonly AsyncCommand _generateCommand;
    private readonly AsyncCommand _copyCommand;
    private readonly AsyncCommand _generateQrCommand;
    private readonly AsyncCommand _beginAddCommand;
    private readonly AsyncCommand _beginEditCommand;
    private readonly AsyncCommand _saveAccountCommand;
    private readonly AsyncCommand _cancelEditCommand;
    private readonly AsyncCommand _deleteAccountCommand;
    private readonly AsyncCommand _beginContextEditCommand;
    private readonly AsyncCommand _deleteContextAccountCommand;
    private CancellationTokenSource? _codeLifetime;
    private CancellationTokenSource? _messageLifetime;
    private CancellationTokenSource? _recentHighlightLifetime;
    private IReadOnlyList<AccountListItemViewModel> _allAccounts = [];
    private IReadOnlyList<AccountListItemViewModel> _accounts = [];
    private string _message = string.Empty;
    private string _searchText = string.Empty;
    private AccountListItemViewModel? _selectedAccount;
    private AccountListItemViewModel? _contextAccount;
    private string _generatedCode = string.Empty;
    private string _codeMessage = string.Empty;
    private bool _isBusy;
    private bool _isGenerating;
    private int _remainingSeconds;
    private int _periodSeconds;
    private AvaloniaQrImageHandle? _qrImage;
    private bool _isEditorVisible;
    private Guid? _editingAccountId;
    private string _editorIssuer = string.Empty;
    private string _editorAccountName = string.Empty;
    private string _editorSecret = string.Empty;
    private string _editorMessage = string.Empty;
    private bool _autoGenerateCodeOnSelection;

    public AccountListViewModel(
        IAccountManager accountManager,
        IAccountTotpService accountTotpService,
        IAsyncClipboardService clipboardService,
        IAccountQrCodeService accountQrCodeService,
        IAvaloniaQrImageFactory qrImageFactory,
        IAvaloniaDialogService dialogs,
        IAvaloniaLocalizationService localization,
        TimeSpan? countdownTickInterval = null,
        ISettingsService? settingsService = null,
        IAvaloniaQrPreviewDialogService? qrPreviewDialogs = null,
        TimeSpan? transientMessageDuration = null)
    {
        _accountManager = accountManager ?? throw new ArgumentNullException(nameof(accountManager));
        _accountTotpService = accountTotpService ?? throw new ArgumentNullException(nameof(accountTotpService));
        _clipboardService = clipboardService ?? throw new ArgumentNullException(nameof(clipboardService));
        _accountQrCodeService = accountQrCodeService ?? throw new ArgumentNullException(nameof(accountQrCodeService));
        _qrImageFactory = qrImageFactory ?? throw new ArgumentNullException(nameof(qrImageFactory));
        _dialogs = dialogs ?? throw new ArgumentNullException(nameof(dialogs));
        _localization = localization ?? throw new ArgumentNullException(nameof(localization));
        _countdownTickInterval = countdownTickInterval ?? TimeSpan.FromSeconds(1);
        _settingsService = settingsService;
        _qrPreviewDialogs = qrPreviewDialogs;
        _transientMessageDuration = transientMessageDuration ?? TimeSpan.FromSeconds(3);
        if (_countdownTickInterval <= TimeSpan.Zero)
            throw new ArgumentOutOfRangeException(nameof(countdownTickInterval));
        if (_transientMessageDuration <= TimeSpan.Zero)
            throw new ArgumentOutOfRangeException(nameof(transientMessageDuration));
        _loadCommand = new AsyncCommand(LoadAsync, () => !_isBusy);
        _generateCommand = new AsyncCommand(
            GenerateCodeAsync,
            () => !_isGenerating && _selectedAccount is not null);
        _copyCommand = new AsyncCommand(CopyCodeAsync, () => HasGeneratedCode);
        _generateQrCommand = new AsyncCommand(GenerateQrAsync, () => _selectedAccount is not null);
        _beginAddCommand = new AsyncCommand(BeginAddAsync, () => !IsBusy && !IsEditorVisible);
        _beginEditCommand = new AsyncCommand(
            BeginEditAsync,
            () => !IsBusy && !IsEditorVisible && SelectedAccount is not null);
        _saveAccountCommand = new AsyncCommand(SaveAccountAsync, () => !IsBusy && IsEditorVisible);
        _cancelEditCommand = new AsyncCommand(CancelEditAsync, () => !IsBusy && IsEditorVisible);
        _deleteAccountCommand = new AsyncCommand(
            DeleteAccountAsync,
            () => !IsBusy && !IsEditorVisible && SelectedAccount is not null);
        _beginContextEditCommand = new AsyncCommand(
            BeginContextEditAsync,
            () => !IsBusy && !IsEditorVisible && ContextAccount is not null);
        _deleteContextAccountCommand = new AsyncCommand(
            DeleteContextAccountAsync,
            () => !IsBusy && !IsEditorVisible && ContextAccount is not null);
    }

    public event PropertyChangedEventHandler? PropertyChanged;

    public IReadOnlyList<AccountListItemViewModel> Accounts
    {
        get => _accounts;
        private set
        {
            if (!SetField(ref _accounts, value)) return;
            OnPropertyChanged(nameof(HasNoAccounts));
            OnPropertyChanged(nameof(HasNoSearchResults));
        }
    }

    public bool HasNoAccounts =>
        !IsBusy && !HasMessage && _allAccounts.Count == 0;

    public bool HasNoSearchResults =>
        !IsBusy && !HasMessage && _allAccounts.Count > 0 && Accounts.Count == 0;

    public string Message
    {
        get => _message;
        private set
        {
            if (!SetField(ref _message, value)) return;
            OnPropertyChanged(nameof(HasMessage));
            OnPropertyChanged(nameof(HasNoAccounts));
            OnPropertyChanged(nameof(HasNoSearchResults));
        }
    }

    public bool HasMessage => Message.Length > 0;

    public AccountListItemViewModel? SelectedAccount
    {
        get => _selectedAccount;
        set
        {
            if (!SetField(ref _selectedAccount, value)) return;
            ClearGeneratedCode();
            ClearQrImage();
            _generateCommand.NotifyCanExecuteChanged();
            _generateQrCommand.NotifyCanExecuteChanged();
            _beginEditCommand.NotifyCanExecuteChanged();
            _deleteAccountCommand.NotifyCanExecuteChanged();
            OnPropertyChanged(nameof(HasSelectedAccount));
            if (_autoGenerateCodeOnSelection && _selectedAccount is not null)
                _generateCommand.Execute(null);
        }
    }

    public void EnableAutomaticCodeGenerationOnSelection() =>
        _autoGenerateCodeOnSelection = true;

    public bool HasSelectedAccount => SelectedAccount is not null;

    public string GeneratedCode
    {
        get => _generatedCode;
        private set
        {
            if (!SetField(ref _generatedCode, value)) return;
            OnPropertyChanged(nameof(HasGeneratedCode));
            _copyCommand.NotifyCanExecuteChanged();
        }
    }

    public bool HasGeneratedCode => GeneratedCode.Length > 0;

    public int RemainingSeconds
    {
        get => _remainingSeconds;
        private set => SetField(ref _remainingSeconds, Math.Max(0, value));
    }

    public int PeriodSeconds
    {
        get => _periodSeconds;
        private set => SetField(ref _periodSeconds, Math.Max(0, value));
    }

    public string CodeMessage
    {
        get => _codeMessage;
        private set
        {
            if (!SetField(ref _codeMessage, value)) return;
            OnPropertyChanged(nameof(HasCodeMessage));
        }
    }

    public bool HasCodeMessage => CodeMessage.Length > 0;

    public IImage? QrImage => _qrImage?.Image;

    public bool HasQrImage => QrImage is not null;

    public double QrPreviewSize => 256 * Math.Clamp(
        _settingsService?.Current.QrPreviewScaleFactor
            ?? TOTP.Core.Models.AppSettings.DefaultQrPreviewScaleFactor,
        1.0,
        6.0);

    public string SearchText
    {
        get => _searchText;
        set
        {
            if (!SetField(ref _searchText, value ?? string.Empty)) return;
            ClearRecentHighlight();
            OnPropertyChanged(nameof(HasSearchText));
            ApplyFilter();
        }
    }

    public bool HasSearchText => SearchText.Length > 0;

    public bool IsBusy
    {
        get => _isBusy;
        private set
        {
            if (!SetField(ref _isBusy, value)) return;
            _loadCommand.NotifyCanExecuteChanged();
            NotifyCrudCommands();
            OnPropertyChanged(nameof(HasNoAccounts));
            OnPropertyChanged(nameof(HasNoSearchResults));
        }
    }

    public ICommand LoadCommand => _loadCommand;

    public ICommand GenerateCommand => _generateCommand;

    public ICommand CopyCommand => _copyCommand;

    public ICommand GenerateQrCommand => _generateQrCommand;

    public ICommand BeginAddCommand => _beginAddCommand;
    public ICommand BeginEditCommand => _beginEditCommand;
    public ICommand SaveAccountCommand => _saveAccountCommand;
    public ICommand CancelEditCommand => _cancelEditCommand;
    public ICommand DeleteAccountCommand => _deleteAccountCommand;
    public ICommand BeginContextEditCommand => _beginContextEditCommand;
    public ICommand DeleteContextAccountCommand => _deleteContextAccountCommand;

    public AccountListItemViewModel? ContextAccount
    {
        get => _contextAccount;
        set
        {
            if (!SetField(ref _contextAccount, value)) return;
            _beginContextEditCommand.NotifyCanExecuteChanged();
            _deleteContextAccountCommand.NotifyCanExecuteChanged();
        }
    }

    public bool IsEditorVisible
    {
        get => _isEditorVisible;
        private set
        {
            if (!SetField(ref _isEditorVisible, value)) return;
            NotifyCrudCommands();
        }
    }

    public bool IsEditingExistingAccount => _editingAccountId.HasValue;

    public string EditorIssuer
    {
        get => _editorIssuer;
        set
        {
            if (!SetField(ref _editorIssuer, value ?? string.Empty)) return;
            EditorMessage = string.Empty;
        }
    }

    public string EditorAccountName
    {
        get => _editorAccountName;
        set
        {
            if (!SetField(ref _editorAccountName, value ?? string.Empty)) return;
            EditorMessage = string.Empty;
        }
    }

    public string EditorSecret
    {
        get => _editorSecret;
        set
        {
            if (!SetField(ref _editorSecret, value ?? string.Empty)) return;
            EditorMessage = string.Empty;
        }
    }

    public string EditorMessage
    {
        get => _editorMessage;
        private set => SetField(ref _editorMessage, value);
    }

    public Task LoadAsync() => LoadAsync(null);

    private async Task LoadAsync(Guid? recentlyAddedAccountId)
    {
        if (IsBusy) return;

        IsBusy = true;
        Message = string.Empty;
        try
        {
            var result = await _accountManager.GetAllOtpEntriesSortedAsync();
            if (result.IsFailed)
            {
                _allAccounts = [];
                Accounts = [];
                Message = "Accounts could not be loaded. Your encrypted data was not changed.";
                return;
            }

            SelectedAccount = null;
            _allAccounts = result.Value
                .Select(account => new AccountListItemViewModel(
                    account.ID,
                    account.Issuer,
                    account.AccountName ?? string.Empty,
                    account.ID == recentlyAddedAccountId))
                .ToArray();
            ApplyFilter();
            StartRecentHighlightLifetime(
                _allAccounts.FirstOrDefault(account => account.IsRecentlyAdded));
        }
        catch (Exception)
        {
            _allAccounts = [];
            Accounts = [];
            Message = "Accounts could not be loaded safely. Try again.";
        }
        finally
        {
            IsBusy = false;
        }
    }

    public async Task GenerateCodeAsync()
    {
        var requestedAccount = _selectedAccount;
        if (requestedAccount is null || _isGenerating) return;

        _isGenerating = true;
        _generateCommand.NotifyCanExecuteChanged();
        ClearGeneratedCode();
        try
        {
            var result = await _accountTotpService.GenerateAsync(requestedAccount.Id);
            if (_selectedAccount?.Id != requestedAccount.Id)
                return;
            if (result.IsFailed)
            {
                CodeMessage = "A code could not be generated for this account.";
                return;
            }

            GeneratedCode = result.Value.Code;
            RemainingSeconds = Math.Max(1, result.Value.RemainingSeconds);
            PeriodSeconds = Math.Max(RemainingSeconds, result.Value.PeriodSeconds);
            var codeLifetime = new CancellationTokenSource();
            _codeLifetime = codeLifetime;
            _ = RunCodeCountdownAsync(
                requestedAccount.Id,
                codeLifetime);
            if (_autoGenerateCodeOnSelection
                && _selectedAccount?.Id == requestedAccount.Id)
            {
                await CopyCodeAsync();
            }
        }
        catch (Exception)
        {
            if (_selectedAccount?.Id == requestedAccount.Id)
                CodeMessage = "A code could not be generated safely. Try again.";
        }
        finally
        {
            _isGenerating = false;
            _generateCommand.NotifyCanExecuteChanged();
            if (_autoGenerateCodeOnSelection
                && _selectedAccount is not null
                && _selectedAccount.Id != requestedAccount.Id
                && !HasGeneratedCode)
            {
                _generateCommand.Execute(null);
            }
        }
    }

    public async Task CopyCodeAsync()
    {
        if (!HasGeneratedCode) return;
        if (_settingsService?.Current.ClearClipboardEnabled == false)
        {
            CodeMessage = _localization.GetString(AvaloniaStringKeys.ClipboardCopyDisabled);
            return;
        }

        var configuredLifetime = _settingsService?.Current.ClearClipboardSeconds
            ?? RemainingSeconds;
        var clearSeconds = Math.Max(1, Math.Min(RemainingSeconds, configuredLifetime));
        var result = await _clipboardService.CopyAndScheduleClearAsync(
            GeneratedCode,
            TimeSpan.FromSeconds(clearSeconds));
        CodeMessage = result.IsSuccess
            ? string.Format(
                _localization.GetString(AvaloniaStringKeys.CodeCopiedWithClear),
                clearSeconds)
            : _localization.GetString(AvaloniaStringKeys.ClipboardSafeCopyUnavailable);
    }

    public async Task GenerateQrAsync()
    {
        if (_selectedAccount is null) return;

        ClearQrImage();
        var result = await _accountQrCodeService.GenerateAsync(_selectedAccount.Id);
        if (result.IsFailed)
        {
            CodeMessage = "A QR code could not be generated for this account.";
            return;
        }

        using var sensitivePng = result.Value;
        try
        {
            _qrImage = _qrImageFactory.Create(sensitivePng.Memory);
            OnPropertyChanged(nameof(QrImage));
            OnPropertyChanged(nameof(HasQrImage));
            await ShowQrPreviewAsync();
        }
        catch (Exception)
        {
            CodeMessage = "A QR code could not be displayed safely.";
        }
        finally
        {
            ClearQrImage();
        }
    }

    private async Task ShowQrPreviewAsync()
    {
        var image = QrImage;
        if (image is null) return;
        if (_qrPreviewDialogs is null)
        {
            CodeMessage = "A QR code preview is not available on this platform.";
            return;
        }

        try
        {
            await _qrPreviewDialogs.ShowAsync(image, QrPreviewSize);
        }
        catch (Exception)
        {
            if (ReferenceEquals(QrImage, image))
                CodeMessage = "The enlarged QR code could not be displayed safely.";
        }
    }

    public Task BeginAddAsync()
    {
        if (IsBusy || IsEditorVisible) return Task.CompletedTask;
        ClearEditor();
        IsEditorVisible = true;
        return Task.CompletedTask;
    }

    public Task BeginEditAsync() => BeginEditAsync(SelectedAccount);

    public Task BeginContextEditAsync() => BeginEditAsync(ContextAccount);

    private async Task BeginEditAsync(AccountListItemViewModel? target)
    {
        if (IsBusy || IsEditorVisible || target is null) return;

        IsBusy = true;
        try
        {
            var loaded = await _accountManager.GetAllOtpEntriesSortedAsync();
            var account = loaded.IsSuccess
                ? loaded.Value.FirstOrDefault(value => value.ID == target.Id)
                : null;
            if (account is null)
            {
                Message = _localization.GetString(AvaloniaStringKeys.AccountEditLoadFailed);
                return;
            }

            _editingAccountId = account.ID;
            OnPropertyChanged(nameof(IsEditingExistingAccount));
            EditorIssuer = account.Issuer;
            EditorAccountName = account.AccountName ?? string.Empty;
            EditorSecret = account.Secret;
            IsEditorVisible = true;
        }
        catch (Exception)
        {
            Message = _localization.GetString(AvaloniaStringKeys.AccountEditLoadFailed);
            ClearEditor();
        }
        finally
        {
            IsBusy = false;
        }
    }

    public async Task SaveAccountAsync()
    {
        if (IsBusy || !IsEditorVisible) return;

        var issuer = EditorIssuer.Trim();
        var accountName = EditorAccountName.Trim();
        var secret = EditorSecret;
        EditorSecret = string.Empty;
        if (issuer.Length == 0)
        {
            EditorMessage = _localization.GetString(AvaloniaStringKeys.AccountIssuerRequired);
            return;
        }

        if (!SecretValidation.IsValidBase32Secret(secret))
        {
            EditorMessage = _localization.GetString(AvaloniaStringKeys.AccountSecretInvalid);
            return;
        }

        IsBusy = true;
        try
        {
            var loaded = await _accountManager.GetAllOtpEntriesSortedAsync();
            if (loaded.IsFailed)
            {
                EditorMessage = _localization.GetString(AvaloniaStringKeys.AccountSaveFailed);
                return;
            }

            if (HasDuplicateIdentity(loaded.Value, issuer, accountName, _editingAccountId))
            {
                EditorMessage = _localization.GetString(AvaloniaStringKeys.AccountDuplicate);
                return;
            }

            var isNewAccount = !_editingAccountId.HasValue;
            var normalizedSecret = SecretValidation.NormalizeBase32Secret(secret);
            var updated = new Account(
                _editingAccountId ?? Guid.NewGuid(),
                issuer,
                normalizedSecret,
                accountName.Length == 0 ? null : accountName);
            var saved = _editingAccountId.HasValue
                ? await UpdateExistingAsync(loaded.Value, updated)
                : await _accountManager.AddNewAsync(updated);
            if (saved.IsFailed)
            {
                EditorMessage = _localization.GetString(AvaloniaStringKeys.AccountSaveFailed);
                return;
            }

            ClearEditor();
            IsBusy = false;
            await LoadAsync(isNewAccount ? updated.ID : null);
            if (!HasMessage)
            {
                SelectedAccount = Accounts.FirstOrDefault(account => account.Id == updated.ID);
                ShowTransientMessage(_localization.GetString(AvaloniaStringKeys.AccountSaved));
            }
        }
        catch (Exception)
        {
            EditorMessage = _localization.GetString(AvaloniaStringKeys.AccountSaveFailed);
        }
        finally
        {
            secret = string.Empty;
            IsBusy = false;
        }
    }

    public Task CancelEditAsync()
    {
        if (IsBusy || !IsEditorVisible) return Task.CompletedTask;
        ClearEditor();
        return Task.CompletedTask;
    }

    public async Task RevealImportedAccountAsync(
        Guid accountId,
        bool highlightAsNew,
        string successMessage)
    {
        SearchText = string.Empty;
        await LoadAsync(highlightAsNew ? accountId : null);
        if (HasMessage) return;

        SelectedAccount = Accounts.FirstOrDefault(account => account.Id == accountId);
        ShowTransientMessage(successMessage);
    }

    public Task DeleteAccountAsync() => DeleteAccountAsync(SelectedAccount);

    public Task DeleteContextAccountAsync() => DeleteAccountAsync(ContextAccount);

    private async Task DeleteAccountAsync(AccountListItemViewModel? target)
    {
        if (IsBusy || IsEditorVisible || target is null) return;

        var selected = target;
        IsBusy = true;
        bool confirmed;
        try
        {
            confirmed = await _dialogs.ConfirmAsync(new ConfirmationDialogRequest(
                _localization.GetString(AvaloniaStringKeys.DeleteAccount),
                string.Format(
                    _localization.GetString(AvaloniaStringKeys.DeleteAccountPrompt),
                    selected.Issuer,
                    selected.AccountName),
                NotificationSeverity.Warning,
                _localization.GetString(AvaloniaStringKeys.Delete),
                _localization.GetString(AvaloniaStringKeys.Cancel),
                IsDestructive: true));
        }
        catch (Exception)
        {
            Message = _localization.GetString(AvaloniaStringKeys.AccountDeleteFailed);
            IsBusy = false;
            return;
        }
        if (!confirmed)
        {
            IsBusy = false;
            return;
        }

        try
        {
            var loaded = await _accountManager.GetAllOtpEntriesSortedAsync();
            var account = loaded.IsSuccess
                ? loaded.Value.FirstOrDefault(value => value.ID == selected.Id)
                : null;
            if (account is null || (await _accountManager.DeleteAsync(account)).IsFailed)
            {
                Message = _localization.GetString(AvaloniaStringKeys.AccountDeleteFailed);
                return;
            }

            SelectedAccount = null;
            IsBusy = false;
            await LoadAsync();
            if (!HasMessage)
                ShowTransientMessage(_localization.GetString(AvaloniaStringKeys.AccountDeleted));
        }
        catch (Exception)
        {
            Message = _localization.GetString(AvaloniaStringKeys.AccountDeleteFailed);
        }
        finally
        {
            IsBusy = false;
        }
    }

    private async Task<FluentResults.Result> UpdateExistingAsync(
        IReadOnlyList<Account> accounts,
        Account updated)
    {
        var previous = accounts.FirstOrDefault(value => value.ID == updated.ID);
        return previous is null
            ? FluentResults.Result.Fail("Account unavailable for update.")
            : await _accountManager.UpdateAsync(previous, updated);
    }

    private static bool HasDuplicateIdentity(
        IEnumerable<Account> accounts,
        string issuer,
        string accountName,
        Guid? excludedId) =>
        accounts.Any(account => account.ID != excludedId
            && string.Equals(account.Issuer.Trim(), issuer, StringComparison.OrdinalIgnoreCase)
            && string.Equals(
                (account.AccountName ?? string.Empty).Trim(),
                accountName,
                StringComparison.OrdinalIgnoreCase));

    private void ApplyFilter()
    {
        Accounts = AccountListFilter.Apply(_allAccounts, SearchText);
        if (SelectedAccount is not null
            && !Accounts.Any(account => account.Id == SelectedAccount.Id))
        {
            SelectedAccount = null;
        }
    }

    private async Task RunCodeCountdownAsync(Guid accountId, CancellationTokenSource lifetime)
    {
        var cancellationToken = lifetime.Token;
        try
        {
            while (!cancellationToken.IsCancellationRequested)
            {
                await Task.Delay(_countdownTickInterval, cancellationToken);
                if (RemainingSeconds > 1)
                {
                    RemainingSeconds--;
                    continue;
                }

                var refreshed = await _accountTotpService.GenerateAsync(accountId);
                if (refreshed.IsFailed
                    || SelectedAccount?.Id != accountId
                    || cancellationToken.IsCancellationRequested)
                {
                    ClearGeneratedCodeWithoutCancellingTimer();
                    CodeMessage = _localization.GetString(AvaloniaStringKeys.CodeRefreshFailed);
                    return;
                }

                GeneratedCode = refreshed.Value.Code;
                RemainingSeconds = Math.Max(1, refreshed.Value.RemainingSeconds);
                PeriodSeconds = Math.Max(RemainingSeconds, refreshed.Value.PeriodSeconds);
            }
        }
        catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested)
        {
        }
        finally
        {
            if (ReferenceEquals(_codeLifetime, lifetime))
            {
                _codeLifetime = null;
                lifetime.Dispose();
            }
        }
    }

    private void ClearGeneratedCode()
    {
        _codeLifetime?.Cancel();
        _codeLifetime?.Dispose();
        _codeLifetime = null;
        ClearGeneratedCodeWithoutCancellingTimer();
    }

    private void ClearGeneratedCodeWithoutCancellingTimer()
    {
        GeneratedCode = string.Empty;
        RemainingSeconds = 0;
        PeriodSeconds = 0;
        CodeMessage = string.Empty;
    }

    public void Dispose()
    {
        CancelTransientMessage();
        ClearRecentHighlight();
        ClearGeneratedCode();
        ClearQrImage();
        ClearEditor();
    }

    public void Clear()
    {
        CancelTransientMessage();
        ClearRecentHighlight();
        ContextAccount = null;
        SelectedAccount = null;
        SearchText = string.Empty;
        _allAccounts = [];
        Accounts = [];
        Message = string.Empty;
        ClearGeneratedCode();
        ClearQrImage();
        ClearEditor();
    }

    public void ClearSensitiveOutput()
    {
        ClearGeneratedCode();
        ClearQrImage();
        ClearEditor();
    }

    public void NotifySettingsChanged() => OnPropertyChanged(nameof(QrPreviewSize));

    private void ClearEditor()
    {
        _editingAccountId = null;
        OnPropertyChanged(nameof(IsEditingExistingAccount));
        EditorIssuer = string.Empty;
        EditorAccountName = string.Empty;
        EditorSecret = string.Empty;
        EditorMessage = string.Empty;
        IsEditorVisible = false;
    }

    private void NotifyCrudCommands()
    {
        _beginAddCommand.NotifyCanExecuteChanged();
        _beginEditCommand.NotifyCanExecuteChanged();
        _saveAccountCommand.NotifyCanExecuteChanged();
        _cancelEditCommand.NotifyCanExecuteChanged();
        _deleteAccountCommand.NotifyCanExecuteChanged();
        _beginContextEditCommand.NotifyCanExecuteChanged();
        _deleteContextAccountCommand.NotifyCanExecuteChanged();
    }

    private void ClearQrImage()
    {
        _qrPreviewDialogs?.Close();
        var image = _qrImage;
        _qrImage = null;
        OnPropertyChanged(nameof(QrImage));
        OnPropertyChanged(nameof(HasQrImage));
        image?.Dispose();
    }

    private void ShowTransientMessage(string message)
    {
        CancelTransientMessage();
        Message = message;
        var lifetime = new CancellationTokenSource();
        _messageLifetime = lifetime;
        _ = ClearTransientMessageAsync(message, lifetime);
    }

    private async Task ClearTransientMessageAsync(
        string expectedMessage,
        CancellationTokenSource lifetime)
    {
        try
        {
            await Task.Delay(_transientMessageDuration, lifetime.Token);
            if (ReferenceEquals(_messageLifetime, lifetime)
                && string.Equals(Message, expectedMessage, StringComparison.Ordinal))
            {
                Message = string.Empty;
            }
        }
        catch (OperationCanceledException) when (lifetime.IsCancellationRequested)
        {
        }
        finally
        {
            if (ReferenceEquals(_messageLifetime, lifetime)) _messageLifetime = null;
            lifetime.Dispose();
        }
    }

    private void CancelTransientMessage()
    {
        var lifetime = _messageLifetime;
        _messageLifetime = null;
        lifetime?.Cancel();
    }

    private void StartRecentHighlightLifetime(AccountListItemViewModel? highlightedAccount)
    {
        _recentHighlightLifetime?.Cancel();
        _recentHighlightLifetime = null;
        if (highlightedAccount is null) return;

        var lifetime = new CancellationTokenSource();
        _recentHighlightLifetime = lifetime;
        _ = ClearRecentHighlightAfterAnimationAsync(highlightedAccount, lifetime);
    }

    private async Task ClearRecentHighlightAfterAnimationAsync(
        AccountListItemViewModel highlightedAccount,
        CancellationTokenSource lifetime)
    {
        try
        {
            await Task.Delay(TimeSpan.FromMilliseconds(1400), lifetime.Token);
            if (ReferenceEquals(_recentHighlightLifetime, lifetime))
                highlightedAccount.ClearRecentlyAdded();
        }
        catch (OperationCanceledException) when (lifetime.IsCancellationRequested)
        {
        }
        finally
        {
            if (ReferenceEquals(_recentHighlightLifetime, lifetime))
                _recentHighlightLifetime = null;
            lifetime.Dispose();
        }
    }

    private void ClearRecentHighlight()
    {
        var lifetime = _recentHighlightLifetime;
        _recentHighlightLifetime = null;
        lifetime?.Cancel();
        foreach (var account in _allAccounts)
            account.ClearRecentlyAdded();
    }

    private bool SetField<T>(ref T field, T value, [CallerMemberName] string? propertyName = null)
    {
        if (EqualityComparer<T>.Default.Equals(field, value)) return false;
        field = value;
        OnPropertyChanged(propertyName);
        return true;
    }

    private void OnPropertyChanged([CallerMemberName] string? propertyName = null) =>
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
}
