using System.Linq;
using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Effects;
using TOTP.Core.Enums;
using TOTP.Helper;
using TOTP.Interfaces;
using TOTP.Resources;
using TOTP.Views;

namespace TOTP.Services;

public class MessageService(IUserMessageDialogViewModel userMessageDialogViewModel) : IMessageService
{
    public void ShowErrorMessage(string message)
    {
        ShowMessage(message, CaptionType.Error, StringsConstants.ImgUrl.ImgError);
    }
    public void ShowInfoMessage(string message)
    {
        ShowMessage(message, CaptionType.Info, StringsConstants.ImgUrl.ImgInfo);
    }

    public void ShowWarningMessage(string message)
    {
        ShowMessage(message, CaptionType.Warning, StringsConstants.ImgUrl.ImgWarning);
    }

    public bool ShowErrorMessageDialog(string message)
    {
        return ShowMessageDialog(message, CaptionType.Error, StringsConstants.ImgUrl.ImgError);
    }
    public bool ShowInfoMessageDialog(string message)
    {

        return ShowMessageDialog(message, CaptionType.Info, StringsConstants.ImgUrl.ImgInfo);
    }

    public bool ShowWarningMessageDialog(string message)
    {
        return ShowMessageDialog(message, CaptionType.Warning, StringsConstants.ImgUrl.ImgWarning);
    }

    public void ShowMessage(string message, CaptionType caption = CaptionType.Default, string iconPath = "")
    {
        var vm = CreateUserMessageDialogViewModel(message, caption, iconPath, UI.ui_btnOK, string.Empty);
        vm.ShowCancelButton = false;

        vm.IconPath = caption switch
        {
            CaptionType.Error => StringsConstants.ImgUrl.ImgError,
            CaptionType.Warning => StringsConstants.ImgUrl.ImgWarning,
            CaptionType.Info => StringsConstants.ImgUrl.ImgInfo,
            _ => string.Empty
        };

        var dialog = new UserMessageDialog(vm)
        {
            Owner = Application.Current.Windows.OfType<Window>().SingleOrDefault(x => x.IsActive)
        };
        var oldOpacity = dialog.Owner.Opacity;
        var oldEffect = dialog.Owner.Effect;

        try
        {
            dialog.Owner.Opacity = 0.65;      // dim the owner
            dialog.Owner.Effect = new BlurEffect { Radius = 6 };
            dialog.ShowDialog();
        }
        finally
        {
            dialog.Owner.Opacity = oldOpacity;
            dialog.Owner.Effect = oldEffect;
        }
    }

    public bool ShowMessageDialog(string message, CaptionType caption = CaptionType.Default, string iconPath = "")
    {
        var vm = CreateUserMessageDialogViewModel(message, caption, iconPath, UI.ui_btnOK, UI.ui_btnCancel);

        var dialog = new UserMessageDialog(vm)
        {
            Owner = Application.Current.Windows.OfType<Window>().SingleOrDefault(x => x.IsActive)
        };
        var oldOpacity = dialog.Owner.Opacity;
        var oldEffect = dialog.Owner.Effect;

        try
        {
            dialog.Owner.Opacity = 0.55;      // dim the owner
            dialog.Owner.Effect = new BlurEffect { Radius = 6 };
            dialog.ShowDialog();
        }
        finally
        {
            dialog.Owner.Opacity = oldOpacity;
            dialog.Owner.Effect = oldEffect;
        }
        return dialog.Result;
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