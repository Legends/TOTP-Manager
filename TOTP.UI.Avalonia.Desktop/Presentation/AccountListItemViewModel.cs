using System.ComponentModel;

namespace TOTP.Avalonia.Desktop.Presentation;

public sealed class AccountListItemViewModel(
    Guid id,
    string issuer,
    string accountName,
    bool isRecentlyAdded = false) : INotifyPropertyChanged
{
    private bool _isRecentlyAdded = isRecentlyAdded;

    public event PropertyChangedEventHandler? PropertyChanged;

    public Guid Id { get; } = id;
    public string Issuer { get; } = issuer;
    public string AccountName { get; } = accountName;

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
}
