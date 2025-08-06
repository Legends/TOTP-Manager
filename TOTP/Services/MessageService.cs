using System.Linq;
using System.Windows;
using System.Windows.Media;
using TOTP.Enums;
using TOTP.Helper;
using TOTP.Interfaces;
using TOTP.Resources;
using TOTP.UserControls;

namespace TOTP.Services;

public class MessageService(IUserMessageDialogViewModel userMessageDialogViewModel) : IMessageService
{
    public void ShowErrorMessage(string message)
    {
        ShowMessage(message, CaptionType.Error, StringsConstants.ImgError);
    }
    public void ShowInfoMessage(string message)
    {
        ShowMessage(message, CaptionType.Info, StringsConstants.ImgInfo);
    }

    public void ShowWarningMessage(string message)
    {
        ShowMessage(message, CaptionType.Warning, StringsConstants.ImgWarning);
    }

    public bool ShowErrorMessageDialog(string message)
    {
        return ShowMessageDialog(message, CaptionType.Error, StringsConstants.ImgError);
    }
    public bool ShowInfoMessageDialog(string message)
    {

        return ShowMessageDialog(message, CaptionType.Info, StringsConstants.ImgInfo);
    }

    public bool ShowWarningMessageDialog(string message)
    {
        return ShowMessageDialog(message, CaptionType.Warning, StringsConstants.ImgWarning);
    }

    public void ShowMessage(string message, CaptionType caption = CaptionType.Default, string iconPath = "")
    {
        var vm = CreateViewModel(message, caption, iconPath, UI.ui_btnOK, string.Empty);
        vm.ShowCancelButton = false;

        vm.IconPath = caption switch
        {
            CaptionType.Error => StringsConstants.ImgError,
            CaptionType.Warning => StringsConstants.ImgWarning,
            CaptionType.Info => StringsConstants.ImgInfo,
            _ => string.Empty
        };

        var dialog = new UserMessageDialog(vm)
        {
            Owner = Application.Current.Windows.OfType<Window>().SingleOrDefault(x => x.IsActive)
        };
        dialog.ShowDialog();
    }

    public bool ShowMessageDialog(string message, CaptionType caption = CaptionType.Default, string iconPath = "")
    {
        var vm = CreateViewModel(message, caption, iconPath, UI.ui_btnOK, UI.ui_btnCancel);

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
            CaptionType.Warning => (Brushes.LightYellow, Brushes.Black),
            CaptionType.Info => (Brushes.LightSteelBlue, Brushes.White),
            _ => (Brushes.Gray, Brushes.White)
        };
        return vm;
    }

}