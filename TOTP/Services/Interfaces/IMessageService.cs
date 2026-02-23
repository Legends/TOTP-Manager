using FluentResults;
using TOTP.Core.Enums;

namespace TOTP.Services.Interfaces;

public interface IMessageService
{
    // Smart Result Handling
    void ShowResultError(IResultBase result, string? platform = null);

    // Notification API
    void ShowInfo(string msg);
    void ShowWarning(string msg);
    void ShowError(string msg);

    // Confirmation API (with optional custom button text)
    bool ConfirmInfo(string msg, string? ok = null, string? cancel = null);
    bool ConfirmWarning(string msg, string? ok = null, string? cancel = null);
    bool ConfirmError(string msg, string? ok = null, string? cancel = null);

    // Generic API (matching your original requirements)
    void ShowMessage(string message, CaptionType caption = CaptionType.Default, string iconPath = "");
    bool ShowMessageDialog(string message, CaptionType caption = CaptionType.Default, string iconPath = "");
}