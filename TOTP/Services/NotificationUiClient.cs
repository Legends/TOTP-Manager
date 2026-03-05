using Notification.Wpf;
using Notification.Wpf.Base;
using Notification.Wpf.Constants;
using System;
using System.Windows.Threading;
using TOTP.Services.Interfaces;

namespace TOTP.Services;

public sealed class NotificationUiClient : INotificationUiClient
{
    private const string NotificationAreaName = "MainWindowNotificationArea";
    private readonly NotificationManager _notificationManager = new();

    private static readonly TextContentSettings TitleTextSettings = new()
    {
        FontFamily = new System.Windows.Media.FontFamily("Segoe UI Semibold"),
        FontSize = 13,
        TextAlignment = System.Windows.TextAlignment.Left
    };

    private static readonly TextContentSettings MessageTextSettings = new()
    {
        FontFamily = new System.Windows.Media.FontFamily("Segoe UI"),
        FontSize = 12,
        TextAlignment = System.Windows.TextAlignment.Left
    };

    public void Show(NotificationShowRequest request)
    {
        _notificationManager.Show(
            request.Title,
            request.Message,
            request.Type,
            NotificationAreaName,
            expirationTime: request.ExpirationTime,
            onClick: request.OnClick,
            onClose: request.OnClose,
            LeftButton: request.LeftButton,
            LeftButtonText: request.LeftButtonText,
            RightButton: request.RightButton,
            RightButtonText: request.RightButtonText,
            trim: NotificationTextTrimType.NoTrim,
            RowsCountWhenTrim: 2,
            CloseOnClick: request.CloseOnClick,
            TitleSettings: TitleTextSettings,
            MessageSettings: MessageTextSettings,
            ShowXbtn: true,
            icon: request.Icon);
    }

    public bool Confirm(NotificationConfirmRequest request)
    {
        var result = false;
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
            request.Title,
            request.Message,
            request.Type,
            NotificationAreaName,
            expirationTime: TimeSpan.MaxValue,
            onClick: request.OnClick,
            onClose: () => Complete(false),
            LeftButton: () => Complete(true),
            LeftButtonText: request.OkText,
            RightButton: () => Complete(false),
            RightButtonText: request.CancelText,
            trim: NotificationTextTrimType.NoTrim,
            RowsCountWhenTrim: 2,
            CloseOnClick: false,
            TitleSettings: TitleTextSettings,
            MessageSettings: MessageTextSettings,
            ShowXbtn: true,
            icon: request.Icon);

        Dispatcher.PushFrame(frame);
        return result;
    }
}
