using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows.Input;
using TOTP.Avalonia.Desktop.Localization;
using TOTP.Avalonia.Desktop.Platform;
using TOTP.Avalonia.Desktop.Presentation.Dialogs;
using TOTP.Core.Security.Interfaces;
using TOTP.Core.Security.Models;
using TOTP.Core.Services.Interfaces;

namespace TOTP.Avalonia.Desktop.Presentation;

public sealed class AuthorizationSettingsViewModel : INotifyPropertyChanged
{
    private readonly IAuthorizationService _authorization;
    private readonly IAvaloniaDialogService _dialogs;
    private readonly IAvaloniaLocalizationService _localization;
    private readonly AsyncCommand _refreshCommand;
    private readonly AsyncCommand _enableQuickUnlockCommand;
    private readonly AsyncCommand _usePasswordCommand;
    private bool _isBusy;
    private bool _isQuickUnlockAvailable;
    private bool _isQuickUnlockEnabled;
    private string _message = string.Empty;
    private NotificationSeverity _messageSeverity = NotificationSeverity.Information;

    public AuthorizationSettingsViewModel(
        IAuthorizationService authorization,
        IAvaloniaDialogService dialogs,
        IAvaloniaLocalizationService localization)
    {
        _authorization = authorization ?? throw new ArgumentNullException(nameof(authorization));
        _dialogs = dialogs ?? throw new ArgumentNullException(nameof(dialogs));
        _localization = localization ?? throw new ArgumentNullException(nameof(localization));
        _refreshCommand = new AsyncCommand(RefreshAsync, () => !IsBusy);
        _enableQuickUnlockCommand = new AsyncCommand(
            EnableQuickUnlockAsync,
            () => !IsBusy && IsQuickUnlockAvailable && !IsQuickUnlockEnabled);
        _usePasswordCommand = new AsyncCommand(
            UsePasswordAsync,
            () => !IsBusy && IsQuickUnlockEnabled);
    }

    public event PropertyChangedEventHandler? PropertyChanged;

    public ICommand RefreshCommand => _refreshCommand;
    public ICommand EnableQuickUnlockCommand => _enableQuickUnlockCommand;
    public ICommand UsePasswordCommand => _usePasswordCommand;

    public bool IsBusy
    {
        get => _isBusy;
        private set
        {
            if (!SetField(ref _isBusy, value)) return;
            NotifyCommands();
        }
    }

    public bool IsQuickUnlockAvailable
    {
        get => _isQuickUnlockAvailable;
        private set
        {
            if (!SetField(ref _isQuickUnlockAvailable, value)) return;
            _enableQuickUnlockCommand.NotifyCanExecuteChanged();
        }
    }

    public bool IsQuickUnlockEnabled
    {
        get => _isQuickUnlockEnabled;
        private set
        {
            if (!SetField(ref _isQuickUnlockEnabled, value)) return;
            _enableQuickUnlockCommand.NotifyCanExecuteChanged();
            _usePasswordCommand.NotifyCanExecuteChanged();
        }
    }

    public string Message
    {
        get => _message;
        private set => SetField(ref _message, value);
    }

    public NotificationSeverity MessageSeverity
    {
        get => _messageSeverity;
        private set => SetField(ref _messageSeverity, value);
    }

    public async Task RefreshAsync()
    {
        if (IsBusy) return;
        IsBusy = true;
        try
        {
            IsQuickUnlockEnabled = IsQuickUnlockPreferred();
            IsQuickUnlockAvailable = await _authorization.IsHelloAvailableAsync();
            SetMessage(
                IsQuickUnlockAvailable
                    ? AvaloniaStringKeys.QuickUnlockAvailable
                    : AvaloniaStringKeys.QuickUnlockUnavailable,
                IsQuickUnlockAvailable
                    ? NotificationSeverity.Information
                    : NotificationSeverity.Warning);
        }
        catch (Exception)
        {
            IsQuickUnlockAvailable = false;
            SetMessage(AvaloniaStringKeys.QuickUnlockUnavailable, NotificationSeverity.Warning);
        }
        finally
        {
            IsBusy = false;
        }
    }

    public async Task EnableQuickUnlockAsync()
    {
        if (IsBusy || !IsQuickUnlockAvailable || IsQuickUnlockEnabled) return;
        IsBusy = true;
        string? recoveryPassword = null;
        try
        {
            var selected = await _authorization.SetGateAsync(AuthorizationGateKind.Hello);
            if (selected == AuthorizationResult.PasswordRequired)
            {
                recoveryPassword = await _dialogs.PromptForPasswordAsync(CreateRecoveryPasswordRequest());
                if (recoveryPassword is null)
                {
                    SetMessage(AvaloniaStringKeys.QuickUnlockEnrollmentCancelled, NotificationSeverity.Information);
                    return;
                }

                selected = await _authorization.ConfigureHelloAsync(recoveryPassword);
            }

            if (selected == AuthorizationResult.Success)
            {
                IsQuickUnlockEnabled = true;
                SetMessage(AvaloniaStringKeys.QuickUnlockEnabled, NotificationSeverity.Success);
                return;
            }

            SetMessage(
                selected is AuthorizationResult.NotAvailable or AuthorizationResult.DisabledByPolicy
                    ? AvaloniaStringKeys.QuickUnlockUnavailable
                    : AvaloniaStringKeys.QuickUnlockEnrollmentFailed,
                NotificationSeverity.Error);
        }
        catch (Exception)
        {
            SetMessage(AvaloniaStringKeys.QuickUnlockEnrollmentFailed, NotificationSeverity.Error);
        }
        finally
        {
            recoveryPassword = null;
            IsBusy = false;
        }
    }

    public async Task UsePasswordAsync()
    {
        if (IsBusy || !IsQuickUnlockEnabled) return;
        IsBusy = true;
        string? verifiedPassword = null;
        try
        {
            verifiedPassword = await _dialogs.PromptForPasswordAsync(CreateVerificationRequest());
            if (verifiedPassword is null)
            {
                SetMessage(AvaloniaStringKeys.PasswordPreferenceUnchanged, NotificationSeverity.Information);
                return;
            }

            var selected = await _authorization.SetGateAsync(AuthorizationGateKind.Password);
            if (selected == AuthorizationResult.Success)
            {
                IsQuickUnlockEnabled = false;
                SetMessage(AvaloniaStringKeys.PasswordPreferred, NotificationSeverity.Success);
                return;
            }

            SetMessage(AvaloniaStringKeys.PasswordPreferenceFailed, NotificationSeverity.Error);
        }
        catch (Exception)
        {
            SetMessage(AvaloniaStringKeys.PasswordPreferenceFailed, NotificationSeverity.Error);
        }
        finally
        {
            verifiedPassword = null;
            IsBusy = false;
        }
    }

    private PasswordDialogRequest CreateRecoveryPasswordRequest() => new(
        _localization.GetString(AvaloniaStringKeys.EnableQuickUnlock),
        _localization.GetString(AvaloniaStringKeys.QuickUnlockRecoveryPrompt),
        _localization.GetString(AvaloniaStringKeys.Enable),
        _localization.GetString(AvaloniaStringKeys.Cancel),
        _localization.GetString(AvaloniaStringKeys.PasswordRequired),
        _localization.GetString(AvaloniaStringKeys.PasswordVerificationFailed));

    private PasswordDialogRequest CreateVerificationRequest() => new(
        _localization.GetString(AvaloniaStringKeys.UsePasswordAtStartup),
        _localization.GetString(AvaloniaStringKeys.PasswordPreferencePrompt),
        _localization.GetString(AvaloniaStringKeys.Confirm),
        _localization.GetString(AvaloniaStringKeys.Cancel),
        _localization.GetString(AvaloniaStringKeys.PasswordRequired),
        _localization.GetString(AvaloniaStringKeys.PasswordVerificationFailed),
        async (candidate, _) =>
            await _authorization.TryUnlockWithPasswordAsync(candidate) == AuthorizationResult.Success
                ? null
                : _localization.GetString(AvaloniaStringKeys.PasswordVerificationFailed));

    private bool IsQuickUnlockPreferred() =>
        _authorization.State.ConfiguredGate == AuthorizationGateKind.Hello;

    private void SetMessage(string key, NotificationSeverity severity)
    {
        Message = _localization.GetString(key);
        MessageSeverity = severity;
    }

    private void NotifyCommands()
    {
        _refreshCommand.NotifyCanExecuteChanged();
        _enableQuickUnlockCommand.NotifyCanExecuteChanged();
        _usePasswordCommand.NotifyCanExecuteChanged();
    }

    private bool SetField<T>(ref T field, T value, [CallerMemberName] string? propertyName = null)
    {
        if (EqualityComparer<T>.Default.Equals(field, value)) return false;
        field = value;
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        return true;
    }
}
