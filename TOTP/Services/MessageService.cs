using FluentResults;
using Notification.Wpf;
using System;
using System.Diagnostics;
using System.IO;
using System.Windows;
using System.Windows.Threading;
using TOTP.Core.Common;
using TOTP.Infrastructure.Common;
using TOTP.Resources;
using TOTP.Services.Interfaces;

namespace TOTP.Services;

public sealed class MessageService : IMessageService
{
    private const string NotificationAreaName = "MainWindowNotificationArea";
    private readonly NotificationManager _notificationManager = new();

    public void ShowResultError(IResultBase result, string? context = null)
    {
        if (result.IsSuccess)
        {
            return;
        }

        var errorCode = result.GetErrorCode();
        var localizedMessage = ResultErrorLocalizer.ToUserMessage(errorCode, context);

        Show(
            UI.ui_Caption_Error,
            localizedMessage,
            NotificationType.Error,
            buttonText: UI.ui_btnDetails,
            buttonAction: OpenCurrentLogFile);
    }

    public void ShowInfo(string msg) => Show(UI.ui_Caption_Info, msg, NotificationType.Information);
    public void ShowWarning(string msg) => Show(UI.ui_Caption_Warning, msg, NotificationType.Warning);
    public void ShowError(string msg) => Show(UI.ui_Caption_Error, msg, NotificationType.Error, UI.ui_btnDetails, OpenCurrentLogFile);

    public bool ConfirmInfo(string msg, string? ok = null, string? cancel = null) =>
        Confirm(UI.ui_Caption_Info, msg, NotificationType.Information, ok, cancel);

    public bool ConfirmWarning(string msg, string? ok = null, string? cancel = null) =>
        Confirm(UI.ui_Caption_Warning, msg, NotificationType.Warning, ok, cancel);

    public bool ConfirmError(string msg, string? ok = null, string? cancel = null) =>
        Confirm(UI.ui_Caption_Error, msg, NotificationType.Error, ok, cancel);

    private void Show(
        string title,
        string message,
        NotificationType type,
        string? buttonText = null,
        Action? buttonAction = null)
    {
        RunOnUiThread(() =>
        {
            if (buttonAction == null || string.IsNullOrWhiteSpace(buttonText))
            {
                _notificationManager.Show(
                    title,
                    message,
                    type,
                    NotificationAreaName,
                    expirationTime: TimeSpan.FromSeconds(2),
                    onClose: null,
                    onClick: type == NotificationType.Error ? OpenCurrentLogFile : null,
                    trim: NotificationTextTrimType.NoTrim,
                    RowsCountWhenTrim: 2,
                    CloseOnClick: true,
                    TitleSettings: null,
                    MessageSettings: null,
                    ShowXbtn: true,
                    icon: null);

                return;
            }

            _notificationManager.Show(
                title,
                message,
                type,
                NotificationAreaName,
                expirationTime: TimeSpan.FromSeconds(2),
                onClick: null,
                onClose: null,
                LeftButton: buttonAction,
                LeftButtonText: buttonText,
                RightButton: null,
                RightButtonText: null,
                trim: NotificationTextTrimType.NoTrim,
                RowsCountWhenTrim: 2,
                CloseOnClick: false,
                TitleSettings: null,
                MessageSettings: null,
                ShowXbtn: true,
                icon: null);
        });
    }

    private bool Confirm(
        string title,
        string message,
        NotificationType type,
        string? ok,
        string? cancel)
    {
        bool result = false;

        RunOnUiThread(() =>
        {
            var frame = new DispatcherFrame();
            var completed = false;

            void Complete(bool value)
            {
                if (completed)
                {
                    return;
                }

                completed = true;
                result = value;
                frame.Continue = false;
            }

            _notificationManager.Show(
                title,
                message,
                type,
                NotificationAreaName,
                expirationTime: TimeSpan.MaxValue,
                onClick: type == NotificationType.Error ? OpenCurrentLogFile : null,
                onClose: () => Complete(false),
                LeftButton: () => Complete(true),
                LeftButtonText: ok ?? UI.ui_btnOK,
                RightButton: () => Complete(false),
                RightButtonText: cancel ?? UI.ui_btnCancel,
                trim: NotificationTextTrimType.NoTrim,
                RowsCountWhenTrim: 2,
                CloseOnClick: false,
                TitleSettings: null,
                MessageSettings: null,
                ShowXbtn: true,
                icon: null);

            Dispatcher.PushFrame(frame);
        });

        return result;
    }

    private static void RunOnUiThread(Action action)
    {
        var dispatcher = Application.Current?.Dispatcher;
        if (dispatcher == null || dispatcher.CheckAccess())
        {
            action();
            return;
        }

        dispatcher.Invoke(action);
    }

    private static void OpenCurrentLogFile()
    {
        try
        {
            var fullPath = Path.GetFullPath(StringsConstants.AppLogPath);
            if (!File.Exists(fullPath))
            {
                return;
            }

            Process.Start(new ProcessStartInfo
            {
                FileName = fullPath,
                UseShellExecute = true
            });
        }
        catch
        {
        }
    }
}
