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
    private readonly AsyncCommand _toggleSettingsCommand;
    private readonly CancellationTokenSource _lifetime = new();
    private string _statusText = "Starting TOTP Manager…";
    private bool _isBusy;
    private bool _canRetry;
    private bool _isPasswordUnlockVisible;
    private bool _isAccountListVisible;
    private bool _isSettingsVisible;

    public MainWindowViewModel(
        IAvaloniaStartupCoordinator startupCoordinator,
        IAuthorizationService authorizationService,
        PasswordUnlockViewModel passwordUnlock,
        AccountListViewModel accountList,
        SettingsPageViewModel settingsPage,
        NativeFilePickerViewModel nativeFilePicker)
    {
        _startupCoordinator = startupCoordinator ?? throw new ArgumentNullException(nameof(startupCoordinator));
        _authorizationService = authorizationService ?? throw new ArgumentNullException(nameof(authorizationService));
        PasswordUnlock = passwordUnlock ?? throw new ArgumentNullException(nameof(passwordUnlock));
        AccountList = accountList ?? throw new ArgumentNullException(nameof(accountList));
        SettingsPage = settingsPage ?? throw new ArgumentNullException(nameof(settingsPage));
        NativeFilePicker = nativeFilePicker ?? throw new ArgumentNullException(nameof(nativeFilePicker));
        PasswordUnlock.Unlocked += OnUnlocked;
        _initializeCommand = new AsyncCommand(InitializeAsync, () => !_isBusy);
        _lockCommand = new AsyncCommand(LockAsync, () => _isAccountListVisible);
        _toggleSettingsCommand = new AsyncCommand(ToggleSettingsAsync, () => _isAccountListVisible);
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

    public ICommand ToggleSettingsCommand => _toggleSettingsCommand;

    public PasswordUnlockViewModel PasswordUnlock { get; }

    public AccountListViewModel AccountList { get; }

    public SettingsPageViewModel SettingsPage { get; }

    public NativeFilePickerViewModel NativeFilePicker { get; }

    public bool IsPasswordUnlockVisible
    {
        get => _isPasswordUnlockVisible;
        private set => SetField(ref _isPasswordUnlockVisible, value);
    }

    public bool IsAccountListVisible
    {
        get => _isAccountListVisible;
        private set
        {
            if (!SetField(ref _isAccountListVisible, value)) return;
            _lockCommand.NotifyCanExecuteChanged();
            _toggleSettingsCommand.NotifyCanExecuteChanged();
        }
    }

    public bool IsSettingsVisible
    {
        get => _isSettingsVisible;
        private set => SetField(ref _isSettingsVisible, value);
    }

    public async Task InitializeAsync()
    {
        if (IsBusy) return;

        IsBusy = true;
        CanRetry = false;
        IsPasswordUnlockVisible = false;
        IsAccountListVisible = false;
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
        PasswordUnlock.Unlocked -= OnUnlocked;
        _lifetime.Cancel();
        _lifetime.Dispose();
    }

    private void OnUnlocked(object? sender, EventArgs e)
    {
        IsPasswordUnlockVisible = false;
        IsAccountListVisible = true;
        StatusText = "Vault unlocked.";
        AccountList.LoadCommand.Execute(null);
    }

    public Task LockAsync()
    {
        _authorizationService.Lock();
        AccountList.Clear();
        IsSettingsVisible = false;
        IsAccountListVisible = false;
        IsPasswordUnlockVisible = true;
        StatusText = "Vault locked. Enter your master password to continue.";
        return Task.CompletedTask;
    }

    public Task ToggleSettingsAsync()
    {
        IsSettingsVisible = !IsSettingsVisible;
        if (IsSettingsVisible) SettingsPage.Reload();
        return Task.CompletedTask;
    }

    private bool SetField<T>(ref T field, T value, [CallerMemberName] string? propertyName = null)
    {
        if (EqualityComparer<T>.Default.Equals(field, value)) return false;
        field = value;
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        return true;
    }
}
