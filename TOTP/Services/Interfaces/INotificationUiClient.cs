using Notification.Wpf;
using System;

namespace TOTP.Services.Interfaces;

public interface INotificationUiClient
{
    void Show(NotificationShowRequest request);
    bool Confirm(NotificationConfirmRequest request);
}

public sealed class NotificationShowRequest
{
    public required string Title { get; init; }
    public required string Message { get; init; }
    public required NotificationType Type { get; init; }
    public required TimeSpan ExpirationTime { get; init; }
    public Action? OnClick { get; init; }
    public Action? OnClose { get; init; }
    public Action? LeftButton { get; init; }
    public string? LeftButtonText { get; init; }
    public Action? RightButton { get; init; }
    public string? RightButtonText { get; init; }
    public required bool CloseOnClick { get; init; }
    public object? Icon { get; init; }
}

public sealed class NotificationConfirmRequest
{
    public required string Title { get; init; }
    public required string Message { get; init; }
    public required NotificationType Type { get; init; }
    public required string OkText { get; init; }
    public required string CancelText { get; init; }
    public Action? OnClick { get; init; }
    public object? Icon { get; init; }
}
