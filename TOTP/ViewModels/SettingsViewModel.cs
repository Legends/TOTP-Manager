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
            if (Set(ref _isHelloSelected, value))
            {
                if (value) IsPasswordSelected = false;
            }
        }
    }

    private bool _isPasswordSelected;
    public bool IsPasswordSelected
    {
        get => _isPasswordSelected;
        set
        {
            if (Set(ref _isPasswordSelected, value))
            {
                if (value) IsHelloSelected = false;
            }
        }
    }

    private bool _lockOnSessionLock = true;
    public bool LockOnSessionLock
    {
        get => _lockOnSessionLock;
        set => Set(ref _lockOnSessionLock, value);
    }

    private bool _clearClipboardEnabled = true;
    public bool ClearClipboardEnabled
    {
        get => _clearClipboardEnabled;
        set => Set(ref _clearClipboardEnabled, value);
    }

    private int _clearClipboardSeconds = 15;
    public int ClearClipboardSeconds
    {
        get => _clearClipboardSeconds;
        set => Set(ref _clearClipboardSeconds, value);
    }

    private bool _exportIncludeQr;
    public bool ExportIncludeQr
    {
        get => _exportIncludeQr;
        set => Set(ref _exportIncludeQr, value);
    }

    private bool _exportEncrypt = true;
    public bool ExportEncrypt
    {
        get => _exportEncrypt;
        set => Set(ref _exportEncrypt, value);
    }

    private bool _hideSecretsByDefault = true;
    public bool HideSecretsByDefault
    {
        get => _hideSecretsByDefault;
        set => Set(ref _hideSecretsByDefault, value);
    }

    private string? _authError;
    public string? AuthError
    {
        get => _authError;
        set
        {
            if (Set(ref _authError, value))
                OnPropertyChanged(nameof(HasAuthError));
        }
    }

    public bool HasAuthError => !string.IsNullOrWhiteSpace(AuthError);

    // bound to SettingsView.xaml uc
    public ICommand SaveCommand { get; }
    public ICommand CloseCommand { get; }
    public ICommand ExportTestCommand { get; }

    public SettingsViewModel(ICommand cmdClose, Action save, Action exportTest)
    {
        CloseCommand = cmdClose; //new RelayCommand(CloseSWMOdelMethod(close));
        SaveCommand = new RelayCommand(_ => save());
        ExportTestCommand = new RelayCommand(_ => exportTest());
    }

    //private static Action<object?> CloseSWMOdelMethod(Action close)
    //{
    //    return _ => close();
    //}

    private bool Set<T>(ref T field, T value, [CallerMemberName] string? name = null)
    {
        if (Equals(field, value)) return false;
        field = value;
        OnPropertyChanged(name);
        return true;
    }

    private void OnPropertyChanged([CallerMemberName] string? name = null)
        => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
}
