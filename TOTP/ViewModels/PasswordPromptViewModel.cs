using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace TOTP.ViewModels;

public class PasswordPromptViewModel : INotifyPropertyChanged
{
    #region ### PROPERTIES & FIELDS ###

    private string _password = string.Empty;
    private string _title = string.Empty;
    private string _message = string.Empty;

    public event PropertyChangedEventHandler? PropertyChanged;

    public string Title
    {
        get => _title;
        set
        {
            if (_title != value)
            {
                _title = value;
                OnPropertyChanged();
            }
        }
    }

    public string Message
    {
        get => _message;
        set
        {
            if (_message != value)
            {
                _message = value;
                OnPropertyChanged();
            }
        }
    }

    public string Password
    {
        get => _password;
        set
        {
            if (_password != value)
            {
                _password = value;
                OnPropertyChanged();
            }
        }
    }
    #endregion

    public PasswordPromptViewModel(string title, string message)
    {
        _title = title;
        _message = message;
    }

    protected void OnPropertyChanged([CallerMemberName] string propertyName = null!)
    {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }
}