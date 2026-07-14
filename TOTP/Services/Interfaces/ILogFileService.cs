namespace TOTP.Services.Interfaces;

public interface ILogFileService
{
    void OpenCurrentLogFile();
    bool CanOpenLogFolder();
    void OpenLogFolder();
}
