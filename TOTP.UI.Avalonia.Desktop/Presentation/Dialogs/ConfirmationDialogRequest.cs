using TOTP.Core.Services.Interfaces;

namespace TOTP.Avalonia.Desktop.Presentation.Dialogs;

public sealed record ConfirmationDialogRequest(
    string Title,
    string Message,
    NotificationSeverity Severity,
    string ConfirmText,
    string CancelText);
