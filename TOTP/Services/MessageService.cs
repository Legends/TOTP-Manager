using TOTP.Interfaces;
using TOTP.UserControls;
using TOTP.ViewModels;

namespace TOTP.Services;



public class MessageService : IMessageService
{
    public void ShowErrorMessage(string message, string btnOkText = "Ok")
    {
        ShowMessage(message, "Error", "pack://application:,,,/TOTP.Manager;component/Assets/Icons/Wrong.png", btnOkText);
    }
    public void ShowInfoMessage(string message, string btnOkText = "Ok")
    {
        ShowMessage(message, "Info", "pack://application:,,,/TOTP.Manager;component/Assets/Icons/Info.png", btnOkText);
    }

    public void ShowWarningMessage(string message, string btnOkText = "Ok")
    {
        ShowMessage(message, "Warning", "pack://application:,,,/TOTP.Manager;component/Assets/Icons/Warning_message.png", btnOkText);
    }

    public bool ShowErrorMessageDialog(string message, string btnOkText = "Ok", string btnCancelText = "Cancel")
    {
        return ShowMessageDialog(message, "Error", "pack://application:,,,/TOTP.Manager;component/Assets/Icons/Wrong.png", btnOkText, btnCancelText);
    }
    public bool ShowInfoMessageDialog(string message, string btnOkText = "Ok", string btnCancelText = "Cancel")
    {
        return ShowMessageDialog(message, "Info", "pack://application:,,,/TOTP.Manager;component/Assets/Icons/Info.png", btnOkText, btnCancelText);
    }

    public bool ShowWarningMessageDialog(string message, string btnOkText = "Ok", string btnCancelText = "Cancel")
    {
        return ShowMessageDialog(message, "Warning", "pack://application:,,,/TOTP.Manager;component/Assets/Icons/Warning_message.png", btnOkText, btnCancelText);
    }


    public void ShowMessage(string message, string caption = "Info", string iconPath = "", string btnOkText = "Ok")
    {
        //var dialog = new Dialog(message, caption);
        //dialog.ShowDialog();

        var vm = new DialogViewModel
        {
            Caption = caption,
            Message = message,
            OkButtonText = btnOkText,
            ShowCancelButton = false,
            IconPath = iconPath // Optional
        };

        var dialog = new Dialog(vm);
        bool? result = dialog.ShowDialog();
    }

    public bool ShowMessageDialog(string message, string caption = "Info", string iconPath = "", string btnOkText = "Ok", string btnCancelText = "Cancel")
    {
        //var dialog = new Dialog(message, caption);
        //return dialog.ShowDialog() == true && dialog.Result;

        var vm = new DialogViewModel
        {
            Caption = caption,
            Message = message,
            OkButtonText = btnOkText,
            CancelButtonText = btnCancelText,
            ShowCancelButton = true,
            IconPath = iconPath// Optional
        };

        var dialog = new Dialog(vm);
        dialog.ShowDialog();
        return dialog.Result;
    }

}