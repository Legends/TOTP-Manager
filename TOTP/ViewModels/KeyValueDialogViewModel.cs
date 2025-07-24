using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace TOTP.ViewModels;

public class KeyValueDialogViewModel : INotifyPropertyChanged
{
    private string? _account;
    private string? _platform;
    private string? _secret;

    public string? Platform
    {
        get => _platform;
        set
        {
            _platform = value;
            OnPropertyChanged();
        }
    }

    public string? Secret
    {
        get => _secret;
        set
        {
            _secret = value;
            OnPropertyChanged();
        }
    }

    public string? Account
    {
        get => _account;
        set
        {
            _account = value;
            OnPropertyChanged();
        }
    }

    public event PropertyChangedEventHandler? PropertyChanged;

    private void OnPropertyChanged([CallerMemberName] string? name = null)
    {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
    }
}