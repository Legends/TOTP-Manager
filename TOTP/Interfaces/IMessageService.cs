using TOTP.Enums;

namespace TOTP.Interfaces;

public interface IMessageService
{
    void ShowMessage(string message, CaptionType caption = CaptionType.Info, string iconPath = "");
    bool ShowMessageDialog(string message, CaptionType caption = CaptionType.Info, string iconPath = "");

    public bool ShowErrorMessageDialog(string message);
    public bool ShowInfoMessageDialog(string message);
    public bool ShowWarningMessageDialog(string message);

    public void ShowErrorMessage(string message);
    public void ShowInfoMessage(string message);
    public void ShowWarningMessage(string message);
}