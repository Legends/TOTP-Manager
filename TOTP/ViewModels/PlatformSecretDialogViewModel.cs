using System;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using TOTP.Commands;
using TOTP.Core.Models;
using TOTP.Core.Services;
using TOTP.Helper;
using TOTP.Interfaces;
using TOTP.Validation;

namespace TOTP.ViewModels;

public class PlatformSecretDialogViewModel : INotifyPropertyChanged, IPlatformSecretDialogViewModel
{

    #region ### PROPS & VARS ###

    private string? _account;
    private string? _platform;
    private string? _secret;
    private ImageSource? _icon = new BitmapImage(new Uri(StringsConstants.ImgUrl.ImgLockAdd));
    private readonly IMessageService _messageService;

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

    public Guid ID { get; } = Guid.NewGuid();

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

    #endregion

    public PlatformSecretDialogViewModel(IMessageService msgService)
    {
        _messageService = msgService;
        OkCommand = new RelayCommand(ExecuteOkCommand);
        CancelCommand = new RelayCommand((_) =>
        {
            RequestClose?.Invoke(this, false);
        });
    }


    private void ExecuteOkCommand(object? parameter)
    {
        try
        {
            var (isValid, error) = SecretsManager.IsValidSecretItem(new SecretItem(ID, Platform, Secret));

            if (!isValid)
            {
                _messageService.ShowWarningMessage(ValidationMessageMapper.ToMessage(error));
                return;
            }

            RequestClose?.Invoke(this, true);
        }
        catch (Exception e)
        {
            System.Windows.Forms.MessageBox.Show(e.Message);
            throw;
        }

    }

    public event PropertyChangedEventHandler? PropertyChanged;

    private void OnPropertyChanged([CallerMemberName] string? name = null)
    {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
    }
}
