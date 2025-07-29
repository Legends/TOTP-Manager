using TOTP.Enums;

namespace TOTP.Interfaces;

public interface IMessageService
{
    void ShowMessage(string message, CaptionType caption = CaptionType.Info, string iconPath = "", string btnOkText = "Ok");
    bool ShowMessageDialog(string message, CaptionType caption = CaptionType.Info, string iconPath = "", string btnOkText = "Ok", string btnCancelText = "Cancel");

    public bool ShowErrorMessageDialog(string message, string btnOkText = "Ok", string btnCancelText = "Cancel");
    public bool ShowInfoMessageDialog(string message, string btnOkText = "Ok", string btnCancelText = "Cancel");
    public bool ShowWarningMessageDialog(string message, string btnOkText = "Ok", string btnCancelText = "Cancel");

    public void ShowErrorMessage(string message, string btnOkText = "Ok");
    public void ShowInfoMessage(string message, string btnOkText = "Ok");
    public void ShowWarningMessage(string message, string btnOkText = "Ok");
}