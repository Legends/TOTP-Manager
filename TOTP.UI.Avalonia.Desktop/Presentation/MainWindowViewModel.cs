using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows.Input;
using TOTP.Core.Security.Interfaces;
using TOTP.Avalonia.Desktop.Startup;

namespace TOTP.Avalonia.Desktop.Presentation;

public sealed class MainWindowViewModel : INotifyPropertyChanged, IDisposable
{
    private readonly IAvaloniaStartupCoordinator _startupCoordinator;
    private readonly IAuthorizationService _authorizationService;
    private readonly AsyncCommand _initializeCommand;
    private readonly AsyncCommand _lockCommand;
    private readonly AsyncCommand _showAccountsCommand;
    private readonly AsyncCommand _showToolsCommand;
    private readonly AsyncCommand _showSettingsCommand;
    private readonly CancellationTokenSource _lifetime = new();
    private string _statusText = "Starting TOTP Manager…";
    private bool _isBusy;
    private bool _canRetry;
    private bool _isPasswordUnlockVisible;
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
        AccountListViewModel accountList,
        SettingsPageViewModel settingsPage,
        NativeFilePickerViewModel nativeFilePicker,
        CameraScannerViewModel cameraScanner,
        UpdateCheckViewModel updateCheck)
    {
        _startupCoordinator = startupCoordinator ?? throw new ArgumentNullException(nameof(startupCoordinator));
        _authorizationService = authorizationService ?? throw new ArgumentNullException(nameof(authorizationService));
        PasswordUnlock = passwordUnlock ?? throw new ArgumentNullException(nameof(passwordUnlock));
        AccountList = accountList ?? throw new ArgumentNullException(nameof(accountList));
        SettingsPage = settingsPage ?? throw new ArgumentNullException(nameof(settingsPage));
        NativeFilePicker = nativeFilePicker ?? throw new ArgumentNullException(nameof(nativeFilePicker));
        CameraScanner = cameraScanner ?? throw new ArgumentNullException(nameof(cameraScanner));
        UpdateCheck = updateCheck ?? throw new ArgumentNullException(nameof(updateCheck));
        PasswordUnlock.Unlocked += OnUnlocked;
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

    public AccountListViewModel AccountList { get; }

    public SettingsPageViewModel SettingsPage { get; }

    public NativeFilePickerViewModel NativeFilePicker { get; }

    public CameraScannerViewModel CameraScanner { get; }

    public UpdateCheckViewModel UpdateCheck { get; }

    public bool IsPasswordUnlockVisible
    {
        get => _isPasswordUnlockVisible;
        private set => SetField(ref _isPasswordUnlockVisible, value);
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
        IsShellVisible = false;
        IsAccountListVisible = false;
        IsToolsVisible = false;
        IsSettingsVisible = false;
        StatusText = "Starting TOTP Manager…";

        try
        {
            var outcome = await _startupCoordinator.InitializeAsync(_lifetime.Token);
            (StatusText, CanRetry) = outcome switch
            {
                AvaloniaStartupOutcome.ReadyForPasswordSetup =>
                    ("Create a master password to protect your authenticator.", false),
                AvaloniaStartupOutcome.ReadyForUnlock =>
                    ("Enter your master password to unlock your authenticator.", false),
                AvaloniaStartupOutcome.PreferencesUnavailable =>
                    ("Your preferences could not be loaded. Your encrypted data was not changed.", true),
                _ =>
                    ("TOTP Manager could not start safely. Your encrypted data was not changed.", true)
            };
            IsPasswordUnlockVisible = outcome == AvaloniaStartupOutcome.ReadyForUnlock;
        }
        catch (OperationCanceledException) when (_lifetime.IsCancellationRequested)
        {
            StatusText = "Startup cancelled.";
        }
        catch (Exception)
        {
            StatusText = "TOTP Manager could not start safely. Your encrypted data was not changed.";
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
        _lifetime.Cancel();
        _lifetime.Dispose();
        CameraScanner.Dispose();
    }

    public void PrepareForShutdown()
    {
        if (_shutdownPrepared) return;
        _shutdownPrepared = true;

        _authorizationService.Lock();
        AccountList.Clear();
        CameraScanner.Clear();
        IsSettingsVisible = false;
        IsToolsVisible = false;
        IsShellVisible = false;
        IsAccountListVisible = false;
        IsPasswordUnlockVisible = false;
        StatusText = "TOTP Manager is closing safely.";
    }

    private void OnUnlocked(object? sender, EventArgs e)
    {
        IsPasswordUnlockVisible = false;
        IsShellVisible = true;
        SetActivePage(ShellPage.Accounts);
        StatusText = "Vault unlocked.";
        AccountList.LoadCommand.Execute(null);
    }

    public Task LockAsync()
    {
        _authorizationService.Lock();
        AccountList.Clear();
        CameraScanner.Clear();
        IsSettingsVisible = false;
        IsToolsVisible = false;
        IsShellVisible = false;
        IsAccountListVisible = false;
        IsPasswordUnlockVisible = true;
        StatusText = "Vault locked. Enter your master password to continue.";
        return Task.CompletedTask;
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

    public Task ShowSettingsAsync()
    {
        if (IsShellVisible) SetActivePage(ShellPage.Settings);
        return Task.CompletedTask;
    }

    private void SetActivePage(ShellPage page)
    {
        if (IsAccountListVisible && page != ShellPage.Accounts)
            AccountList.ClearSensitiveOutput();
        if (IsToolsVisible && page != ShellPage.Tools)
            CameraScanner.Clear();

        IsAccountListVisible = page == ShellPage.Accounts;
        IsToolsVisible = page == ShellPage.Tools;
        IsSettingsVisible = page == ShellPage.Settings;
        if (IsSettingsVisible) SettingsPage.Reload();
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
