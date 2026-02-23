using TOTP.Core.Enums;
using TOTP.ViewModels;

namespace TOTP.Services.Interfaces;

public interface IMessageService
{
    string ShowMessageBasedOnOperationStatus(OperationStatus opStatus, AccountViewModel? account);
    bool ShowDefaultMessageDialog(string message, string btnOkText = "", string btnCancelText = "",
        CaptionType caption = CaptionType.Default, string iconPath = "");
    void ShowMessage(string message, CaptionType caption = CaptionType.Info, string iconPath = "");
    bool ShowMessageDialog(string message, CaptionType caption = CaptionType.Info, string iconPath = "");

    public bool ShowErrorMessageDialog(string message);
    public bool ShowInfoMessageDialog(string message);
    public bool ShowWarningMessageDialog(string message);

    public void ShowErrorMessage(string message);
    public void ShowInfoMessage(string message);
    public void ShowWarningMessage(string message);
}