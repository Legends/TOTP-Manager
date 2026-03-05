using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Input;
using TOTP.Commands;
using TOTP.Core.Enums;
using TOTP.Core.Models;
using TOTP.Core.Security.Interfaces;
using TOTP.Core.Security.Models;
using TOTP.Core.Services.Interfaces;
using TOTP.Infrastructure.Common;
using TOTP.Resources;
using TOTP.Services.Interfaces;

namespace TOTP.ViewModels;

public sealed class ImportConflictOptionItem
{
    public ImportConflictOptionItem(ImportConflictStrategy strategy, string displayName)
    {
        Strategy = strategy;
        DisplayName = displayName;
    }

    public ImportConflictStrategy Strategy { get; }
    public string DisplayName { get; }
}

public sealed class SettingsViewModel : INotifyPropertyChanged
{
    private const double MinQrPreviewScale = 1.0;
    private const double MaxQrPreviewScale = 6.0;
    private static readonly ExportFileFormat[] PlainExportFormats =
    [
        ExportFileFormat.Json,
        ExportFileFormat.Csv,
        ExportFileFormat.Txt
    ];
    private static readonly ExportFileFormat[] EncryptedExportFormats =
    [
        ExportFileFormat.Totp
    ];

    #region ### PROPERTIES/FIELDS ###

    public event PropertyChangedEventHandler? PropertyChanged;
    private readonly IAuthorizationService _authorizationService;
    private readonly ILogSwitchService _logSwitchService;
    private readonly IQrPreviewService _qrPreviewService;
    private readonly Action _saveAction;
    private CancellationTokenSource? _saveDebounceCts;
    private CancellationTokenSource? _authGateDebounceCts;
    private bool _isLoadingSettings;
    private bool _suppressAuthAutoSave;

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
            if (!_suppressAuthAutoSave)
            {
                QueueAuthorizationGateSave();
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
            if (value) IsHelloSelected = false;
            if (!_suppressAuthAutoSave)
            {
                QueueAuthorizationGateSave();
            }
            RaiseCommandStates();
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
        set
        {
            _newPassword = value;
            OnPropertyChanged();
            RaiseCommandStates();
        }
    }

    private string _confirmPassword = string.Empty;
    public string ConfirmPassword
    {
        get => _confirmPassword;
        set
        {
            _confirmPassword = value;
            OnPropertyChanged();
            RaiseCommandStates();
        }
    }
    #endregion

    #region General Settings
    private bool _lockOnSessionLock = true;
    public bool LockOnSessionLock
    {
        get => _lockOnSessionLock;
        set
        {
            if (_lockOnSessionLock == value) return;
            _lockOnSessionLock = value;
            OnPropertyChanged();
            OnPropertyChanged(nameof(CanResetToDefaults));
            QueueGeneralSettingsSave();
        }
    }

    private bool _lockOnMinimize = true;
    public bool LockOnMinimize
    {
        get => _lockOnMinimize;
        set
        {
            if (_lockOnMinimize == value) return;
            _lockOnMinimize = value;
            OnPropertyChanged();
            OnPropertyChanged(nameof(CanResetToDefaults));
            QueueGeneralSettingsSave();
        }
    }

    private bool _lockOnIdleTimeout = true;
    public bool LockOnIdleTimeout
    {
        get => _lockOnIdleTimeout;
        set
        {
            if (_lockOnIdleTimeout == value)
            {
                return;
            }

            _lockOnIdleTimeout = value;
            OnPropertyChanged();
            OnPropertyChanged(nameof(IsIdleTimeoutMinutesEnabled));
            OnPropertyChanged(nameof(CanResetToDefaults));
            QueueGeneralSettingsSave();
        }
    }

    private int _idleTimeoutMinutes = 10;
    public int IdleTimeoutMinutes
    {
        get => _idleTimeoutMinutes;
        set
        {
            var clamped = Math.Clamp(value, 1, 1440);
            if (_idleTimeoutMinutes == clamped)
            {
                return;
            }

            _idleTimeoutMinutes = clamped;
            OnPropertyChanged();
            OnPropertyChanged(nameof(CanResetToDefaults));
            QueueGeneralSettingsSave();
        }
    }

    public bool IsIdleTimeoutMinutesEnabled => LockOnIdleTimeout;

    private bool _clearClipboardEnabled = true;
    public bool ClearClipboardEnabled
    {
        get => _clearClipboardEnabled;
        set
        {
            if (_clearClipboardEnabled == value) return;
            _clearClipboardEnabled = value;
            OnPropertyChanged();
            OnPropertyChanged(nameof(CanResetToDefaults));
            QueueGeneralSettingsSave();
        }
    }

    private int _clearClipboardSeconds = 15;
    public int ClearClipboardSeconds
    {
        get => _clearClipboardSeconds;
        set
        {
            var clamped = Math.Clamp(value, 1, 300);
            if (_clearClipboardSeconds == clamped) return;
            _clearClipboardSeconds = clamped;
            OnPropertyChanged();
            OnPropertyChanged(nameof(CanResetToDefaults));
            QueueGeneralSettingsSave();
        }
    }

    private double _qrPreviewScaleFactor = 2.0;
    public double QrPreviewScaleFactor
    {
        get => _qrPreviewScaleFactor;
        set
        {
            var clamped = Math.Clamp(value, MinQrPreviewScale, MaxQrPreviewScale);
            var halfStep = Math.Round(clamped * 2d, MidpointRounding.AwayFromZero) / 2d;
            if (Math.Abs(_qrPreviewScaleFactor - halfStep) < 0.001d) return;
            _qrPreviewScaleFactor = halfStep;
            OnPropertyChanged();
            OnPropertyChanged(nameof(CanResetToDefaults));
            QueueGeneralSettingsSave();
        }
    }

    private bool _exportEncrypt = true;
    private bool _openExportFileAfterExportBeforeEncrypt = true;
    public bool ExportEncrypt
    {
        get => _exportEncrypt;
        set
        {
            if (_exportEncrypt == value)
            {
                return;
            }

            if (value)
            {
                _openExportFileAfterExportBeforeEncrypt = OpenExportFileAfterExport;
            }

            _exportEncrypt = value;
            OnPropertyChanged();
            OnPropertyChanged(nameof(IsExportFormatSelectionEnabled));
            OnPropertyChanged(nameof(IsOpenExportFileAfterExportOptionEnabled));

            OpenExportFileAfterExport = _exportEncrypt
                ? false
                : _openExportFileAfterExportBeforeEncrypt;

            UpdateAvailableExportFormats();
            OnPropertyChanged(nameof(CanResetToDefaults));
            QueueGeneralSettingsSave();
        }
    }

    private ExportFileFormat _selectedExportFormat = ExportFileFormat.Totp;
    public ExportFileFormat SelectedExportFormat
    {
        get => _selectedExportFormat;
        set
        {
            if (_selectedExportFormat == value) return;
            _selectedExportFormat = value;
            OnPropertyChanged();
            QueueGeneralSettingsSave();
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
            OnPropertyChanged(nameof(CanResetToDefaults));
            QueueGeneralSettingsSave();
        }
    }

    private AppLogLevel _selectedLogLevel;
    public AppLogLevel SelectedLogLevel
    {
        get => _selectedLogLevel;
        set
        {
            _selectedLogLevel = value;
            OnPropertyChanged();
            OnPropertyChanged(nameof(CanResetToDefaults));
            QueueGeneralSettingsSave();
        }
    }

    public List<AppLogLevel> AvailableLogLevels { get; }
    public ObservableCollection<ExportFileFormat> AvailableExportFormats { get; }
    public List<ImportConflictOptionItem> AvailableImportConflictOptions { get; }
    private ImportConflictOptionItem _selectedImportConflictOption = null!;
    public ImportConflictOptionItem SelectedImportConflictOption
    {
        get => _selectedImportConflictOption;
        set
        {
            _selectedImportConflictOption = value;
            OnPropertyChanged();
        }
    }
    public bool IsExportFormatSelectionEnabled => !ExportEncrypt;
    public bool IsOpenExportFileAfterExportOptionEnabled => !ExportEncrypt;

    private bool _openExportFileAfterExport = true;
    public bool OpenExportFileAfterExport
    {
        get => _openExportFileAfterExport;
        set
        {
            if (_openExportFileAfterExport == value)
            {
                return;
            }

            _openExportFileAfterExport = value;
            OnPropertyChanged();

            if (!ExportEncrypt)
            {
                _openExportFileAfterExportBeforeEncrypt = value;
                OnPropertyChanged(nameof(CanResetToDefaults));
            }
            QueueGeneralSettingsSave();
        }
    }

    public bool IsCliOverrideActive => _logSwitchService.IsCliOverrideActive;
    public string ClrOverrideText => _logSwitchService.IsCliOverrideActive ?
        $"(Overridden via CLI to {SelectedLogLevel})" :
        "";
    public bool CanResetToDefaults =>
        LockOnSessionLock != true ||
        LockOnMinimize != true ||
        LockOnIdleTimeout != true ||
        IdleTimeoutMinutes != (int)AppSettings.DefaultIdleTimeout.TotalMinutes ||
        ClearClipboardEnabled != true ||
        ClearClipboardSeconds != AppSettings.DefaultClearClipboardSeconds ||
        Math.Abs(QrPreviewScaleFactor - AppSettings.DefaultQrPreviewScaleFactor) > 0.001d ||
        ExportEncrypt != true ||
        _openExportFileAfterExportBeforeEncrypt != true ||
        HideSecretsByDefault != true ||
        SelectedLogLevel != AppLogLevel.Information;

    #endregion

    public ICommand SaveCommand { get; }
    public ICommand CloseCommand { get; }
    public ICommand ChangePasswordCommand { get; }
    public ICommand ResetToDefaultsCommand { get; }
    public ICommand ExportCommand { get; }
    public ICommand ImportCommand { get; }
    public ICommand OpenLogFolderCommand { get; }

    public delegate SettingsViewModel SettingsViewModelFactory(
        ICommand closeCommand,
        Action saveAction,
        Func<bool, ExportFileFormat, Task> actionExportOtps,
        Func<ImportConflictStrategy, Task> actionImportOtps);

    private ISettingsService _settingsSvc;
    IAppSettings _appSettings => _settingsSvc.Current;
    #endregion

    #region  ### CONSTRUCTOR ###

    public SettingsViewModel(ISettingsService settingsSvc,
                            IAuthorizationService authorizationService,
                            IQrPreviewService qrPreviewService,
                            ILogSwitchService logSwitchService,
                            ICommand closeCommand,
                            Action saveAction,
                            Func<bool, ExportFileFormat, Task> actionExportOtps,
                            Func<ImportConflictStrategy, Task> actionImportOtps)
    {
        _logSwitchService = logSwitchService;
        _settingsSvc = settingsSvc;
        _authorizationService = authorizationService;
        _qrPreviewService = qrPreviewService;
        _saveAction = saveAction;
        
        CloseCommand = closeCommand;
        SaveCommand = new AsyncCommand(SaveAndCloseAsync);
        ChangePasswordCommand = new AsyncCommand(ChangePasswordAsync, () => IsPasswordSelected && (!string.IsNullOrWhiteSpace(NewPassword) || !string.IsNullOrWhiteSpace(ConfirmPassword)));
        ResetToDefaultsCommand = new AsyncCommand(ResetToDefaultsAsync, () => CanResetToDefaults);
        ExportCommand = new AsyncCommand(() => actionExportOtps(ExportEncrypt, SelectedExportFormat));
        ImportCommand = new AsyncCommand(() => actionImportOtps(SelectedImportConflictOption.Strategy));
        OpenLogFolderCommand = new RelayCommand(OnOpenLogFolder);

        AvailableLogLevels = Enum.GetValues(typeof(AppLogLevel)).Cast<AppLogLevel>().ToList();
        AvailableExportFormats = [];
        UpdateAvailableExportFormats();
        AvailableImportConflictOptions =
        [
            new ImportConflictOptionItem(ImportConflictStrategy.SkipExisting, UI.ui_Settings_Import_Conflict_Skip),
            new ImportConflictOptionItem(ImportConflictStrategy.ReplaceExisting, UI.ui_Settings_Import_Conflict_Replace),
            new ImportConflictOptionItem(ImportConflictStrategy.KeepBoth, UI.ui_Settings_Import_Conflict_KeepBoth)
        ];
        _selectedImportConflictOption = AvailableImportConflictOptions.First();
        _selectedLogLevel = _logSwitchService.MinimumLevel;
 
    }

    #endregion

    public async Task LoadAsync()
    {
        _isLoadingSettings = true;
        try
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
            LockOnMinimize = _appSettings.LockOnMinimize;
            LockOnIdleTimeout = _appSettings.IdleTimeout > TimeSpan.Zero;
            IdleTimeoutMinutes = (int)Math.Max(1, _appSettings.IdleTimeout.TotalMinutes);
            ClearClipboardEnabled = _appSettings.ClearClipboardEnabled;
            ClearClipboardSeconds = _appSettings.ClearClipboardSeconds > 0 ? _appSettings.ClearClipboardSeconds : 15;
            QrPreviewScaleFactor = Math.Clamp(
                _appSettings.QrPreviewScaleFactor > 0 ? _appSettings.QrPreviewScaleFactor : AppSettings.DefaultQrPreviewScaleFactor,
                MinQrPreviewScale,
                MaxQrPreviewScale);
            _qrPreviewService.PreviewScaleFactor = QrPreviewScaleFactor;
            OpenExportFileAfterExport = _appSettings.OpenExportFileAfterExport;
            _openExportFileAfterExportBeforeEncrypt = _appSettings.OpenExportFileAfterExport;
            ExportEncrypt = _appSettings.ExportEncrypt;
            OpenExportFileAfterExport = ExportEncrypt
                ? false
                : _openExportFileAfterExportBeforeEncrypt;
            UpdateAvailableExportFormats();
            SelectedImportConflictOption = AvailableImportConflictOptions.First();
            HideSecretsByDefault = _appSettings.HideSecretsByDefault;
        }
        finally
        {
            _isLoadingSettings = false;
            OnPropertyChanged(nameof(CanResetToDefaults));
            RaiseCommandStates();
        }
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
        _appSettings.MinimumLogLevel = SelectedLogLevel;

        _appSettings.LockOnSessionLock = LockOnSessionLock;
        _appSettings.LockOnMinimize = LockOnMinimize;
        _appSettings.IdleTimeout = LockOnIdleTimeout
            ? TimeSpan.FromMinutes(Math.Max(1, IdleTimeoutMinutes))
            : TimeSpan.Zero;
        _appSettings.ClearClipboardEnabled = ClearClipboardEnabled;
        _appSettings.ClearClipboardSeconds = ClearClipboardSeconds;
        _appSettings.QrPreviewScaleFactor = Math.Clamp(
            QrPreviewScaleFactor > 0 ? QrPreviewScaleFactor : AppSettings.DefaultQrPreviewScaleFactor,
            MinQrPreviewScale,
            MaxQrPreviewScale);
        _appSettings.ExportEncrypt = ExportEncrypt;
        _appSettings.OpenExportFileAfterExport = _openExportFileAfterExportBeforeEncrypt;
        _appSettings.HideSecretsByDefault = HideSecretsByDefault;

        var saveResult = await _settingsSvc.SaveAsync();
        if (saveResult.IsFailed)
        {
            AuthError = string.Join("; ", saveResult.Errors.Select(e => e.Message));
            return;
        }

        if (SelectedLogLevel != _logSwitchService.GetLevel())
        {
            _logSwitchService.SetLevel(SelectedLogLevel);
            _logSwitchService.IsCliOverrideActive = false;
            OnPropertyChanged(nameof(IsCliOverrideActive));
            OnPropertyChanged(nameof(ClrOverrideText));
        }

        _qrPreviewService.PreviewScaleFactor = _appSettings.QrPreviewScaleFactor;

    }

    private void OnOpenLogFolder()
    {
        var path = System.IO.Path.GetDirectoryName(StringsConstants.AppLogPath);
        if (System.IO.Directory.Exists(path))
            Process.Start(new ProcessStartInfo("explorer.exe", path) { UseShellExecute = true });
    }

    private void QueueGeneralSettingsSave(int delayMs = 200)
    {
        if (_isLoadingSettings)
        {
            return;
        }

        _saveDebounceCts?.Cancel();
        _saveDebounceCts?.Dispose();
        _saveDebounceCts = new CancellationTokenSource();
        _ = SaveGeneralSettingsDebouncedAsync(_saveDebounceCts.Token, delayMs);
    }

    private async Task SaveGeneralSettingsDebouncedAsync(CancellationToken token, int delayMs)
    {
        try
        {
            await Task.Delay(delayMs, token);
            await SaveGeneralSettingsAsync();
        }
        catch (TaskCanceledException)
        {
            // expected when user changes multiple settings quickly
        }
    }

    private void QueueAuthorizationGateSave(int delayMs = 120)
    {
        if (_isLoadingSettings || _suppressAuthAutoSave)
        {
            return;
        }

        _authGateDebounceCts?.Cancel();
        _authGateDebounceCts?.Dispose();
        _authGateDebounceCts = new CancellationTokenSource();
        _ = SaveAuthorizationGateDebouncedAsync(_authGateDebounceCts.Token, delayMs);
    }

    private async Task SaveAuthorizationGateDebouncedAsync(CancellationToken token, int delayMs)
    {
        try
        {
            await Task.Delay(delayMs, token);
            await ApplyAuthorizationGateSelectionAsync();
        }
        catch (TaskCanceledException)
        {
            // expected when toggling quickly
        }
    }

    private async Task ApplyAuthorizationGateSelectionAsync()
    {
        AuthError = null;
        var selectedGate = IsHelloSelected ? AuthorizationGateKind.Hello : AuthorizationGateKind.Password;
        if (_appSettings.Authorization.Gate == selectedGate)
        {
            return;
        }

        if (selectedGate == AuthorizationGateKind.Hello)
        {
            if (!IsHelloAvailable)
            {
                AuthError = "Windows Hello is not supported on this device.";
                return;
            }

            if (!_appSettings.Authorization.HasHelloSetup)
            {
                var configureResult = await _authorizationService.ConfigureHelloAsync();
                if (configureResult != AuthorizationResult.Success)
                {
                    AuthError = "Windows Hello setup failed.";
                    return;
                }
            }
        }

        _appSettings.Authorization.Gate = selectedGate;
        var saveResult = await _settingsSvc.SaveAsync();
        if (saveResult.IsFailed)
        {
            AuthError = string.Join("; ", saveResult.Errors.Select(e => e.Message));
            _suppressAuthAutoSave = true;
            IsHelloSelected = _appSettings.Authorization.Gate == AuthorizationGateKind.Hello;
            IsPasswordSelected = _appSettings.Authorization.Gate == AuthorizationGateKind.Password;
            _suppressAuthAutoSave = false;
        }
    }

    private async Task ChangePasswordAsync()
    {
        AuthError = null;

        if (string.IsNullOrWhiteSpace(NewPassword))
        {
            AuthError = "Password cannot be empty.";
            return;
        }

        var result = await _authorizationService.ChangePasswordAsync(NewPassword, ConfirmPassword);
        if (result != AuthorizationResult.Success)
        {
            AuthError = "Passwords must match and be at least 8 characters.";
            return;
        }

        NewPassword = string.Empty;
        ConfirmPassword = string.Empty;
    }

    private async Task ResetToDefaultsAsync()
    {
        _isLoadingSettings = true;
        try
        {
            LockOnSessionLock = true;
            LockOnMinimize = true;
            LockOnIdleTimeout = true;
            IdleTimeoutMinutes = (int)AppSettings.DefaultIdleTimeout.TotalMinutes;
            ClearClipboardEnabled = true;
            ClearClipboardSeconds = AppSettings.DefaultClearClipboardSeconds;
            QrPreviewScaleFactor = AppSettings.DefaultQrPreviewScaleFactor;
            ExportEncrypt = true;
            _openExportFileAfterExportBeforeEncrypt = true;
            OpenExportFileAfterExport = false;
            HideSecretsByDefault = true;
            SelectedLogLevel = AppLogLevel.Information;
        }
        finally
        {
            _isLoadingSettings = false;
        }

        await SaveGeneralSettingsAsync();
        OnPropertyChanged(nameof(CanResetToDefaults));
        RaiseCommandStates();
    }

    private void UpdateAvailableExportFormats()
    {
        var formats = ExportEncrypt ? EncryptedExportFormats : PlainExportFormats;

        AvailableExportFormats.Clear();
        foreach (var format in formats)
        {
            AvailableExportFormats.Add(format);
        }

        if (!AvailableExportFormats.Contains(SelectedExportFormat))
        {
            SelectedExportFormat = AvailableExportFormats.First();
        }
    }

    private void RaiseCommandStates()
    {
        if (ChangePasswordCommand is AsyncCommand changePasswordCommand)
        {
            changePasswordCommand.RaiseCanExecuteChanged();
        }

        if (ResetToDefaultsCommand is AsyncCommand resetToDefaultsCommand)
        {
            resetToDefaultsCommand.RaiseCanExecuteChanged();
        }
    }

    private void OnPropertyChanged([CallerMemberName] string? name = null)
    {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));

        if (name == nameof(CanResetToDefaults) || name == nameof(NewPassword) || name == nameof(ConfirmPassword) || name == nameof(IsPasswordSelected))
        {
            RaiseCommandStates();
        }
    }
}
