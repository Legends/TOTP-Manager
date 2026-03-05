using Microsoft.Extensions.Logging;
using Moq;
using TOTP.Core.Services.Interfaces;
using TOTP.Infrastructure.Services;

namespace TOTP.Tests.Infrastructure.Services;

public sealed class ClipboardBackgroundServiceTests
{
    [Fact]
    public void CopyAndScheduleClear_WhenTextEmpty_ReturnsWithoutScheduling()
    {
        var logger = new Mock<ILogger<ClipboardBackgroundService>>();
        var logSwitch = new Mock<ILogSwitchService>();
        var sut = new ClipboardBackgroundService(logger.Object, logSwitch.Object);

        sut.CopyAndScheduleClear(string.Empty);

        Assert.Null(GetPrivateField<string>(sut, "_lastCopiedText"));
        Assert.Null(GetPrivateField<DateTime?>(sut, "_clearAt"));
        logSwitch.Verify(l => l.GetLevel(), Times.Never);
        VerifyNoLog(logger);
    }

    [Fact]
    public void SetText_WhenTextEmpty_ReturnsWithoutChangingState()
    {
        var logger = new Mock<ILogger<ClipboardBackgroundService>>();
        var logSwitch = new Mock<ILogSwitchService>();
        var sut = new ClipboardBackgroundService(logger.Object, logSwitch.Object);

        sut.SetText(string.Empty);

        Assert.Null(GetPrivateField<string>(sut, "_lastCopiedText"));
        Assert.Null(GetPrivateField<DateTime?>(sut, "_clearAt"));
        VerifyNoLog(logger);
    }

    private static T GetPrivateField<T>(object instance, string fieldName)
    {
        var field = instance.GetType().GetField(fieldName, System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
        Assert.NotNull(field);
        return (T)field.GetValue(instance)!;
    }

    private static void VerifyNoLog(Mock<ILogger<ClipboardBackgroundService>> logger)
    {
        logger.Verify(
            x => x.Log(
                It.IsAny<LogLevel>(),
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((_, _) => true),
                It.IsAny<Exception>(),
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.Never);
    }
}
