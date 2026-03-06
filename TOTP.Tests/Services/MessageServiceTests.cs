using FluentResults;
using Moq;
using Notification.Wpf;
using TOTP.Core.Common;
using TOTP.Resources;
using TOTP.Services;
using TOTP.Services.Interfaces;

namespace TOTP.Tests.Services;

public sealed class MessageServiceTests
{
    [StaFact]
    public void ShowResultError_WhenResultSuccess_DoesNothing()
    {
        var client = new FakeNotificationUiClient();
        var sut = new MessageService(Mock.Of<ILogFileService>(), client);

        sut.ShowResultError(Result.Ok());

        Assert.Empty(client.ShowRequests);
    }

    [StaFact]
    public void ShowResultError_WhenFailed_ShowsErrorWithDetailsButton()
    {
        var client = new FakeNotificationUiClient();
        var log = new Mock<ILogFileService>();
        var sut = new MessageService(log.Object, client);

        var result = Result.Fail(new AppError(AppErrorCode.TokensDeleteFailed, "tech"));

        sut.ShowResultError(result);

        var req = Assert.Single(client.ShowRequests);
        Assert.Equal(UI.ui_Caption_Error, req.Title);
        Assert.Equal(UI.err_TokensDeleteFailed, req.Message);
        Assert.Equal(NotificationType.Error, req.Type);
        Assert.Equal(UI.ui_btnDetails, req.LeftButtonText);
        Assert.False(req.CloseOnClick);
        Assert.NotNull(req.LeftButton);

        req.LeftButton!.Invoke();
        log.Verify(l => l.OpenCurrentLogFile(), Times.Once);
    }

    [StaFact]
    public void ShowInfo_UsesInformationTypeAndCustomDuration()
    {
        var client = new FakeNotificationUiClient();
        var sut = new MessageService(Mock.Of<ILogFileService>(), client);

        sut.ShowInfo("hello", durationSeconds: 3);

        var req = Assert.Single(client.ShowRequests);
        Assert.Equal(NotificationType.Information, req.Type);
        Assert.Equal(TimeSpan.FromSeconds(3), req.ExpirationTime);
        Assert.True(req.CloseOnClick);
    }

    [StaFact]
    public void ShowError_UsesDetailsButtonAndNoAutoCloseOnClick()
    {
        var client = new FakeNotificationUiClient();
        var log = new Mock<ILogFileService>();
        var sut = new MessageService(log.Object, client);

        sut.ShowError("fatal");

        var req = Assert.Single(client.ShowRequests);
        Assert.Equal(NotificationType.Error, req.Type);
        Assert.Equal(UI.ui_btnDetails, req.LeftButtonText);
        Assert.False(req.CloseOnClick);
        Assert.NotNull(req.LeftButton);
    }

    [StaFact]
    public void ShowWarning_UsesWarningType()
    {
        var client = new FakeNotificationUiClient();
        var sut = new MessageService(Mock.Of<ILogFileService>(), client);

        sut.ShowWarning("warn");

        var req = Assert.Single(client.ShowRequests);
        Assert.Equal(NotificationType.Warning, req.Type);
    }

    [StaFact]
    public void ConfirmInfo_UsesDefaultButtonsAndReturnsClientResult()
    {
        var client = new FakeNotificationUiClient { ConfirmResult = true };
        var sut = new MessageService(Mock.Of<ILogFileService>(), client);

        var result = sut.ConfirmInfo("confirm?");

        Assert.True(result);
        var req = Assert.Single(client.ConfirmRequests);
        Assert.Equal(NotificationType.Information, req.Type);
        Assert.Equal(UI.ui_btnOK, req.OkText);
        Assert.Equal(UI.ui_btnCancel, req.CancelText);
    }

    [StaFact]
    public void ConfirmError_SetsErrorCaptionAndOnClickLogAction()
    {
        var client = new FakeNotificationUiClient { ConfirmResult = false };
        var log = new Mock<ILogFileService>();
        var sut = new MessageService(log.Object, client);

        var result = sut.ConfirmError("error?", ok: "Yes", cancel: "No");

        Assert.False(result);
        var req = Assert.Single(client.ConfirmRequests);
        Assert.Equal(UI.ui_Caption_Error, req.Title);
        Assert.Equal("Yes", req.OkText);
        Assert.Equal("No", req.CancelText);
        Assert.NotNull(req.OnClick);

        req.OnClick!.Invoke();
        log.Verify(l => l.OpenCurrentLogFile(), Times.Once);
    }

    private sealed class FakeNotificationUiClient : INotificationUiClient
    {
        public List<NotificationShowRequest> ShowRequests { get; } = [];
        public List<NotificationConfirmRequest> ConfirmRequests { get; } = [];
        public bool ConfirmResult { get; set; }

        public void Show(NotificationShowRequest request) => ShowRequests.Add(request);

        public bool Confirm(NotificationConfirmRequest request)
        {
            ConfirmRequests.Add(request);
            return ConfirmResult;
        }
    }
}
