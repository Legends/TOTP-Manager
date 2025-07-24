namespace TOTP.Interfaces;

public interface IMessageService
{
    void ShowMessage(string message, string caption = "Info");
    bool ShowMessageDialog(string message, string caption = "Info");
}