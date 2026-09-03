using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace TOTP.Avalonia.Mobile.Presentation;

public sealed class MobileAccountItem(
    Guid id,
    string issuer,
    string accountName) : INotifyPropertyChanged
{
    private string _code = string.Empty;
    private int _remainingSeconds;
    private int _periodSeconds = 30;

    public event PropertyChangedEventHandler? PropertyChanged;

    public Guid Id { get; } = id;
    public string Issuer { get; } = issuer;
    public string AccountName { get; } = accountName;
    public bool HasAccountName => AccountName.Length > 0;
    public string Code => _code;
    public string DisplayCode => FormatCode(_code);
    public int RemainingSeconds => _remainingSeconds;
    public int PeriodSeconds => _periodSeconds;

    public string DisplayName => HasAccountName
        ? $"{Issuer} · {AccountName}"
        : Issuer;

    internal void UpdateCode(string code, int remainingSeconds, int periodSeconds)
    {
        _code = code;
        _remainingSeconds = Math.Max(1, remainingSeconds);
        _periodSeconds = Math.Max(_remainingSeconds, periodSeconds);
        NotifyCodeChanged();
    }

    internal void Tick()
    {
        if (_remainingSeconds <= 0) return;
        _remainingSeconds--;
        OnPropertyChanged(nameof(RemainingSeconds));
    }

    internal void ClearCode()
    {
        _code = string.Empty;
        _remainingSeconds = 0;
        _periodSeconds = 30;
        NotifyCodeChanged();
    }

    private static string FormatCode(string code)
    {
        if (code.Length < 2) return code;
        var midpoint = code.Length / 2;
        return $"{code[..midpoint]} {code[midpoint..]}";
    }

    private void NotifyCodeChanged()
    {
        OnPropertyChanged(nameof(Code));
        OnPropertyChanged(nameof(DisplayCode));
        OnPropertyChanged(nameof(RemainingSeconds));
        OnPropertyChanged(nameof(PeriodSeconds));
    }

    private void OnPropertyChanged([CallerMemberName] string? propertyName = null) =>
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
}
