using System;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows.Input;
using TOTP.Commands;

namespace TOTP.ViewModels;

public sealed class SettingsViewModel : INotifyPropertyChanged
{
    public event PropertyChangedEventHandler? PropertyChanged;

    private bool _isHelloSelected = true;
    public bool IsHelloSelected
    {
        get => _isHelloSelected;
        set
        {
            if (_isHelloSelected == value) return;
            _isHelloSelected = value;
            OnPropertyChanged();

            if (value && _isPasswordSelected)
            {
                _isPasswordSelected = false;
                OnPropertyChanged(nameof(IsPasswordSelected));
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

            if (value && _isHelloSelected)
            {
                _isHelloSelected = false;
                OnPropertyChanged(nameof(IsHelloSelected));
            }
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
        }
    }

    private bool _clearClipboardEnabled = true;
    public bool ClearClipboardEnabled
    {
        get => _clearClipboardEnabled;
        set
        {
            if (_clearClipboardEnabled == value) return;
            _clearClipboardEnabled = value;
            OnPropertyChanged();
        }
    }

    private int _clearClipboardSeconds = 15;
    public int ClearClipboardSeconds
    {
        get => _clearClipboardSeconds;
        set
        {
            if (_clearClipboardSeconds == value) return;
            _clearClipboardSeconds = value;
            OnPropertyChanged();
        }
    }

    private bool _exportIncludeQr;
    public bool ExportIncludeQr
    {
        get => _exportIncludeQr;
        set
        {
            if (_exportIncludeQr == value) return;
            _exportIncludeQr = value;
            OnPropertyChanged();
        }
    }

    private bool _exportEncrypt = true;
    public bool ExportEncrypt
    {
        get => _exportEncrypt;
        set
        {
            if (_exportEncrypt == value) return;
            _exportEncrypt = value;
            OnPropertyChanged();
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

    // bound to SettingsView.xaml uc
    public ICommand SaveCommand { get; }
    public ICommand CloseCommand { get; }
    public ICommand ExportTestCommand { get; }

    public SettingsViewModel(ICommand closeCommand, Action saveAction, Action exportTest)
    {
        CloseCommand = closeCommand;
        SaveCommand = new RelayCommand(_ => saveAction());
        ExportTestCommand = new RelayCommand(_ => exportTest());
    }

    private void OnPropertyChanged([CallerMemberName] string? name = null)
        => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
}
