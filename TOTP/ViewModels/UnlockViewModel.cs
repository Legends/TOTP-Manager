using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using System.Windows.Input;
using TOTP.Commands;
using TOTP.Core.Security.Interfaces;
using TOTP.Core.Security.Models;
using TOTP.Security.Interfaces;

namespace TOTP.ViewModels;

public sealed class UnlockViewModel : INotifyPropertyChanged
{
    #region PROPS AND VARS

    private readonly IAuthorizationService _auth;

    public event PropertyChangedEventHandler? PropertyChanged;

    public bool HasSelectedSetupGate => CurrentGate != null;

    /// <summary>
    /// A gate has been chosen Pwd or Hello
    /// </summary>
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
            _currentGate = value;
            OnPropertyChanged();
            OnPropertyChanged(nameof(HasSelectedSetupGate));
            RaiseGateCommandStates();
        }
    }

    public HelloUnlockViewModel HelloUnlockVM { get; }
    public PasswordUnlockViewModel PasswordUnlockVM { get; }

    public ICommand ChooseHelloCommand { get; }
    public ICommand ChoosePasswordCommand { get; }

    #endregion

    public UnlockViewModel(IAuthorizationService auth, HelloUnlockViewModel helloVM, PasswordUnlockViewModel pwdVM, ISettingsService settingsService)
    {
        _auth = auth;

        HelloUnlockVM = helloVM;
        PasswordUnlockVM = pwdVM;

        ChooseHelloCommand = new AsyncCommand(ChooseHelloAsync, CanChooseHello);
        ChoosePasswordCommand = new RelayCommand(ChoosePassword, CanChoosePassword);

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
            AuthorizationGateKind.Hello => HelloUnlockVM,
            AuthorizationGateKind.Password => PasswordUnlockVM,
            _ => null
        };

        RaiseGateCommandStates();
    }

    private bool CanChooseHello() => !IsConfigured && CurrentGate is null;

    private bool CanChoosePassword() => !IsConfigured && CurrentGate is null;

    private void RaiseGateCommandStates()
    {
        if (ChooseHelloCommand is AsyncCommand chooseHelloCommand)
        {
            chooseHelloCommand.RaiseCanExecuteChanged();
        }

        if (ChoosePasswordCommand is RelayCommand choosePasswordCommand)
        {
            choosePasswordCommand.RaiseCanExecuteChanged();
        }
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
        PasswordUnlockVM.EnterSetupMode();
        CurrentGate = PasswordUnlockVM;
    }

    private void OnPropertyChanged([CallerMemberName] string? name = null)
        => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
}
