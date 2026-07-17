using System;
using FluentResults;
using TOTP.Core.Common;
using TOTP.Core.Services.Interfaces;
using TOTP.Resources;
using TOTP.Services.Interfaces;

namespace TOTP.Services;

public sealed class MessageService(ILogFileService logFileService, INotificationUiClient notificationUiClient) : IMessageService
{
    private const int DefaultDurationSeconds = 5;

    public void ShowResultError(IResultBase result, string? context = null)
    {
        if (result.IsSuccess)
        {
            return;
        }

        Show(
            UI.ui_Caption_Error,
            ResultErrorLocalizer.ToUserMessage(result.GetErrorCode(), context),
            NotificationSeverity.Error,
            UI.ui_btnDetails,
            logFileService.OpenCurrentLogFile);
    }

    public void ShowInfo(string msg, int? durationSeconds = null) =>
        Show(UI.ui_Caption_Info, msg, NotificationSeverity.Information, durationSeconds: durationSeconds);

    public void ShowSuccess(string msg, int? durationSeconds = null) =>
        Show(UI.ui_Caption_Info, msg, NotificationSeverity.Success, durationSeconds: durationSeconds);

    public void ShowWarning(string msg) => Show(UI.ui_Caption_Warning, msg, NotificationSeverity.Warning);

    public void ShowError(string msg) =>
        Show(UI.ui_Caption_Error, msg, NotificationSeverity.Error, UI.ui_btnDetails, logFileService.OpenCurrentLogFile);

    public bool ConfirmInfo(string msg, string? ok = null, string? cancel = null) =>
        Confirm(UI.ui_Caption_Info, msg, NotificationSeverity.Information, ok, cancel);

    public bool ConfirmWarning(string msg, string? ok = null, string? cancel = null) =>
        Confirm(UI.ui_Caption_Warning, msg, NotificationSeverity.Warning, ok, cancel);

    public bool ConfirmError(string msg, string? ok = null, string? cancel = null) =>
        Confirm(UI.ui_Caption_Error, msg, NotificationSeverity.Error, ok, cancel);

    private void Show(
        string title,
        string message,
        NotificationSeverity severity,
        string? buttonText = null,
        Action? buttonAction = null,
        int? durationSeconds = null)
    {
        notificationUiClient.Show(new NotificationShowRequest
        {
            Title = title,
            Message = message,
            Severity = severity,
            ExpirationTime = TimeSpan.FromSeconds(durationSeconds ?? DefaultDurationSeconds),
            OnClick = buttonAction == null && severity == NotificationSeverity.Error
                ? logFileService.OpenCurrentLogFile
                : null,
            LeftButton = buttonAction,
            LeftButtonText = buttonText,
            CloseOnClick = buttonAction == null
        });
    }

    private bool Confirm(
        string title,
        string message,
        NotificationSeverity severity,
        string? ok,
        string? cancel) =>
        notificationUiClient.Confirm(new NotificationConfirmRequest
        {
            Title = title,
            Message = message,
            Severity = severity,
            OkText = ok ?? UI.ui_btnOK,
            CancelText = cancel ?? UI.ui_btnCancel,
            OnClick = severity == NotificationSeverity.Error ? logFileService.OpenCurrentLogFile : null
        });
}
