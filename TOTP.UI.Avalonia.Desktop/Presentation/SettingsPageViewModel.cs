using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows.Input;
using TOTP.Core.Security.Interfaces;
using TOTP.Avalonia.Desktop.Localization;
using TOTP.Core.Enums;
using TOTP.Core.Services.Interfaces;
using System.Reflection;

namespace TOTP.Avalonia.Desktop.Presentation;

public sealed class SettingsPageViewModel : INotifyPropertyChanged
{
    private readonly ISettingsService _settingsService;
    private readonly AsyncCommand _saveCommand;
    private readonly IAvaloniaLocalizationService? _localization;
    private readonly IPlatformApplicationPaths? _applicationPaths;
    private readonly IPlatformFolderLauncher? _folderLauncher;
    private readonly AsyncCommand _openLogFolderCommand;
    private int _idleTimeoutMinutes;
    private bool _lockOnMinimize;
    private bool _lockOnSessionLock;
    private bool _clearClipboardEnabled;
    private int _clearClipboardSeconds;
    private decimal _qrPreviewScaleFactor;
    private bool _exportEncrypt;
    private bool _openExportFileAfterExport;
    private bool _hideSecretsByDefault;
    private AppLogLevel _minimumLogLevel;
    private bool _isBusy;
    private string _message = string.Empty;
    private LanguageOption? _selectedLanguage;

    public SettingsPageViewModel(
        ISettingsService settingsService,
        IAvaloniaLocalizationService? localization = null,
        IPlatformApplicationPaths? applicationPaths = null,
        IPlatformFolderLauncher? folderLauncher = null)
    {
        _settingsService = settingsService ?? throw new ArgumentNullException(nameof(settingsService));
        _localization = localization;
        _applicationPaths = applicationPaths;
        _folderLauncher = folderLauncher;
        Languages = localization?.SupportedLanguages ?? [];
        LogLevels = Enum.GetValues<AppLogLevel>();
        _selectedLanguage = localization?.CurrentLanguage;
        _saveCommand = new AsyncCommand(SaveAsync, () =>
            !_isBusy
            && _idleTimeoutMinutes is >= 0 and <= 1440
            && _clearClipboardSeconds is >= 1 and <= 300
            && _qrPreviewScaleFactor is >= 1.0m and <= 6.0m
            && _qrPreviewScaleFactor * 2 == decimal.Truncate(_qrPreviewScaleFactor * 2));
        _openLogFolderCommand = new AsyncCommand(
            OpenLogFolderAsync,
            () => !_isBusy && _applicationPaths is not null && _folderLauncher is not null);
        VersionText = typeof(SettingsPageViewModel).Assembly
            .GetCustomAttribute<AssemblyInformationalVersionAttribute>()?
            .InformationalVersion
            ?? typeof(SettingsPageViewModel).Assembly.GetName().Version?.ToString()
            ?? "unknown";
        Reload();
    }

    public event PropertyChangedEventHandler? PropertyChanged;
    public event EventHandler? SettingsSaved;

    public IReadOnlyList<LanguageOption> Languages { get; }
    public IReadOnlyList<AppLogLevel> LogLevels { get; }
    public string VersionText { get; }
    public ICommand OpenLogFolderCommand => _openLogFolderCommand;

    public LanguageOption? SelectedLanguage
    {
        get => _selectedLanguage;
        set
        {
            if (!SetField(ref _selectedLanguage, value) || value is null) return;
            _localization?.ApplyCulture(value.CultureName);
        }
    }

    public int IdleTimeoutMinutes
    {
        get => _idleTimeoutMinutes;
        set
        {
            if (!SetField(ref _idleTimeoutMinutes, value)) return;
            _saveCommand.NotifyCanExecuteChanged();
            _openLogFolderCommand.NotifyCanExecuteChanged();
        }
    }

    public bool LockOnMinimize
    {
        get => _lockOnMinimize;
        set => SetField(ref _lockOnMinimize, value);
    }

    public bool LockOnSessionLock
    {
        get => _lockOnSessionLock;
        set => SetField(ref _lockOnSessionLock, value);
    }

    public bool ClearClipboardEnabled
    {
        get => _clearClipboardEnabled;
        set => SetField(ref _clearClipboardEnabled, value);
    }

    public int ClearClipboardSeconds
    {
        get => _clearClipboardSeconds;
        set
        {
            if (!SetField(ref _clearClipboardSeconds, value)) return;
            _saveCommand.NotifyCanExecuteChanged();
        }
    }

    public decimal QrPreviewScaleFactor
    {
        get => _qrPreviewScaleFactor;
        set
        {
            if (!SetField(ref _qrPreviewScaleFactor, value)) return;
            _saveCommand.NotifyCanExecuteChanged();
        }
    }

    public bool ExportEncrypt
    {
        get => _exportEncrypt;
        set => SetField(ref _exportEncrypt, value);
    }

    public bool OpenExportFileAfterExport
    {
        get => _openExportFileAfterExport;
        set => SetField(ref _openExportFileAfterExport, value);
    }

    public bool HideSecretsByDefault
    {
        get => _hideSecretsByDefault;
        set => SetField(ref _hideSecretsByDefault, value);
    }

    public AppLogLevel MinimumLogLevel
    {
        get => _minimumLogLevel;
        set => SetField(ref _minimumLogLevel, value);
    }

    public string Message
    {
        get => _message;
        private set => SetField(ref _message, value);
    }

    public ICommand SaveCommand => _saveCommand;

    public void Reload()
    {
        IdleTimeoutMinutes = (int)Math.Clamp(
            Math.Round(_settingsService.Current.IdleTimeout.TotalMinutes),
            0,
            1440);
        LockOnMinimize = _settingsService.Current.LockOnMinimize;
        LockOnSessionLock = _settingsService.Current.LockOnSessionLock;
        ClearClipboardEnabled = _settingsService.Current.ClearClipboardEnabled;
        ClearClipboardSeconds = _settingsService.Current.ClearClipboardSeconds;
        QrPreviewScaleFactor = (decimal)_settingsService.Current.QrPreviewScaleFactor;
        ExportEncrypt = _settingsService.Current.ExportEncrypt;
        OpenExportFileAfterExport = _settingsService.Current.OpenExportFileAfterExport;
        HideSecretsByDefault = _settingsService.Current.HideSecretsByDefault;
        MinimumLogLevel = _settingsService.Current.MinimumLogLevel;
        Message = string.Empty;
    }

    public async Task SaveAsync()
    {
        if (_isBusy
            || IdleTimeoutMinutes is < 0 or > 1440
            || ClearClipboardSeconds is < 1 or > 300
            || QrPreviewScaleFactor is < 1.0m or > 6.0m
            || QrPreviewScaleFactor * 2 != decimal.Truncate(QrPreviewScaleFactor * 2)) return;

        _isBusy = true;
        _saveCommand.NotifyCanExecuteChanged();
        _openLogFolderCommand.NotifyCanExecuteChanged();
        var previous = TOTP.Core.Models.AppPreferencesMapper.FromSettings(_settingsService.Current);
        try
        {
            _settingsService.Current.IdleTimeout = TimeSpan.FromMinutes(IdleTimeoutMinutes);
            _settingsService.Current.LockOnMinimize = LockOnMinimize;
            _settingsService.Current.LockOnSessionLock = LockOnSessionLock;
            _settingsService.Current.ClearClipboardEnabled = ClearClipboardEnabled;
            _settingsService.Current.ClearClipboardSeconds = ClearClipboardSeconds;
            _settingsService.Current.QrPreviewScaleFactor = (double)QrPreviewScaleFactor;
            _settingsService.Current.ExportEncrypt = ExportEncrypt;
            _settingsService.Current.OpenExportFileAfterExport = OpenExportFileAfterExport;
            _settingsService.Current.HideSecretsByDefault = HideSecretsByDefault;
            _settingsService.Current.MinimumLogLevel = MinimumLogLevel;
            if (SelectedLanguage is not null)
                _settingsService.Current.CultureName = SelectedLanguage.CultureName;
            var result = await _settingsService.SaveAsync();
            if (result.IsSuccess)
            {
                Message = "Settings saved.";
                SettingsSaved?.Invoke(this, EventArgs.Empty);
                return;
            }

            TOTP.Core.Models.AppPreferencesMapper.ApplyTo(previous, _settingsService.Current);
            Message = "Settings could not be saved. Existing settings remain active.";
        }
        catch (Exception)
        {
            TOTP.Core.Models.AppPreferencesMapper.ApplyTo(previous, _settingsService.Current);
            Message = "Settings could not be saved safely. Existing settings remain active.";
        }
        finally
        {
            _isBusy = false;
            _saveCommand.NotifyCanExecuteChanged();
            _openLogFolderCommand.NotifyCanExecuteChanged();
        }
    }

    public async Task OpenLogFolderAsync()
    {
        if (_isBusy || _applicationPaths is null || _folderLauncher is null) return;
        _isBusy = true;
        _openLogFolderCommand.NotifyCanExecuteChanged();
        _saveCommand.NotifyCanExecuteChanged();
        try
        {
            var opened = await _folderLauncher.OpenFolderAsync(_applicationPaths.LogDirectory);
            Message = opened.IsSuccess
                ? "Log folder opened."
                : "The log folder could not be opened.";
        }
        catch (Exception)
        {
            Message = "The log folder could not be opened safely.";
        }
        finally
        {
            _isBusy = false;
            _openLogFolderCommand.NotifyCanExecuteChanged();
            _saveCommand.NotifyCanExecuteChanged();
        }
    }

    private bool SetField<T>(ref T field, T value, [CallerMemberName] string? propertyName = null)
    {
        if (EqualityComparer<T>.Default.Equals(field, value)) return false;
        field = value;
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        return true;
    }
}
