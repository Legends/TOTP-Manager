using System.Linq;
using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Effects;
using TOTP.Core.Enums;
using TOTP.Helper;
using TOTP.Resources;
using TOTP.Services.Interfaces;
using TOTP.Views;

namespace TOTP.Services;

public class MessageService(IUserMessageDialogViewModel userMessageDialogViewModel) : IMessageService
{
    public void ShowInfoMessage(string message)
    {
        ShowMessage(message, CaptionType.Info, StringsConstants.ImgUrl.ImgInfo);
    }

    public void ShowWarningMessage(string message)
    {
        ShowMessage(message, CaptionType.Warning, StringsConstants.ImgUrl.ImgWarning);
    }

    public void ShowErrorMessage(string message)
    {
        ShowMessage(message, CaptionType.Error, StringsConstants.ImgUrl.ImgError);
    }

    public bool ShowInfoMessageDialog(string message)
    {
        return ShowMessageDialog(message, CaptionType.Info, StringsConstants.ImgUrl.ImgInfo);
    }

    public bool ShowWarningMessageDialog(string message)
    {
        return ShowMessageDialog(message, CaptionType.Warning, StringsConstants.ImgUrl.ImgWarning);
    }

    public bool ShowErrorMessageDialog(string message)
    {
        return ShowMessageDialog(message, CaptionType.Error, StringsConstants.ImgUrl.ImgError);
    }

    private bool ShowUserMessageDialogCore(
      string message,
      CaptionType caption,
      string iconPath,
      string okText,
      string cancelText,
      bool showCancelButton,
      double dimOpacity)
    {
        var vm = CreateUserMessageDialogViewModel(message, caption, iconPath, okText, cancelText);

        vm.ShowCancelButton = showCancelButton;

        // keep your existing caption->icon behavior for the non-custom path
        vm.IconPath = caption switch
        {
            CaptionType.Error => StringsConstants.ImgUrl.ImgError,
            CaptionType.Warning => StringsConstants.ImgUrl.ImgWarning,
            CaptionType.Info => StringsConstants.ImgUrl.ImgInfo,
            _ => string.Empty
        };

        var owner = Application.Current.Windows.OfType<Window>().SingleOrDefault(x => x.IsActive);

        var dialog = new UserMessageDialog(vm)
        {
            Owner = owner
        };

        // If there is no owner (rare), just show normally (avoid NullRef)
        if (dialog.Owner is null)
        {
            dialog.ShowDialog();
            return dialog.Result;
        }

        var oldOpacity = dialog.Owner.Opacity;
        var oldEffect = dialog.Owner.Effect;

        try
        {
            dialog.Owner.Opacity = dimOpacity;
            dialog.Owner.Effect = new BlurEffect { Radius = 6 };
            dialog.ShowDialog();
            return dialog.Result;
        }
        finally
        {
            dialog.Owner.Opacity = oldOpacity;
            dialog.Owner.Effect = oldEffect;
        }
    }

    public bool ShowDefaultMessageDialog(string message, string btnOkText = null, string btnCancelText = null, CaptionType caption = CaptionType.Default, string iconPath = "")
    {

       return ShowUserMessageDialogCore(
            message: message,
            caption: caption,
            iconPath: iconPath,
            okText: !string.IsNullOrEmpty(btnOkText) ? btnOkText : UI.ui_btnOK,
            cancelText: !string.IsNullOrEmpty(btnCancelText) ? btnCancelText : UI.ui_btnCancel,
            showCancelButton: true,
            dimOpacity: 0.55);
    }

    public void ShowMessage(string message, CaptionType caption = CaptionType.Default, string iconPath = "")
    {
        _ = ShowUserMessageDialogCore(
            message,
            caption,
            iconPath,
            okText: UI.ui_btnOK,
            cancelText: string.Empty,
            showCancelButton: false,
            dimOpacity: 0.65);
    }

    public bool ShowMessageDialog(string message, CaptionType caption = CaptionType.Default, string iconPath = "")
    {
        return ShowUserMessageDialogCore(
            message,
            caption,
            iconPath,
            okText: UI.ui_btnOK,
            cancelText: UI.ui_btnCancel,
            showCancelButton: true,
            dimOpacity: 0.55);
    }

    private IUserMessageDialogViewModel CreateUserMessageDialogViewModel(string message, CaptionType caption, string iconPath,
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
            CaptionType.Warning => (Brushes.LightYellow, Brushes.Black),
            CaptionType.Info => (Brushes.LightSteelBlue, Brushes.White),
            _ => (Brushes.Gray, Brushes.White)
        };
        return vm;
    }

}