using FluentResults;

namespace TOTP.Services.Interfaces;

public interface IMessageService
{
    void ShowResultError(IResultBase result, string? context = null);
    void ShowInfo(string msg, int? durationSeconds = null);
    void ShowSuccess(string msg, int? durationSeconds = null);
    void ShowWarning(string msg);
    void ShowError(string msg);
    bool ConfirmInfo(string msg, string? ok = null, string? cancel = null);
    bool ConfirmWarning(string msg, string? ok = null, string? cancel = null);
    bool ConfirmError(string msg, string? ok = null, string? cancel = null);
}
