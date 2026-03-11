using FluentResults;
using Moq;
using System.Collections.ObjectModel;
using TOTP.Commands;
using TOTP.Core.Enums;
using TOTP.Core.Models;
using TOTP.Core.Security.Interfaces;
using TOTP.Core.Security.Models;
using TOTP.Core.Services.Interfaces;
using TOTP.Services;
using TOTP.Services.Interfaces;
using TOTP.ViewModels;

namespace TOTP.Tests.Services;

public sealed class SettingsDialogOrchestrationServiceTests
{
    [Fact]
    public async Task CreateAndLoadAsync_WhenSettingsLoadFails_ThrowsWithCombinedMessages()
    {
        var settings = new Mock<ISettingsService>();
        settings.Setup(s => s.LoadAsync())
            .ReturnsAsync(Result.Fail<IAppSettings>("load failed").WithError("disk error"));

        var sut = new SettingsDialogOrchestrationService(
            settings.Object,
            Mock.Of<IAuthorizationService>(),
            Mock.Of<IAccountTransferWorkflowService>(),
            Mock.Of<ISettingsAuthorizationWorkflowService>(),
            Mock.Of<ISettingsPersistenceService>(),
            Mock.Of<IAutoUpdateService>(),
            Mock.Of<IMessageService>(),
            Mock.Of<ILogSwitchService>());

        var ex = await Assert.ThrowsAsync<InvalidOperationException>(() =>
            sut.CreateAndLoadAsync(new RelayCommand(() => { }), () => { }, CreateContext()));

        Assert.Contains("load failed", ex.Message);
        Assert.Contains("disk error", ex.Message);
    }

    [Fact]
    public async Task CreateAndLoadAsync_WhenSuccessful_ReturnsLoadedViewModel()
    {
        var appSettings = new AppSettings
        {
            Authorization = new AuthorizationProfile { Gate = AuthorizationGateKind.Password }
        };

        var settings = new Mock<ISettingsService>();
        var auth = new Mock<IAuthorizationService>();
        var authWorkflow = new Mock<ISettingsAuthorizationWorkflowService>();
        var persistence = new Mock<ISettingsPersistenceService>();
        var tokenTransfer = new Mock<IAccountTransferWorkflowService>();
        var message = new Mock<IMessageService>();
        var logSwitch = new Mock<ILogSwitchService>();

        settings.SetupGet(s => s.Current).Returns(appSettings);
        settings.Setup(s => s.LoadAsync()).ReturnsAsync(Result.Ok<IAppSettings>(appSettings));

        auth.Setup(a => a.IsHelloAvailableAsync()).ReturnsAsync(false);

        persistence.Setup(p => p.ReadCurrentGeneralSettings())
            .Returns(new SettingsGeneralSnapshot(
                AppLogLevel.Warning,
                LockOnSessionLock: false,
                LockOnMinimize: true,
                LockOnIdleTimeout: true,
                IdleTimeoutMinutes: 11,
                ClearClipboardEnabled: true,
                ClearClipboardSeconds: 15,
                QrPreviewScaleFactor: 2.0,
                ExportEncrypt: true,
                OpenExportFileAfterExportBeforeEncrypt: true,
                HideSecretsByDefault: true));
        persistence.Setup(p => p.SaveGeneralSettingsAsync(It.IsAny<SettingsGeneralSnapshot>()))
            .ReturnsAsync(new SettingsPersistenceResult(true));

        logSwitch.SetupGet(l => l.MinimumLevel).Returns(AppLogLevel.Warning);
        logSwitch.SetupGet(l => l.IsCliOverrideActive).Returns(false);

        var sut = new SettingsDialogOrchestrationService(
            settings.Object,
            auth.Object,
            tokenTransfer.Object,
            authWorkflow.Object,
            persistence.Object,
            Mock.Of<IAutoUpdateService>(),
            message.Object,
            logSwitch.Object);

        var vm = await sut.CreateAndLoadAsync(new RelayCommand(() => { }), () => { }, CreateContext());

        Assert.NotNull(vm);
        Assert.True(vm.IsPasswordSelected);
        Assert.False(vm.IsHelloAvailable);
        Assert.False(vm.LockOnSessionLock);
        Assert.Equal(AppLogLevel.Warning, vm.SelectedLogLevel);
    }

    [Fact]
    public async Task CreateAndLoadAsync_WiresTransferCommandsToAccountTransferWorkflow()
    {
        var appSettings = new AppSettings
        {
            Authorization = new AuthorizationProfile { Gate = AuthorizationGateKind.Password }
        };

        var settings = new Mock<ISettingsService>();
        var auth = new Mock<IAuthorizationService>();
        var authWorkflow = new Mock<ISettingsAuthorizationWorkflowService>();
        var persistence = new Mock<ISettingsPersistenceService>();
        var tokenTransfer = new Mock<IAccountTransferWorkflowService>();
        var message = new Mock<IMessageService>();
        var logSwitch = new Mock<ILogSwitchService>();

        var contextOtps = new ObservableCollection<OtpViewModel>
        {
            new(Guid.NewGuid(), "GitHub", "AAAA", "john")
        };
        var context = new Mock<IAccountsCollectionContext>();
        context.SetupGet(c => c.AllOtps).Returns(contextOtps);

        settings.SetupGet(s => s.Current).Returns(appSettings);
        settings.Setup(s => s.LoadAsync()).ReturnsAsync(Result.Ok<IAppSettings>(appSettings));

        auth.Setup(a => a.IsHelloAvailableAsync()).ReturnsAsync(true);

        persistence.Setup(p => p.ReadCurrentGeneralSettings())
            .Returns(new SettingsGeneralSnapshot(
                AppLogLevel.Information,
                LockOnSessionLock: true,
                LockOnMinimize: true,
                LockOnIdleTimeout: true,
                IdleTimeoutMinutes: 10,
                ClearClipboardEnabled: true,
                ClearClipboardSeconds: 15,
                QrPreviewScaleFactor: 1.5,
                ExportEncrypt: false,
                OpenExportFileAfterExportBeforeEncrypt: true,
                HideSecretsByDefault: true));
        persistence.Setup(p => p.SaveGeneralSettingsAsync(It.IsAny<SettingsGeneralSnapshot>()))
            .ReturnsAsync(new SettingsPersistenceResult(true));

        logSwitch.SetupGet(l => l.MinimumLevel).Returns(AppLogLevel.Information);
        logSwitch.SetupGet(l => l.IsCliOverrideActive).Returns(false);

        tokenTransfer.Setup(a => a.ExportOtpsAsync(false, ExportFileFormat.Csv)).Returns(Task.CompletedTask);
        tokenTransfer.Setup(a => a.ImportOtpsAsync(ImportConflictStrategy.ReplaceExisting, contextOtps)).Returns(Task.CompletedTask);

        var sut = new SettingsDialogOrchestrationService(
            settings.Object,
            auth.Object,
            tokenTransfer.Object,
            authWorkflow.Object,
            persistence.Object,
            Mock.Of<IAutoUpdateService>(),
            message.Object,
            logSwitch.Object);

        var vm = await sut.CreateAndLoadAsync(new RelayCommand(() => { }), () => { }, context.Object);

        vm.SelectedExportFormat = ExportFileFormat.Csv;
        vm.SelectedImportConflictOption = vm.AvailableImportConflictOptions.Single(x => x.Strategy == ImportConflictStrategy.ReplaceExisting);

        vm.ExportCommand.Execute(null);
        vm.ImportCommand.Execute(null);

        await WaitUntilAsync(() => true, 100);

        tokenTransfer.Verify(a => a.ExportOtpsAsync(false, ExportFileFormat.Csv), Times.Once);
        tokenTransfer.Verify(a => a.ImportOtpsAsync(ImportConflictStrategy.ReplaceExisting, contextOtps), Times.Once);
    }

    private static IAccountsCollectionContext CreateContext()
    {
        var context = new Mock<IAccountsCollectionContext>();
        context.SetupGet(c => c.AllOtps).Returns(new ObservableCollection<OtpViewModel>());
        return context.Object;
    }

    private static async Task WaitUntilAsync(Func<bool> condition, int timeoutMs = 1500)
    {
        var start = Environment.TickCount64;
        while (!condition())
        {
            if (Environment.TickCount64 - start > timeoutMs)
            {
                throw new TimeoutException("Condition was not met in time.");
            }

            await Task.Delay(20);
        }
    }
}
