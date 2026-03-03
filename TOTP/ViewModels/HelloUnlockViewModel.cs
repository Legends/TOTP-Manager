using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using System.Windows.Input;
using TOTP.Commands;
using TOTP.Core.Security.Interfaces;
using TOTP.Core.Security.Models;
using TOTP.Security.Interfaces;

namespace TOTP.ViewModels;

public sealed class HelloUnlockViewModel : INotifyPropertyChanged
{
    #region Props and Vars
    private readonly IAuthorizationService _auth;

    public event PropertyChangedEventHandler? PropertyChanged;

    private string? _message;
    public string? Message
    {
        get => _message;
        set { _message = value; OnPropertyChanged(); }
    }

    public ICommand UnlockWithHelloCommand { get; }

    #endregion

    public HelloUnlockViewModel(IAuthorizationService auth)
    {
        _auth = auth;
        UnlockWithHelloCommand = new AsyncCommand(UnlockAsync);
    }

    private async Task UnlockAsync()
    {
        Message = null;

        var result = await _auth.TryUnlockWithHelloAsync();
        if (result == AuthorizationResult.NotAvailable)
            Message = "Windows Hello is not available.";
        else if (result != AuthorizationResult.Success)
            Message = "Hello verification failed.";
    }

    private void OnPropertyChanged([CallerMemberName] string? name = null)
        => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
}