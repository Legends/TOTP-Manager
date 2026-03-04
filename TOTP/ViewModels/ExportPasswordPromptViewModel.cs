using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace TOTP.ViewModels;

public sealed class ExportPasswordPromptViewModel : INotifyPropertyChanged
{
    private string _title;
    private bool _useMasterPassword = true;
    private string _masterPassword = string.Empty;
    private string _customPassword = string.Empty;
    private string _confirmCustomPassword = string.Empty;
    private string _errorMessage = string.Empty;

    public event PropertyChangedEventHandler? PropertyChanged;

    public ExportPasswordPromptViewModel(string title)
    {
        _title = title;
    }

    public string Title
    {
        get => _title;
        set
        {
            if (_title == value)
            {
                return;
            }

            _title = value;
            OnPropertyChanged();
        }
    }

    public bool UseMasterPassword
    {
        get => _useMasterPassword;
        set
        {
            if (_useMasterPassword == value)
            {
                return;
            }

            _useMasterPassword = value;
            ErrorMessage = string.Empty;
            OnPropertyChanged();
            OnPropertyChanged(nameof(UseCustomPassword));
        }
    }

    public bool UseCustomPassword
    {
        get => !UseMasterPassword;
        set => UseMasterPassword = !value;
    }

    public string MasterPassword
    {
        get => _masterPassword;
        set
        {
            if (_masterPassword == value)
            {
                return;
            }

            _masterPassword = value;
            OnPropertyChanged();
        }
    }

    public string CustomPassword
    {
        get => _customPassword;
        set
        {
            if (_customPassword == value)
            {
                return;
            }

            _customPassword = value;
            OnPropertyChanged();
        }
    }

    public string ConfirmCustomPassword
    {
        get => _confirmCustomPassword;
        set
        {
            if (_confirmCustomPassword == value)
            {
                return;
            }

            _confirmCustomPassword = value;
            OnPropertyChanged();
        }
    }

    public string ErrorMessage
    {
        get => _errorMessage;
        set
        {
            if (_errorMessage == value)
            {
                return;
            }

            _errorMessage = value;
            OnPropertyChanged();
        }
    }

    private void OnPropertyChanged([CallerMemberName] string propertyName = "")
    {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }
}
