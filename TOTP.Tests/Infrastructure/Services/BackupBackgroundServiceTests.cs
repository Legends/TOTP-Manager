using FluentResults;
using Microsoft.Extensions.Logging;
using Moq;
using TOTP.Core.Services.Interfaces;
using TOTP.Infrastructure.Services;

namespace TOTP.Tests.Infrastructure.Services;

public sealed class BackupBackgroundServiceTests
{
    [Fact]
    public async Task StopAsync_CallsBackupOnce()
    {
        var otpManager = new Mock<IOtpManager>();
        otpManager.Setup(m => m.BackupOtpEntriesStorageFileAsync()).ReturnsAsync(Result.Ok());
        var logger = new Mock<ILogger<BackupBackgroundService>>();

        var sut = new BackupBackgroundService(otpManager.Object, logger.Object);

        await sut.StopAsync(CancellationToken.None);

        otpManager.Verify(m => m.BackupOtpEntriesStorageFileAsync(), Times.Once);
    }

    [Fact]
    public async Task StopAsync_WhenBackupThrows_DoesNotThrow()
    {
        var otpManager = new Mock<IOtpManager>();
        otpManager.Setup(m => m.BackupOtpEntriesStorageFileAsync()).ThrowsAsync(new InvalidOperationException("boom"));
        var logger = new Mock<ILogger<BackupBackgroundService>>();

        var sut = new BackupBackgroundService(otpManager.Object, logger.Object);

        await sut.StopAsync(CancellationToken.None);

        otpManager.Verify(m => m.BackupOtpEntriesStorageFileAsync(), Times.Once);
    }
}

