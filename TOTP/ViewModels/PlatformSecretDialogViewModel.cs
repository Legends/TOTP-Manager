using System;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using TOTP.Commands;
using TOTP.Helper;
using TOTP.Interfaces;
using TOTP.Resources;
using TOTP.Services; // Assuming you have a message service interface here

namespace TOTP.ViewModels;

public class PlatformSecretDialogViewModel : INotifyPropertyChanged, IPlatformSecretDialogViewModel
{
    private string? _account;
    private string? _platform;
    private string? _secret;
    private ImageSource? _icon = new BitmapImage(new Uri(StringsConstants.ImgLockAdd));
    private readonly IMessageService _messageService;

    public PlatformSecretDialogViewModel(IMessageService msgService)
    {
        _messageService = msgService;
        OkCommand = new RelayCommand(ExecuteOkCommand);
        CancelCommand = new RelayCommand((_) =>
        {
            RequestClose?.Invoke(this, false);
        });
    }

    public ICommand OkCommand { get; }
    public ICommand CancelCommand { get; }

    public ImageSource? Icon
    {
        get => _icon;
        set
        {
            _icon = value;
            OnPropertyChanged();
        }
    }

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

    public event EventHandler<bool>? RequestClose;

    private void ExecuteOkCommand(object? parameter)
    {

        if (string.IsNullOrWhiteSpace(Platform) || string.IsNullOrWhiteSpace(Secret))
        {
            _messageService.ShowInfoMessage(UI.msg_PlatformSecretNotEmpty);
            return;
        }

        if (!SecretsManager.IsValidBase32Format(Secret))
        {
            _messageService.ShowInfoMessage(UI.msg_SecretInvalidFormat);
            return;
        }


        RequestClose?.Invoke(this, true);
    }

    public event PropertyChangedEventHandler? PropertyChanged;

    private void OnPropertyChanged([CallerMemberName] string? name = null)
    {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
    }
}
