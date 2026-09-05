using System.ComponentModel;
using System.Windows.Input;

namespace TOTP.Avalonia.Desktop.Presentation;

public sealed class AccountListItemViewModel(
    Guid id,
    string issuer,
    string accountName,
    bool isRecentlyAdded = false,
    ICommand? copyCodeCommand = null) : INotifyPropertyChanged
{
    private bool _isRecentlyAdded = isRecentlyAdded;
    private string _code = string.Empty;
    private int _remainingSeconds;
    private int _periodSeconds;

    public event PropertyChangedEventHandler? PropertyChanged;

    public Guid Id { get; } = id;
    public string Issuer { get; } = issuer;
    public string AccountName { get; } = accountName;
    public ICommand? CopyCodeCommand { get; } = copyCodeCommand;

    public string Code
    {
        get => _code;
        private set
        {
            if (_code == value) return;
            _code = value;
            OnPropertyChanged(nameof(Code));
            OnPropertyChanged(nameof(DisplayCode));
            OnPropertyChanged(nameof(HasCode));
        }
    }

    public string DisplayCode => Code.Length < 2
        ? Code
        : Code.Insert(Code.Length / 2, " ");

    public bool HasCode => Code.Length > 0;

    public int RemainingSeconds
    {
        get => _remainingSeconds;
        private set
        {
            var normalized = Math.Max(0, value);
            if (_remainingSeconds == normalized) return;
            _remainingSeconds = normalized;
            OnPropertyChanged(nameof(RemainingSeconds));
        }
    }

    public int PeriodSeconds
    {
        get => _periodSeconds;
        private set
        {
            var normalized = Math.Max(0, value);
            if (_periodSeconds == normalized) return;
            _periodSeconds = normalized;
            OnPropertyChanged(nameof(PeriodSeconds));
        }
    }

    public bool IsRecentlyAdded
    {
        get => _isRecentlyAdded;
        private set
        {
            if (_isRecentlyAdded == value) return;
            _isRecentlyAdded = value;
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(IsRecentlyAdded)));
        }
    }

    public void ClearRecentlyAdded() => IsRecentlyAdded = false;

    public void UpdateCode(string code, int remainingSeconds, int periodSeconds)
    {
        Code = code;
        RemainingSeconds = Math.Max(1, remainingSeconds);
        PeriodSeconds = Math.Max(RemainingSeconds, periodSeconds);
    }

    public void Tick()
    {
        if (RemainingSeconds > 0)
            RemainingSeconds--;
    }

    public void ClearCode()
    {
        Code = string.Empty;
        RemainingSeconds = 0;
        PeriodSeconds = 0;
    }

    private void OnPropertyChanged(string propertyName) =>
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
}
