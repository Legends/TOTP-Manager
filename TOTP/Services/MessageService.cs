using System.Linq;
using System.Windows;
using System.Windows.Media;
using TOTP.Enums;
using TOTP.Interfaces;
using TOTP.UserControls;

namespace TOTP.Services;

public class MessageService(IUserMessageDialogViewModel userMessageDialogViewModel) : IMessageService
{
    public void ShowErrorMessage(string message, string btnOkText = "Ok")
    {
        ShowMessage(message, CaptionType.Error, "pack://application:,,,/TOTP.Manager;component/Assets/Icons/Wrong.png", btnOkText);
    }
    public void ShowInfoMessage(string message, string btnOkText = "Ok")
    {
        ShowMessage(message, CaptionType.Info, "pack://application:,,,/TOTP.Manager;component/Assets/Icons/Info.png", btnOkText);
    }

    public void ShowWarningMessage(string message, string btnOkText = "Ok")
    {
        ShowMessage(message, CaptionType.Warning, "pack://application:,,,/TOTP.Manager;component/Assets/Icons/Warning_message.png", btnOkText);
    }

    public bool ShowErrorMessageDialog(string message, string btnOkText = "Ok", string btnCancelText = "Cancel")
    {
        return ShowMessageDialog(message, CaptionType.Error, "pack://application:,,,/TOTP.Manager;component/Assets/Icons/Wrong.png", btnOkText, btnCancelText);
    }
    public bool ShowInfoMessageDialog(string message, string btnOkText = "Ok", string btnCancelText = "Cancel")
    {
        return ShowMessageDialog(message, CaptionType.Info, "pack://application:,,,/TOTP.Manager;component/Assets/Icons/Info.png", btnOkText, btnCancelText);
    }

    public bool ShowWarningMessageDialog(string message, string btnOkText = "Ok", string btnCancelText = "Cancel")
    {
        return ShowMessageDialog(message, CaptionType.Warning, "pack://application:,,,/TOTP.Manager;component/Assets/Icons/Warning-Message.png", btnOkText, btnCancelText);
    }

    public void ShowMessage(string message, CaptionType caption = CaptionType.Default, string iconPath = "", string btnOkText = "Ok")
    {
        var vm = CreateViewModel(message, caption, iconPath, btnOkText, string.Empty);
        vm.ShowCancelButton = false;

        var dialog = new UserMessageDialog(vm)
        {
            Owner = Application.Current.Windows.OfType<Window>().SingleOrDefault(x => x.IsActive)
        };
        dialog.ShowDialog();
    }

    public bool ShowMessageDialog(string message, CaptionType caption = CaptionType.Default, string iconPath = "", string btnOkText = "Ok", string btnCancelText = "Cancel")
    {
        var vm = CreateViewModel(message, caption, iconPath, btnOkText, btnCancelText);

        var dialog = new UserMessageDialog(vm)
        {
            Owner = Application.Current.Windows.OfType<Window>().SingleOrDefault(x => x.IsActive)
        };
        dialog.ShowDialog();
        return dialog.Result;
    }

    private IUserMessageDialogViewModel CreateViewModel(string message, CaptionType caption, string iconPath,
        string btnOkText, string btnCancelText)
    {
        var vm = userMessageDialogViewModel;
        vm.Caption = caption;
        vm.Message = message;
        vm.OkButtonText = btnOkText;
        vm.CancelButtonText = btnCancelText;
        vm.ShowCancelButton = true;
        vm.IconPath = iconPath; // Optional

        (vm.TitleBarBackground, vm.TitleBarForeground) = caption switch
        {
            CaptionType.Error => (Brushes.Red, Brushes.White),
            CaptionType.Warning => (Brushes.Yellow, Brushes.Black),
            CaptionType.Info => (Brushes.LightSkyBlue, Brushes.White),
            _ => (Brushes.Gray, Brushes.White)
        };
        return vm;
    }

}