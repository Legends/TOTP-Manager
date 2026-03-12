using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Input;
using TOTP.Commands;
using TOTP.Core.Enums;
using TOTP.Core.Models;
using TOTP.Core.Security.Interfaces;
using TOTP.Core.Security.Models;
using TOTP.Core.Services.Interfaces;
using TOTP.Core.Common;
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

public sealed partial class SettingsViewModel : INotifyPropertyChanged, IDisposable
{
    private const double MinQrPreviewScale = 1.0;
    private const double MaxQrPreviewScale = 6.0;

    #region ### PROPERTIES/FIELDS ###

    public event PropertyChangedEventHandler? PropertyChanged;
    private readonly IAuthorizationService _authorizationService;
    private readonly ILogSwitchService _logSwitchService;
    private readonly ISettingsAuthorizationWorkflowService _settingsAuthorizationWorkflowService;
    private readonly ISettingsPersistenceService _settingsPersistenceService;
    private readonly ISettingsTransferWorkflowService _settingsTransferWorkflowService;
    private readonly IAutoUpdateService _autoUpdateService;
    private readonly IMessageService _messageService;
    private readonly SettingsExportOptionsController _exportOptions = new();
    private readonly Action _saveAction;
    private CancellationTokenSource? _saveDebounceCts;
    private CancellationTokenSource? _authGateDebounceCts;
    private bool _isLoadingSettings;
    private bool _suppressAuthAutoSave;
    private bool _isCheckingForUpdates;
    private string _checkForUpdatesButtonText = "Check for updates";

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
            NewPasswordError = null;
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
            ConfirmPasswordError = null;
            RaiseCommandStates();
        }
    }

    private string? _newPasswordError;
    public string? NewPasswordError
    {
        get => _newPasswordError;
        set
        {
            if (string.Equals(_newPasswordError, value, StringComparison.Ordinal)) return;
            _newPasswordError = value;
            OnPropertyChanged();
        }
    }

    private string? _confirmPasswordError;
    public string? ConfirmPasswordError
    {
        get => _confirmPasswordError;
        set
        {
            if (string.Equals(_confirmPasswordError, value, StringComparison.Ordinal)) return;
            _confirmPasswordError = value;
            OnPropertyChanged();
        }
    }
    #endregion

    #region General Settings
    public bool IsCheckingForUpdates
    {
        get => _isCheckingForUpdates;
        private set
        {
            if (_isCheckingForUpdates == value)
            {
                return;
            }

            _isCheckingForUpdates = value;
            OnPropertyChanged();
        }
    }

    public string CheckForUpdatesButtonText
    {
        get => _checkForUpdatesButtonText;
        private set
        {
            if (string.Equals(_checkForUpdatesButtonText, value, StringComparison.Ordinal))
            {
                return;
            }

            _checkForUpdatesButtonText = value;
            OnPropertyChanged();
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

    public bool ExportEncrypt
    {
        get => _exportOptions.ExportEncrypt;
        set
        {
            if (!_exportOptions.SetExportEncrypt(value))
            {
                return;
            }

            OnPropertyChanged();
            OnPropertyChanged(nameof(IsExportFormatSelectionEnabled));
            OnPropertyChanged(nameof(IsOpenExportFileAfterExportOptionEnabled));
            OnPropertyChanged(nameof(OpenExportFileAfterExport));

            UpdateAvailableExportFormats();
            // Force ComboBox resync after source list/encryption mode transitions.
            OnPropertyChanged(nameof(SelectedExportFormat));
            OnPropertyChanged(nameof(CanResetToDefaults));
            QueueGeneralSettingsSave();
        }
    }

    public ExportFileFormat SelectedExportFormat
    {
        get => _exportOptions.SelectedExportFormat;
        set
        {
            if (!_exportOptions.SetSelectedExportFormat(value))
            {
                return;
            }

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
    public bool IsExportFormatSelectionEnabled => _exportOptions.IsExportFormatSelectionEnabled;
    public bool IsOpenExportFileAfterExportOptionEnabled => _exportOptions.IsOpenExportFileAfterExportOptionEnabled;

    public bool OpenExportFileAfterExport
    {
        get => _exportOptions.OpenExportFileAfterExport;
        set
        {
            if (!_exportOptions.SetOpenExportFileAfterExport(value))
            {
                return;
            }

            OnPropertyChanged();
            OnPropertyChanged(nameof(CanResetToDefaults));
            QueueGeneralSettingsSave();
        }
    }

    public bool IsCliOverrideActive => _logSwitchService.IsCliOverrideActive;
    public string ClrOverrideText => _logSwitchService.IsCliOverrideActive ?
        $"(Overridden via CLI to {SelectedLogLevel})" :
        "";
    public string RunningVersion { get; }
    public string AssemblyVersion { get; }
    public string InstallationPath { get; }
    public bool CanResetToDefaults =>
        LockOnSessionLock != true ||
        LockOnMinimize != true ||
        LockOnIdleTimeout != true ||
        IdleTimeoutMinutes != (int)AppSettings.DefaultIdleTimeout.TotalMinutes ||
        ClearClipboardEnabled != true ||
        ClearClipboardSeconds != AppSettings.DefaultClearClipboardSeconds ||
        Math.Abs(QrPreviewScaleFactor - AppSettings.DefaultQrPreviewScaleFactor) > 0.001d ||
        ExportEncrypt != true ||
        _exportOptions.OpenExportFileAfterExportBeforeEncrypt != true ||
        HideSecretsByDefault != true ||
        SelectedLogLevel != AppLogLevel.Information;

    #endregion

    public ICommand SaveCommand { get; }
    public ICommand CloseCommand { get; }
    public ICommand ChangePasswordCommand { get; }
    public ICommand ResetToDefaultsCommand { get; }
    public ICommand ExportCommand { get; }
    public ICommand ImportCommand { get; }
    public ICommand CheckForUpdatesCommand { get; }
    public ICommand OpenLogFolderCommand { get; }

    private ISettingsService _settingsSvc;
    IAppSettings _appSettings => _settingsSvc.Current;
    private bool CanSaveSettings() => !_isLoadingSettings;

    private bool CanExportSettings() =>
        !_isLoadingSettings &&
        AvailableExportFormats.Contains(SelectedExportFormat);

    private bool CanImportSettings() =>
        !_isLoadingSettings &&
        SelectedImportConflictOption != null;

    private bool CanOpenLogFolder()
    {
        var path = System.IO.Path.GetDirectoryName(StringsConstants.AppLogPath);
        return !string.IsNullOrWhiteSpace(path) && System.IO.Directory.Exists(path);
    }

    private bool CanCheckForUpdates() => !_isLoadingSettings && !IsCheckingForUpdates;
    #endregion

    #region  ### CONSTRUCTOR ###

    public SettingsViewModel(ISettingsService settingsSvc,
                            IAuthorizationService authorizationService,
                             ISettingsAuthorizationWorkflowService settingsAuthorizationWorkflowService,
                             ISettingsPersistenceService settingsPersistenceService,
                             ISettingsTransferWorkflowService settingsTransferWorkflowService,
                             IAutoUpdateService autoUpdateService,
                             IMessageService messageService,
                             ILogSwitchService logSwitchService,
                             ICommand closeCommand,
                            Action saveAction)
    {
        _logSwitchService = logSwitchService;
        _settingsSvc = settingsSvc;
        _authorizationService = authorizationService;
        _settingsAuthorizationWorkflowService = settingsAuthorizationWorkflowService;
        _settingsPersistenceService = settingsPersistenceService;
        _settingsTransferWorkflowService = settingsTransferWorkflowService;
        _autoUpdateService = autoUpdateService;
        _messageService = messageService;
        _saveAction = saveAction;
        
        CloseCommand = closeCommand;
        SaveCommand = new AsyncCommand(SaveAndCloseAsync, CanSaveSettings);
        ChangePasswordCommand = new AsyncCommand(ChangePasswordAsync, () => IsPasswordSelected && (!string.IsNullOrWhiteSpace(NewPassword) || !string.IsNullOrWhiteSpace(ConfirmPassword)));
        ResetToDefaultsCommand = new AsyncCommand(ResetToDefaultsAsync, () => CanResetToDefaults);
        ExportCommand = new AsyncCommand(() => _settingsTransferWorkflowService.ExportAsync(ExportEncrypt, SelectedExportFormat), CanExportSettings);
        ImportCommand = new AsyncCommand(() => _settingsTransferWorkflowService.ImportAsync(SelectedImportConflictOption.Strategy), CanImportSettings);
        CheckForUpdatesCommand = new AsyncCommand(CheckForUpdatesAsync, CanCheckForUpdates);
        OpenLogFolderCommand = new RelayCommand(OnOpenLogFolder, CanOpenLogFolder);

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

        var assembly = Assembly.GetEntryAssembly() ?? Assembly.GetExecutingAssembly();
        var location = assembly.Location;
        var informationalVersion = assembly.GetCustomAttribute<AssemblyInformationalVersionAttribute>()?.InformationalVersion;
        var assemblyVersion = assembly.GetName().Version?.ToString() ?? "unknown";
        var productVersion = string.Empty;

        if (!string.IsNullOrWhiteSpace(location))
        {
            var versionInfo = FileVersionInfo.GetVersionInfo(location);
            productVersion = versionInfo.ProductVersion ?? string.Empty;
        }

        RunningVersion = !string.IsNullOrWhiteSpace(productVersion)
            ? productVersion
            : !string.IsNullOrWhiteSpace(informationalVersion)
                ? informationalVersion
                : assemblyVersion;
        AssemblyVersion = assemblyVersion;
        InstallationPath = string.IsNullOrWhiteSpace(location) ? "unknown" : location;
 
    }

    #endregion

    public async Task LoadAsync()
    {
        _isLoadingSettings = true;
        try
        {
            IsHelloAvailable = await _authorizationService.IsHelloAvailableAsync();


            // Match UI Radio Buttons to Profile Gate
            IsHelloSelected = _appSettings.Authorization.Gate == AuthorizationGateKind.Hello;
            IsPasswordSelected = _appSettings.Authorization.Gate == AuthorizationGateKind.Password;

            // Fallback if Hello was saved but is no longer available on this hardware
            if (!IsHelloAvailable && IsHelloSelected)
            {
                IsHelloSelected = false;
                IsPasswordSelected = true;
            }

            ApplyGeneralSnapshot(_settingsPersistenceService.ReadCurrentGeneralSettings());
            SelectedImportConflictOption = AvailableImportConflictOptions.First();
        }
        finally
        {
            _isLoadingSettings = false;
            OnPropertyChanged(nameof(CanResetToDefaults));
            RaiseCommandStates();
        }
    }

    public void Dispose()
    {
        _saveDebounceCts?.Cancel();
        _saveDebounceCts?.Dispose();
        _saveDebounceCts = null;

        _authGateDebounceCts?.Cancel();
        _authGateDebounceCts?.Dispose();
        _authGateDebounceCts = null;
    }
}





