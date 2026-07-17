using System;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using System.Windows.Input;
using TOTP.Commands;
using TOTP.Core.Security.Interfaces;
using TOTP.Core.Security.Models;
using TOTP.Core.Services.Interfaces;
using TOTP.Resources;
using TOTP.Services.Interfaces;
using TOTP.Presentation.Services.Interfaces;

namespace TOTP.ViewModels;

public sealed class UnlockViewModel : INotifyPropertyChanged
{
    #region PROPS AND VARS

    private readonly IAuthorizationService _auth;
    private readonly IMessageService _messageService;
    private bool _isOfferingHelloAfterPasswordSetup;

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

    public UnlockViewModel(
        IAuthorizationService auth,
        HelloUnlockViewModel helloVM,
        PasswordUnlockViewModel pwdVM,
        ISettingsService settingsService,
        IMessageService messageService)
    {
        _auth = auth;
        _messageService = messageService;

        HelloUnlockVM = helloVM;
        PasswordUnlockVM = pwdVM;

        ChooseHelloCommand = new AsyncCommand(ChooseHelloAsync, CanChooseHello);
        ChoosePasswordCommand = new RelayCommand(ChoosePassword, CanChoosePassword);

        _auth.State.Changed += (_, _) => SyncFromState();
        PasswordUnlockVM.PasswordConfigured += PasswordUnlockVM_PasswordConfigured;

        SyncFromState();
    }

    private void SyncFromState()
    {
        OnPropertyChanged(nameof(IsConfigured));
        OnPropertyChanged(nameof(ConfiguredGate));

        StatusMessage = null;

        if (!IsConfigured)
        {
            // First-run always establishes the password-backed DEK wrapper before optional Hello.
            PasswordUnlockVM.EnterSetupMode();
            CurrentGate = PasswordUnlockVM;
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
            StatusMessage = UI.ui_Unlock_HelloNotAvailableChoosePassword;
            return;
        }
        if (cfg != AuthorizationResult.Success)
        {
            StatusMessage = UI.ui_Unlock_HelloConfigureFailed;
            return;
        }

        // after configuring, immediately try unlocking (your requirement: gate triggers)
        var unlock = await _auth.TryUnlockWithHelloAsync();
        if (unlock != AuthorizationResult.Success)
            StatusMessage = UI.ui_Unlock_HelloVerificationFailedUsePassword;
    }

    private void ChoosePassword()
    {
        StatusMessage = null;
        PasswordUnlockVM.EnterSetupMode();
        CurrentGate = PasswordUnlockVM;
    }

    private async void PasswordUnlockVM_PasswordConfigured(object? sender, EventArgs e)
    {
        await OfferHelloAfterPasswordSetupAsync();
    }

    private async Task OfferHelloAfterPasswordSetupAsync()
    {
        if (_isOfferingHelloAfterPasswordSetup || !_auth.State.IsUnlocked)
        {
            return;
        }

        _isOfferingHelloAfterPasswordSetup = true;
        try
        {
            var isHelloAvailable = await _auth.IsHelloAvailableAsync();
            if (!isHelloAvailable)
            {
                _messageService.ShowWarning(UI.ui_EnableHelloAfterPasswordSetup_NotAvailable);
                return;
            }

            var enableHello = _messageService.ConfirmInfo(
                UI.ui_EnableHelloAfterPasswordSetup_Message,
                UI.ui_EnableHelloAfterPasswordSetup_Enable,
                UI.ui_EnableHelloAfterPasswordSetup_NotNow);

            if (!enableHello)
            {
                return;
            }

            var result = await _auth.ConfigureHelloAsync();
            if (result == AuthorizationResult.Success)
            {
                _messageService.ShowSuccess(UI.ui_EnableHelloAfterPasswordSetup_Success);
                return;
            }

            _messageService.ShowWarning(UI.ui_EnableHelloAfterPasswordSetup_Failed);
        }
        finally
        {
            _isOfferingHelloAfterPasswordSetup = false;
        }
    }

    private void OnPropertyChanged([CallerMemberName] string? name = null)
        => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
}
