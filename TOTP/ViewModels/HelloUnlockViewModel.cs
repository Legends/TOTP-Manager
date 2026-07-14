using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using System.Windows.Input;
using TOTP.Commands;
using TOTP.Core.Security.Interfaces;
using TOTP.Core.Security.Models;
using TOTP.Resources;
using TOTP.Presentation.Services.Interfaces;

namespace TOTP.ViewModels;

public sealed class HelloUnlockViewModel : INotifyPropertyChanged
{
    #region Props and Vars
    private readonly IAuthorizationService _auth;
    private readonly IMainViewSessionController? _sessionController;
    private bool _isVerifyingLocally;

    public event PropertyChangedEventHandler? PropertyChanged;

    private string? _message;
    public string? Message
    {
        get => _message;
        set
        {
            _message = value;
            OnPropertyChanged();
            OnPropertyChanged(nameof(HasMessage));
        }
    }

    public bool HasMessage => !string.IsNullOrWhiteSpace(Message);
    public bool IsVerifying => _isVerifyingLocally ||
        _sessionController?.SessionState == AppSessionLockState.Unlocking;

    public ICommand UnlockCommand { get; }
    public ICommand UnlockWithHelloCommand => UnlockCommand;

    #endregion

    public HelloUnlockViewModel(
        IAuthorizationService auth,
        IMainViewSessionController? sessionController = null)
    {
        _auth = auth;
        _sessionController = sessionController;
        UnlockCommand = new AsyncCommand(UnlockAsync, CanUnlockWithHello);

        _auth.State.Changed += (_, _) =>
        {
            if (UnlockCommand is AsyncCommand asyncCommand)
            {
                asyncCommand.RaiseCanExecuteChanged();
            }
        };

        if (_sessionController != null)
        {
            _sessionController.SessionStateChanged += (_, _) =>
            {
                OnPropertyChanged(nameof(IsVerifying));
                if (UnlockCommand is AsyncCommand asyncCommand)
                {
                    asyncCommand.RaiseCanExecuteChanged();
                }
            };
        }
    }

    private bool CanUnlockWithHello() => !_auth.State.IsUnlocked && !IsVerifying;

    private async Task UnlockAsync()
    {
        Message = null;
        _isVerifyingLocally = true;
        OnPropertyChanged(nameof(IsVerifying));
        try
        {
            var result = await _auth.TryUnlockWithHelloAsync();
            if (result == AuthorizationResult.NotAvailable)
                Message = UI.ui_HelloUnlock_NotAvailable;
            else if (result == AuthorizationResult.Cancelled)
                Message = UI.ResourceManager.GetString("ui_HelloUnlock_Cancelled");
            else if (result != AuthorizationResult.Success)
                Message = UI.ui_HelloUnlock_VerificationFailed;
        }
        finally
        {
            _isVerifyingLocally = false;
            OnPropertyChanged(nameof(IsVerifying));
        }
    }

    private void OnPropertyChanged([CallerMemberName] string? name = null)
        => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
}

