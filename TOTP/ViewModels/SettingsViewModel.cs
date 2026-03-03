using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using System.Windows.Input;
using TOTP.Commands;
using TOTP.Core.Enums;
using TOTP.Core.Security.Interfaces;
using TOTP.Core.Security.Models;
using TOTP.Core.Services.Interfaces;
using TOTP.Infrastructure.Common;

namespace TOTP.ViewModels;

public sealed class SettingsViewModel : INotifyPropertyChanged
{
    #region ### PROPERTIES/FIELDS ###

    public event PropertyChangedEventHandler? PropertyChanged;
    private readonly IAuthorizationService _authorizationService;
    private readonly ILogSwitchService _logSwitchService;
    private readonly Action _saveAction;

    #region UI State



    private int _requestFocusTick;
    public int RequestFocusTick
    {
        get => _requestFocusTick;
        set { _requestFocusTick = value; OnPropertyChanged(); }
    }

    public void RequestFocus() => RequestFocusTick++;

    private bool _isHelloSelected;
    public bool IsHelloSelected
    {
        get => _isHelloSelected;
        set
        {
            if (_isHelloSelected == value) return;
            _isHelloSelected = value;
            OnPropertyChanged();
            if (value) IsPasswordSelected = false;
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
            if (value) IsHelloSelected = false;
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

    private bool _isHelloAvailable;
    public bool IsHelloAvailable
    {
        get => _isHelloAvailable;
        private set
        {
            _isHelloAvailable = value;
            OnPropertyChanged();
            OnPropertyChanged(nameof(IsHelloUnavailable));
            OnPropertyChanged(nameof(HelloUnavailableText));
        }
    }

    public bool IsHelloUnavailable => !IsHelloAvailable;
    public string HelloUnavailableText => IsHelloUnavailable ? "(Not available)" : string.Empty;

    private string _newPassword = string.Empty;
    public string NewPassword
    {
        get => _newPassword;
        set { _newPassword = value; OnPropertyChanged(); }
    }

    private string _confirmPassword = string.Empty;
    public string ConfirmPassword
    {
        get => _confirmPassword;
        set { _confirmPassword = value; OnPropertyChanged(); }
    }
    #endregion

    #region General Settings
    private bool _lockOnSessionLock = true;
    public bool LockOnSessionLock { get => _lockOnSessionLock; set { _lockOnSessionLock = value; OnPropertyChanged(); } }

    private bool _clearClipboardEnabled = true;
    public bool ClearClipboardEnabled { get => _clearClipboardEnabled; set { _clearClipboardEnabled = value; OnPropertyChanged(); } }

    private int _clearClipboardSeconds = 15;
    public int ClearClipboardSeconds { get => _clearClipboardSeconds; set { _clearClipboardSeconds = value; OnPropertyChanged(); } }

    private bool _exportEncrypt = true;
    public bool ExportEncrypt { get => _exportEncrypt; set { _exportEncrypt = value; OnPropertyChanged(); } }

    private bool _hideSecretsByDefault = true;
    public bool HideSecretsByDefault { get => _hideSecretsByDefault; set { _hideSecretsByDefault = value; OnPropertyChanged(); } }

    private AppLogLevel _selectedLogLevel;
    public AppLogLevel SelectedLogLevel
    {
        get => _selectedLogLevel;
        set
        {
            _selectedLogLevel = value;
            OnPropertyChanged();
        }
    }

    public List<AppLogLevel> AvailableLogLevels { get; }
    public bool IsCliOverrideActive => _logSwitchService.IsCliOverrideActive;
    public string ClrOverrideText => _logSwitchService.IsCliOverrideActive ?
        $"(Overridden via CLI to {SelectedLogLevel})" :
        "";

    #endregion

    public ICommand SaveCommand { get; }
    public ICommand CloseCommand { get; }
    public ICommand ExportCommand { get; }
    public ICommand OpenLogFolderCommand { get; }

    public delegate SettingsViewModel SettingsViewModelFactory(
        ICommand closeCommand,
        Action saveAction,
        Func<bool, Task> actionExportOtps);

    private ISettingsService _settingsSvc;
    IAppSettings _appSettings => _settingsSvc.Current;
    #endregion

    #region  ### CONSTRUCTOR ###

    public SettingsViewModel(ISettingsService settingsSvc,
                            IAuthorizationService authorizationService,
                            ILogSwitchService logSwitchService,
                            ICommand closeCommand,
                            Action saveAction,
                            Func<bool, Task> actionExportOtps)
    {
        _logSwitchService = logSwitchService;
        _settingsSvc = settingsSvc;
        _authorizationService = authorizationService;
        _saveAction = saveAction;
        
        CloseCommand = closeCommand;
        SaveCommand = new AsyncCommand(SaveAndCloseAsync);
        ExportCommand = new AsyncCommand(() => actionExportOtps(ExportEncrypt));
        OpenLogFolderCommand = new RelayCommand(OnOpenLogFolder);

        AvailableLogLevels = Enum.GetValues(typeof(AppLogLevel)).Cast<AppLogLevel>().ToList();
        _selectedLogLevel = _logSwitchService.MinimumLevel;
 
    }

    #endregion

    public async Task LoadAsync()
    {
        IsHelloAvailable = await _authorizationService.IsHelloAvailableAsync();
        

        SelectedLogLevel = _logSwitchService.IsCliOverrideActive ? _logSwitchService.GetLevel() : _appSettings.MinimumLogLevel;

        // Match UI Radio Buttons to Profile Gate
        IsHelloSelected = _appSettings.Authorization.Gate == AuthorizationGateKind.Hello;
        IsPasswordSelected = _appSettings.Authorization.Gate == AuthorizationGateKind.Password;

        // Fallback if Hello was saved but is no longer available on this hardware
        if (!IsHelloAvailable && IsHelloSelected)
        {
            IsHelloSelected = false;
            IsPasswordSelected = true;
        }

        LockOnSessionLock = _appSettings.LockOnSessionLock;
        ClearClipboardEnabled = _appSettings.ClearClipboardEnabled;
        ClearClipboardSeconds = _appSettings.ClearClipboardSeconds > 0 ? _appSettings.ClearClipboardSeconds : 15;
        ExportEncrypt = _appSettings.ExportEncrypt;
        HideSecretsByDefault = _appSettings.HideSecretsByDefault;
    }

    async Task SaveAndCloseAsync()
    {
        AuthError = null;

        if (!await ApplyAuthorizationSettingsAsync())
            return;

        await SaveGeneralSettingsAsync();
        _saveAction();
    }

    private async Task<bool> ApplyAuthorizationSettingsAsync()
    {
        //var applicationSettings = await _appSettings.LoadAsync() ?? new AppSettings();
        var currentGate = _appSettings.Authorization.Gate;

        // CASE 1: Moving to Windows Hello
        if (IsHelloSelected && currentGate != AuthorizationGateKind.Hello)
        {
            if (!IsHelloAvailable)
            {
                AuthError = "Windows Hello is not supported on this device.";
                return false;
            }

            // If never setup, we need to wrap the DEK. ConfigureHelloAsync does this.
            var result = _appSettings.Authorization.HasHelloSetup
                ? await _authorizationService.ChangePasswordAsync("", "") // This is a placeholder for a "SetGateOnly" if you add it, but for now:
                : await _authorizationService.ConfigureHelloAsync();

            // Professional adjustment: We need a simple "SetGate" in the service for when Hello is already configured.
            // For now, if it's already setup, we'll just let the SaveGeneralSettingsAsync handle the Gate property.
        }

        // CASE 2: Password Changes
        if (!string.IsNullOrWhiteSpace(NewPassword))
        {
            var result = await _authorizationService.ChangePasswordAsync(NewPassword, ConfirmPassword);
            if (result != AuthorizationResult.Success)
            {
                AuthError = "Passwords must match and be at least 8 characters.";
                return false;
            }
        }

        NewPassword = string.Empty;
        ConfirmPassword = string.Empty;
        return true;
    }

    private async Task SaveGeneralSettingsAsync()
    {
        // Sync Gate choice
        _appSettings.Authorization.Gate = IsHelloSelected ? AuthorizationGateKind.Hello : AuthorizationGateKind.Password;
        //ILogSwitchService.
        _appSettings.MinimumLogLevel = SelectedLogLevel;

        _appSettings.LockOnSessionLock = LockOnSessionLock;
        _appSettings.ClearClipboardEnabled = ClearClipboardEnabled;
        _appSettings.ClearClipboardSeconds = ClearClipboardSeconds;
        _appSettings.ExportEncrypt = ExportEncrypt;
        _appSettings.HideSecretsByDefault = HideSecretsByDefault;

        await _settingsSvc.SaveAsync();

        if (SelectedLogLevel != _logSwitchService.GetLevel())
        {
            _logSwitchService.SetLevel(SelectedLogLevel);
            _logSwitchService.IsCliOverrideActive = false;
            OnPropertyChanged(nameof(IsCliOverrideActive));
            OnPropertyChanged(nameof(ClrOverrideText));
        }

    }

    private void OnOpenLogFolder()
    {
        var path = System.IO.Path.GetDirectoryName(StringsConstants.AppLogPath);
        if (System.IO.Directory.Exists(path))
            Process.Start(new ProcessStartInfo("explorer.exe", path) { UseShellExecute = true });
    }

    private void OnPropertyChanged([CallerMemberName] string? name = null)
        => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
}