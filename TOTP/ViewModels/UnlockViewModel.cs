using System;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using System.Windows.Input;
using TOTP.Commands;
using TOTP.Security;

namespace TOTP.ViewModels;

public sealed class UnlockViewModel : INotifyPropertyChanged
{

    #region PROPS AND VARS

    private readonly IAuthorizationService _auth;

    public event PropertyChangedEventHandler? PropertyChanged;

    public bool HasSelectedSetupGate => CurrentGate != null;

    public bool IsConfigured => _auth.State.IsConfigured;
    public AuthorizationGateKind ConfiguredGate => _auth.State.ConfiguredGate;

    private string? _statusMessage;
    public string? StatusMessage
    {
        get => _statusMessage;
        set { _statusMessage = value; OnPropertyChanged(); }
    }

    // Host picks which gate VM is displayed (Hello or Password)
    private object? _currentGate;
    public object? CurrentGate
    {
        get => _currentGate;
        private set
        {
            _currentGate = value; OnPropertyChanged(); OnPropertyChanged(nameof(HasSelectedSetupGate));
        }
    }

    public HelloUnlockViewModel HelloVmUnlockViewModel { get; }
    public PasswordUnlockViewModel Password { get; }

    public ICommand ChooseHelloCommand { get; }
    public ICommand ChoosePasswordCommand { get; }

    #endregion

    public UnlockViewModel(IAuthorizationService auth, HelloUnlockViewModel helloVM, PasswordUnlockViewModel pwdVM)
    {
        _auth = auth;

        HelloVmUnlockViewModel = helloVM;
        Password = pwdVM;

        ChooseHelloCommand = new AsyncCommand(ChooseHelloAsync);
        ChoosePasswordCommand = new RelayCommand(ChoosePassword);

        _auth.State.Changed += (_, _) => SyncFromState();

        SyncFromState();
    }

    private void SyncFromState()
    {
        OnPropertyChanged(nameof(IsConfigured));
        OnPropertyChanged(nameof(ConfiguredGate));

        StatusMessage = null;

        if (!IsConfigured)
        {
            // first-run: show setup chooser in UnlockView (host view)
            CurrentGate = null;
            return;
        }

        // configured: show the configured gate
        CurrentGate = ConfiguredGate switch
        {
            AuthorizationGateKind.WindowsHello => HelloVmUnlockViewModel,
            AuthorizationGateKind.Password => Password,
            _ => null
        };
    }

    private async Task ChooseHelloAsync()
    {
        StatusMessage = null;

        var cfg = await _auth.ConfigureHelloAsync();
        if (cfg == AuthorizationResult.NotAvailable)
        {
            StatusMessage = "Windows Hello is not available on this device/account. Choose Password.";
            return;
        }
        if (cfg != AuthorizationResult.Success)
        {
            StatusMessage = "Failed to configure Windows Hello.";
            return;
        }

        // after configuring, immediately try unlocking (your requirement: gate triggers)
        var unlock = await _auth.TryUnlockWithHelloAsync();
        if (unlock != AuthorizationResult.Success)
            StatusMessage = "Hello verification failed. Try again or use Password if configured.";
    }

    private void ChoosePassword()
    {
        StatusMessage = null;
        Password.EnterSetupMode();     // shows confirm field etc.
        CurrentGate = Password;
    }

    private void OnPropertyChanged([CallerMemberName] string? name = null)
        => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
}
