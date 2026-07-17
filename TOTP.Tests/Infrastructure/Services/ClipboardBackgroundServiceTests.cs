using FluentResults;
using Microsoft.Extensions.Logging;
using Moq;
using System.Reflection;
using TOTP.Core.Common;
using TOTP.Core.Services.Interfaces;
using TOTP.Core.Services.Models;
using TOTP.Infrastructure.Services;

namespace TOTP.Tests.Infrastructure.Services;

public sealed class ClipboardBackgroundServiceTests
{
    [Fact]
    public void CopyAndScheduleClear_WhenTextEmpty_ReturnsSuccessWithoutPlatformAccess()
    {
        var platform = CreatePlatform();
        var sut = CreateSut(platform);

        var result = sut.CopyAndScheduleClear(string.Empty);

        Assert.True(result.IsSuccess);
        platform.VerifyNoOtherCalls();
        Assert.Null(GetPrivateField<ClipboardWriteReceipt?>(sut, "_scheduledReceipt"));
    }

    [Fact]
    public void CopyAndScheduleClear_WhenConditionalClearUnsupported_ReturnsUnavailableWithoutCopying()
    {
        var platform = CreatePlatform(ClipboardCapabilities.WriteText);
        var sut = CreateSut(platform);

        var result = sut.CopyAndScheduleClear("123456");

        Assert.False(result.IsSuccess);
        Assert.Equal(AppErrorCode.ClipboardUnavailable, result.GetErrorCode());
        platform.Verify(p => p.SetText(It.IsAny<string>()), Times.Never);
    }

    [Fact]
    public void CopyAndScheduleClear_StoresOnlyReceiptAndDeadline()
    {
        var time = new ManualTimeProvider(new DateTimeOffset(2026, 7, 17, 10, 0, 0, TimeSpan.Zero));
        var platform = CreatePlatform();
        platform.Setup(p => p.SetText("123456")).Returns(Result.Ok(new ClipboardWriteReceipt(42)));
        var sut = CreateSut(platform, time);

        var result = sut.CopyAndScheduleClear("123456", TimeSpan.FromSeconds(15));

        Assert.True(result.IsSuccess);
        Assert.Equal(new ClipboardWriteReceipt(42), GetPrivateField<ClipboardWriteReceipt?>(sut, "_scheduledReceipt"));
        Assert.Equal(time.GetUtcNow().AddSeconds(15), GetPrivateField<DateTimeOffset?>(sut, "_clearAt"));
        Assert.DoesNotContain(
            typeof(ClipboardBackgroundService).GetFields(BindingFlags.Instance | BindingFlags.NonPublic),
            field => field.FieldType == typeof(string));
    }

    [Fact]
    public void ProcessScheduledClear_BeforeDeadline_DoesNotAccessPlatform()
    {
        var time = new ManualTimeProvider(DateTimeOffset.UtcNow);
        var platform = CreatePlatform();
        platform.Setup(p => p.SetText("123456")).Returns(Result.Ok(new ClipboardWriteReceipt(42)));
        var sut = CreateSut(platform, time);
        Assert.True(sut.CopyAndScheduleClear("123456", TimeSpan.FromSeconds(15)).IsSuccess);

        ProcessScheduledClear(sut);

        platform.Verify(p => p.ClearIfUnchanged(It.IsAny<ClipboardWriteReceipt>()), Times.Never);
    }

    [Theory]
    [InlineData(true)]
    [InlineData(false)]
    public void ProcessScheduledClear_WhenDue_ClearsOrSkipsAccordingToReplacementState(bool cleared)
    {
        var time = new ManualTimeProvider(DateTimeOffset.UtcNow);
        var receipt = new ClipboardWriteReceipt(42);
        var platform = CreatePlatform();
        platform.Setup(p => p.SetText("123456")).Returns(Result.Ok(receipt));
        platform.Setup(p => p.ClearIfUnchanged(receipt)).Returns(Result.Ok(cleared));
        var sut = CreateSut(platform, time);
        Assert.True(sut.CopyAndScheduleClear("123456", TimeSpan.FromSeconds(15)).IsSuccess);
        time.Advance(TimeSpan.FromSeconds(15));

        ProcessScheduledClear(sut);

        platform.Verify(p => p.ClearIfUnchanged(receipt), Times.Once);
        Assert.Null(GetPrivateField<ClipboardWriteReceipt?>(sut, "_scheduledReceipt"));
        Assert.Null(GetPrivateField<DateTimeOffset?>(sut, "_clearAt"));
    }

    [Fact]
    public void ProcessScheduledClear_WhenPlatformFails_RetainsScheduleForRetry()
    {
        var time = new ManualTimeProvider(DateTimeOffset.UtcNow);
        var receipt = new ClipboardWriteReceipt(42);
        var platform = CreatePlatform();
        platform.Setup(p => p.SetText("123456")).Returns(Result.Ok(receipt));
        platform.Setup(p => p.ClearIfUnchanged(receipt)).Returns(
            Result.Fail(new AppError(AppErrorCode.ClipboardClearFailed, "failed")));
        var sut = CreateSut(platform, time);
        Assert.True(sut.CopyAndScheduleClear("123456", TimeSpan.Zero).IsSuccess);

        ProcessScheduledClear(sut);

        Assert.Equal(receipt, GetPrivateField<ClipboardWriteReceipt?>(sut, "_scheduledReceipt"));
    }

    [Fact]
    public void SetText_WhenSuccessful_CancelsExistingSchedule()
    {
        var platform = CreatePlatform();
        platform.Setup(p => p.SetText("123456")).Returns(Result.Ok(new ClipboardWriteReceipt(1)));
        platform.Setup(p => p.SetText("other")).Returns(Result.Ok(new ClipboardWriteReceipt(2)));
        var sut = CreateSut(platform);
        Assert.True(sut.CopyAndScheduleClear("123456").IsSuccess);

        var result = sut.SetText("other");

        Assert.True(result.IsSuccess);
        Assert.Null(GetPrivateField<ClipboardWriteReceipt?>(sut, "_scheduledReceipt"));
    }

    private static Mock<IPlatformClipboard> CreatePlatform(
        ClipboardCapabilities capabilities = ClipboardCapabilities.WriteText | ClipboardCapabilities.ConditionalClear)
    {
        var platform = new Mock<IPlatformClipboard>();
        platform.SetupGet(p => p.Capabilities).Returns(capabilities);
        return platform;
    }

    private static ClipboardBackgroundService CreateSut(
        Mock<IPlatformClipboard> platform,
        TimeProvider? timeProvider = null) =>
        new(platform.Object, Mock.Of<ILogger<ClipboardBackgroundService>>(), timeProvider);

    private static void ProcessScheduledClear(ClipboardBackgroundService sut)
    {
        var method = typeof(ClipboardBackgroundService).GetMethod(
            "ProcessScheduledClear",
            BindingFlags.Instance | BindingFlags.NonPublic);
        Assert.NotNull(method);
        method!.Invoke(sut, null);
    }

    private static T GetPrivateField<T>(object instance, string fieldName)
    {
        var field = instance.GetType().GetField(fieldName, BindingFlags.NonPublic | BindingFlags.Instance);
        Assert.NotNull(field);
        return (T)field!.GetValue(instance)!;
    }

    private sealed class ManualTimeProvider(DateTimeOffset utcNow) : TimeProvider
    {
        private DateTimeOffset _utcNow = utcNow;

        public override DateTimeOffset GetUtcNow() => _utcNow;

        public void Advance(TimeSpan duration) => _utcNow = _utcNow.Add(duration);
    }
}
