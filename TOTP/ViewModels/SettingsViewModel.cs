using System;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using System.Windows.Input;
using TOTP.Commands;
using TOTP.Security.Interfaces;
using TOTP.Security.Models;

namespace TOTP.ViewModels;

public sealed class SettingsViewModel : INotifyPropertyChanged
{
    public event PropertyChangedEventHandler? PropertyChanged;

    private bool _isHelloSelected = true;
    public bool IsHelloSelected
    {
        get => _isHelloSelected;
        set
        {
            if (_isHelloSelected == value) return;
            _isHelloSelected = value;
            OnPropertyChanged();

            if (value && _isPasswordSelected)
            {
                _isPasswordSelected = false;
                OnPropertyChanged(nameof(IsPasswordSelected));
            }
        }
    }

    private bool _isPasswordSelected;
    public bool IsPasswordSelected
    {
        get => _isPasswordSelected;
        set
        {
            if (_isPasswordSelected == value) return;
            _isPasswordSelected = value;
            OnPropertyChanged();

            if (value && _isHelloSelected)
            {
                _isHelloSelected = false;
                OnPropertyChanged(nameof(IsHelloSelected));
            }
        }
    }

    private bool _lockOnSessionLock = true;
    public bool LockOnSessionLock
    {
        get => _lockOnSessionLock;
        set
        {
            if (_lockOnSessionLock == value) return;
            _lockOnSessionLock = value;
            OnPropertyChanged();
        }
    }

    private bool _clearClipboardEnabled = true;
    public bool ClearClipboardEnabled
    {
        get => _clearClipboardEnabled;
        set
        {
            if (_clearClipboardEnabled == value) return;
            _clearClipboardEnabled = value;
            OnPropertyChanged();
        }
    }

    private int _clearClipboardSeconds = 15;
    public int ClearClipboardSeconds
    {
        get => _clearClipboardSeconds;
        set
        {
            if (_clearClipboardSeconds == value) return;
            _clearClipboardSeconds = value;
            OnPropertyChanged();
        }
    }

    private bool _exportIncludeQr;
    public bool ExportIncludeQr
    {
        get => _exportIncludeQr;
        set
        {
            if (_exportIncludeQr == value) return;
            _exportIncludeQr = value;
            OnPropertyChanged();
        }
    }

    private bool _exportEncrypt = true;
    public bool ExportEncrypt
    {
        get => _exportEncrypt;
        set
        {
            if (_exportEncrypt == value) return;
            _exportEncrypt = value;
            OnPropertyChanged();
        }
    }

    private bool _hideSecretsByDefault = true;
    public bool HideSecretsByDefault
    {
        get => _hideSecretsByDefault;
        set
        {
            if (_hideSecretsByDefault == value) return;
            _hideSecretsByDefault = value;
            OnPropertyChanged();
        }
    }

    private string? _authError;
    public string? AuthError
    {
        get => _authError;
        set
        {
            if (string.Equals(_authError, value, StringComparison.Ordinal)) return;
            _authError = value;
            OnPropertyChanged();
            OnPropertyChanged(nameof(HasAuthError));
        }
    }

    public bool HasAuthError => !string.IsNullOrWhiteSpace(AuthError);

    private bool _isHelloAvailable = true;
    public bool IsHelloAvailable
    {
        get => _isHelloAvailable;
        private set
        {
            if (_isHelloAvailable == value) return;
            _isHelloAvailable = value;
            OnPropertyChanged();
            OnPropertyChanged(nameof(IsHelloUnavailable));
            OnPropertyChanged(nameof(HelloUnavailableText));
        }
    }

    public bool IsHelloUnavailable => !IsHelloAvailable;

    public string HelloUnavailableText => IsHelloUnavailable
        ? "(Not available)"
        : string.Empty;

    private readonly IAuthorizationService _authorizationService;

    private string _newPassword = string.Empty;
    public string NewPassword
    {
        get => _newPassword;
        set
        {
            if (string.Equals(_newPassword, value, StringComparison.Ordinal)) return;
            _newPassword = value;
            OnPropertyChanged();
        }
    }

    private string _confirmPassword = string.Empty;
    public string ConfirmPassword
    {
        get => _confirmPassword;
        set
        {
            if (string.Equals(_confirmPassword, value, StringComparison.Ordinal)) return;
            _confirmPassword = value;
            OnPropertyChanged();
        }
    }

    private readonly IGlobalProfileStore _globalProfileStore;

    // bound to SettingsView.xaml uc
    public ICommand SaveCommand { get; }
    public ICommand CloseCommand { get; }
    public ICommand ExportTestCommand { get; }

    public SettingsViewModel(IGlobalProfileStore globalProfileStore, IAuthorizationService authorizationService, ICommand closeCommand, Action saveAction, Action exportTest)
    {
        _globalProfileStore = globalProfileStore ?? throw new ArgumentNullException(nameof(globalProfileStore));
        _authorizationService = authorizationService ?? throw new ArgumentNullException(nameof(authorizationService));
        CloseCommand = closeCommand;
        SaveCommand = new AsyncCommand(SaveAndCloseAsync);
        ExportTestCommand = new RelayCommand(_ => exportTest());

        _ = LoadAsync();

        async Task SaveAndCloseAsync()
        {
            AuthError = null;

            if (!await ApplyAuthorizationSettingsAsync())
                return;

            await SaveAsync();
            saveAction();
        }
    }

    private async Task LoadAsync()
    {
        IsHelloAvailable = await _authorizationService.IsHelloAvailableAsync();

        var profile = await _globalProfileStore.LoadAsync();
        if (profile is null)
            return;

        IsHelloSelected = profile.Authorization.Gate != AuthorizationGateKind.Password;
        IsPasswordSelected = profile.Authorization.Gate == AuthorizationGateKind.Password;

        if (!IsHelloAvailable && IsHelloSelected)
        {
            IsHelloSelected = false;
            IsPasswordSelected = true;
        }

        LockOnSessionLock = profile.LockOnSessionLock;
        ClearClipboardEnabled = profile.ClearClipboardEnabled;
        ClearClipboardSeconds = profile.ClearClipboardSeconds > 0
            ? profile.ClearClipboardSeconds
            : GlobalProfile.DefaultClearClipboardSeconds;
        ExportIncludeQr = profile.ExportIncludeQr;
        ExportEncrypt = profile.ExportEncrypt;
        HideSecretsByDefault = profile.HideSecretsByDefault;
    }

    private async Task<bool> ApplyAuthorizationSettingsAsync()
    {
        var currentProfile = await _globalProfileStore.LoadAsync() ?? new GlobalProfile();
        var currentGate = currentProfile.Authorization.Gate;

        if (IsHelloSelected && currentGate != AuthorizationGateKind.WindowsHello)
        {
            if (!IsHelloAvailable)
            {
                AuthError = "Not available";
                return false;
            }

            var helloResult = await _authorizationService.ConfigureHelloAsync();
            if (helloResult != AuthorizationResult.Success)
            {
                AuthError = "Not available";
                return false;
            }

            NewPassword = string.Empty;
            ConfirmPassword = string.Empty;
            return true;
        }

        if (!IsPasswordSelected)
            return true;

        var passwordChanged = !string.IsNullOrWhiteSpace(NewPassword) || !string.IsNullOrWhiteSpace(ConfirmPassword);
        var switchingToPassword = currentGate != AuthorizationGateKind.Password;

        if (!passwordChanged && !switchingToPassword)
            return true;

        var result = await _authorizationService.ConfigurePasswordAsync(NewPassword, ConfirmPassword);
        if (result == AuthorizationResult.Success)
        {
            NewPassword = string.Empty;
            ConfirmPassword = string.Empty;
            return true;
        }

        AuthError = result switch
        {
            AuthorizationResult.InvalidCredentials => "Password must be at least 3 characters and both fields must match.",
            _ => "Failed to configure password authentication."
        };

        return false;
    }

    private async Task SaveAsync()
    {
        var profile = await _globalProfileStore.LoadAsync() ?? new GlobalProfile();

        profile.LockOnSessionLock = LockOnSessionLock;
        profile.ClearClipboardEnabled = ClearClipboardEnabled;
        profile.ClearClipboardSeconds = ClearClipboardSeconds > 0
            ? ClearClipboardSeconds
            : GlobalProfile.DefaultClearClipboardSeconds;
        profile.ExportIncludeQr = ExportIncludeQr;
        profile.ExportEncrypt = ExportEncrypt;
        profile.HideSecretsByDefault = HideSecretsByDefault;

        await _globalProfileStore.SaveAsync(profile);
    }

    private void OnPropertyChanged([CallerMemberName] string? name = null)
        => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
}
