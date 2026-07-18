using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows.Input;
using TOTP.Core.Security.Interfaces;
using TOTP.Avalonia.Desktop.Localization;

namespace TOTP.Avalonia.Desktop.Presentation;

public sealed class SettingsPageViewModel : INotifyPropertyChanged
{
    private readonly ISettingsService _settingsService;
    private readonly AsyncCommand _saveCommand;
    private readonly IAvaloniaLocalizationService? _localization;
    private int _idleTimeoutMinutes;
    private bool _lockOnMinimize;
    private bool _isBusy;
    private string _message = string.Empty;
    private LanguageOption? _selectedLanguage;

    public SettingsPageViewModel(
        ISettingsService settingsService,
        IAvaloniaLocalizationService? localization = null)
    {
        _settingsService = settingsService ?? throw new ArgumentNullException(nameof(settingsService));
        _localization = localization;
        Languages = localization?.SupportedLanguages ?? [];
        _selectedLanguage = localization?.CurrentLanguage;
        _saveCommand = new AsyncCommand(SaveAsync, () => !_isBusy && _idleTimeoutMinutes is >= 0 and <= 1440);
        Reload();
    }

    public event PropertyChangedEventHandler? PropertyChanged;

    public IReadOnlyList<LanguageOption> Languages { get; }

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
        }
    }

    public bool LockOnMinimize
    {
        get => _lockOnMinimize;
        set => SetField(ref _lockOnMinimize, value);
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
        Message = string.Empty;
    }

    public async Task SaveAsync()
    {
        if (_isBusy || IdleTimeoutMinutes is < 0 or > 1440) return;

        _isBusy = true;
        _saveCommand.NotifyCanExecuteChanged();
        var previousTimeout = _settingsService.Current.IdleTimeout;
        var previousLockOnMinimize = _settingsService.Current.LockOnMinimize;
        try
        {
            _settingsService.Current.IdleTimeout = TimeSpan.FromMinutes(IdleTimeoutMinutes);
            _settingsService.Current.LockOnMinimize = LockOnMinimize;
            var result = await _settingsService.SaveAsync();
            if (result.IsSuccess)
            {
                Message = "Settings saved.";
                return;
            }

            _settingsService.Current.IdleTimeout = previousTimeout;
            _settingsService.Current.LockOnMinimize = previousLockOnMinimize;
            Message = "Settings could not be saved. Existing settings remain active.";
        }
        catch (Exception)
        {
            _settingsService.Current.IdleTimeout = previousTimeout;
            _settingsService.Current.LockOnMinimize = previousLockOnMinimize;
            Message = "Settings could not be saved safely. Existing settings remain active.";
        }
        finally
        {
            _isBusy = false;
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
