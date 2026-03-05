using Moq;
using TOTP.Commands;
using TOTP.Core.Enums;
using TOTP.Core.Models;
using TOTP.Core.Security.Interfaces;
using TOTP.Core.Security.Models;
using TOTP.Core.Services.Interfaces;
using TOTP.Services.Interfaces;
using TOTP.ViewModels;

namespace TOTP.Tests.ViewModels;

public sealed class SettingsViewModelTests
{
    [Fact]
    public async Task LoadAsync_WhenHelloUnavailableAndGateWasHello_FallsBackToPassword()
    {
        var settings = new Mock<ISettingsService>();
        var auth = new Mock<IAuthorizationService>();
        var authWorkflow = new Mock<ISettingsAuthorizationWorkflowService>();
        var persistence = new Mock<ISettingsPersistenceService>();
        var transfer = new Mock<ISettingsTransferWorkflowService>();
        var message = new Mock<IMessageService>();
        var logSwitch = new Mock<ILogSwitchService>();

        var appSettings = new AppSettings
        {
            Authorization = new AuthorizationProfile { Gate = AuthorizationGateKind.Hello }
        };

        settings.SetupGet(s => s.Current).Returns(appSettings);
        auth.Setup(a => a.IsHelloAvailableAsync()).ReturnsAsync(false);
        persistence.Setup(p => p.ReadCurrentGeneralSettings()).Returns(CreateSnapshot());
        logSwitch.SetupGet(l => l.MinimumLevel).Returns(AppLogLevel.Information);

        var vm = CreateSut(settings, auth, authWorkflow, persistence, transfer, message, logSwitch);

        await vm.LoadAsync();

        Assert.True(vm.IsPasswordSelected);
        Assert.False(vm.IsHelloSelected);
        Assert.True(vm.IsHelloUnavailable);
    }

    [Fact]
    public async Task QrPreviewScaleFactor_WhenOutOfRangeOrFractional_ClampsAndRoundsHalfSteps()
    {
        var (_, _, _, persistence, _, _, _, vm) = CreateSutWithDependencies();
        persistence.Setup(p => p.ReadCurrentGeneralSettings()).Returns(CreateSnapshot());
        persistence.Setup(p => p.SaveGeneralSettingsAsync(It.IsAny<SettingsGeneralSnapshot>()))
            .ReturnsAsync(new SettingsPersistenceResult(true));

        await vm.LoadAsync();

        vm.QrPreviewScaleFactor = 6.8;
        Assert.Equal(6.0, vm.QrPreviewScaleFactor);

        vm.QrPreviewScaleFactor = 1.24;
        Assert.Equal(1.0, vm.QrPreviewScaleFactor);

        vm.QrPreviewScaleFactor = 1.26;
        Assert.Equal(1.5, vm.QrPreviewScaleFactor);
    }

    [Fact]
    public async Task ExportEncrypt_ToggleRestoresPreviousOpenAfterExportChoice()
    {
        var (_, _, _, persistence, _, _, _, vm) = CreateSutWithDependencies();

        persistence.Setup(p => p.ReadCurrentGeneralSettings()).Returns(CreateSnapshot(
            exportEncrypt: false,
            openExportFileAfterExportBeforeEncrypt: true));
        persistence.Setup(p => p.SaveGeneralSettingsAsync(It.IsAny<SettingsGeneralSnapshot>()))
            .ReturnsAsync(new SettingsPersistenceResult(true));

        await vm.LoadAsync();

        vm.OpenExportFileAfterExport = true;
        vm.ExportEncrypt = true;

        Assert.False(vm.OpenExportFileAfterExport);
        Assert.False(vm.IsOpenExportFileAfterExportOptionEnabled);

        vm.ExportEncrypt = false;

        Assert.True(vm.OpenExportFileAfterExport);
        Assert.True(vm.IsOpenExportFileAfterExportOptionEnabled);
    }

    [Fact]
    public async Task ExportEncrypt_WhenEnabledAfterPlainFormatSelection_UpdatesSelectedFormatToTotpAndRaisesNotification()
    {
        var (_, _, _, persistence, _, _, _, vm) = CreateSutWithDependencies();
        persistence.Setup(p => p.ReadCurrentGeneralSettings()).Returns(CreateSnapshot(exportEncrypt: false));
        await vm.LoadAsync();

        vm.SelectedExportFormat = ExportFileFormat.Csv;

        var raised = false;
        vm.PropertyChanged += (_, e) =>
        {
            if (e.PropertyName == nameof(SettingsViewModel.SelectedExportFormat))
            {
                raised = true;
            }
        };

        vm.ExportEncrypt = true;

        Assert.Equal(ExportFileFormat.Totp, vm.SelectedExportFormat);
        Assert.True(raised);
    }

    [Fact]
    public async Task ExportEncrypt_WhenDisabledAfterEncryptedMode_DefaultsFormatToJsonAndRaisesNotification()
    {
        var (_, _, _, persistence, _, _, _, vm) = CreateSutWithDependencies();
        persistence.Setup(p => p.ReadCurrentGeneralSettings()).Returns(CreateSnapshot(exportEncrypt: true));
        await vm.LoadAsync();

        Assert.Equal(ExportFileFormat.Totp, vm.SelectedExportFormat);

        var raised = false;
        vm.PropertyChanged += (_, e) =>
        {
            if (e.PropertyName == nameof(SettingsViewModel.SelectedExportFormat))
            {
                raised = true;
            }
        };

        vm.ExportEncrypt = false;

        Assert.Equal(ExportFileFormat.Json, vm.SelectedExportFormat);
        Assert.True(raised);
    }

    [Fact]
    public async Task SelectedExportFormat_WhenEncrypted_IgnoresPlainFormatSelection()
    {
        var (_, _, _, persistence, _, _, _, vm) = CreateSutWithDependencies();
        persistence.Setup(p => p.ReadCurrentGeneralSettings()).Returns(CreateSnapshot(exportEncrypt: true));
        await vm.LoadAsync();

        vm.SelectedExportFormat = ExportFileFormat.Csv;

        Assert.Equal(ExportFileFormat.Totp, vm.SelectedExportFormat);
        Assert.Equal([ExportFileFormat.Totp], vm.AvailableExportFormats);
    }

    [Fact]
    public async Task ExportEncrypt_WhenDisabled_EnablesFormatSelectionAndShowsPlainFormats()
    {
        var (_, _, _, persistence, _, _, _, vm) = CreateSutWithDependencies();
        persistence.Setup(p => p.ReadCurrentGeneralSettings()).Returns(CreateSnapshot(exportEncrypt: true));
        await vm.LoadAsync();

        vm.ExportEncrypt = false;

        Assert.True(vm.IsExportFormatSelectionEnabled);
        Assert.Contains(ExportFileFormat.Json, vm.AvailableExportFormats);
        Assert.Contains(ExportFileFormat.Csv, vm.AvailableExportFormats);
        Assert.Contains(ExportFileFormat.Txt, vm.AvailableExportFormats);
        Assert.DoesNotContain(ExportFileFormat.Totp, vm.AvailableExportFormats);
        Assert.Equal(ExportFileFormat.Json, vm.SelectedExportFormat);
    }

    [Fact]
    public async Task OpenExportFileAfterExport_WhenEncrypted_RemainsFalse()
    {
        var (_, _, _, persistence, _, _, _, vm) = CreateSutWithDependencies();
        persistence.Setup(p => p.ReadCurrentGeneralSettings()).Returns(CreateSnapshot(exportEncrypt: true));
        await vm.LoadAsync();

        vm.OpenExportFileAfterExport = true;

        Assert.False(vm.OpenExportFileAfterExport);
        Assert.False(vm.IsOpenExportFileAfterExportOptionEnabled);
    }

    [Fact]
    public async Task CanResetToDefaults_WhenOnlyExportEncryptChanged_IsTrue()
    {
        var (_, _, _, persistence, _, _, _, vm) = CreateSutWithDependencies();
        persistence.Setup(p => p.ReadCurrentGeneralSettings()).Returns(CreateSnapshot(exportEncrypt: true));
        await vm.LoadAsync();

        Assert.False(vm.CanResetToDefaults);
        vm.ExportEncrypt = false;

        Assert.True(vm.CanResetToDefaults);
    }

    [Fact]
    public async Task ChangePasswordCommand_CanExecuteOnlyForPasswordGateWithInput()
    {
        var (_, _, _, persistence, _, _, _, vm) = CreateSutWithDependencies();
        persistence.Setup(p => p.ReadCurrentGeneralSettings()).Returns(CreateSnapshot());

        await vm.LoadAsync();

        Assert.False(vm.ChangePasswordCommand.CanExecute(null));

        vm.IsPasswordSelected = true;
        vm.NewPassword = "new-password";

        Assert.True(vm.ChangePasswordCommand.CanExecute(null));

        vm.IsPasswordSelected = false;

        Assert.False(vm.ChangePasswordCommand.CanExecute(null));
    }

    [Fact]
    public async Task ChangePasswordCommand_WhenSuccessful_ClearsInputsAndShowsSuccess()
    {
        var (_, _, authWorkflow, persistence, _, message, _, vm) = CreateSutWithDependencies();

        persistence.Setup(p => p.ReadCurrentGeneralSettings()).Returns(CreateSnapshot());
        authWorkflow.Setup(a => a.ChangePasswordAsync("new-pass", "new-pass"))
            .ReturnsAsync(new SettingsAuthorizationWorkflowResult(true, ClearPasswordInputs: true));

        await vm.LoadAsync();
        vm.IsPasswordSelected = true;
        vm.NewPassword = "new-pass";
        vm.ConfirmPassword = "new-pass";

        vm.ChangePasswordCommand.Execute(null);

        await WaitUntilAsync(() => string.IsNullOrEmpty(vm.NewPassword) && string.IsNullOrEmpty(vm.ConfirmPassword));

        Assert.Null(vm.AuthError);
        message.Verify(m => m.ShowSuccess(TOTP.Resources.UI.ui_Password_ChangeSuccess, 2), Times.Once);
    }

    [Fact]
    public async Task ChangePasswordCommand_WhenValidationFails_SetsInlineErrorsWithoutSuccessMessage()
    {
        var (_, _, authWorkflow, persistence, _, message, _, vm) = CreateSutWithDependencies();

        persistence.Setup(p => p.ReadCurrentGeneralSettings()).Returns(CreateSnapshot());
        authWorkflow.Setup(a => a.ChangePasswordAsync("short", "mismatch"))
            .ReturnsAsync(new SettingsAuthorizationWorkflowResult(
                false,
                ErrorMessage: "validation failed",
                NewPasswordError: "too short",
                ConfirmPasswordError: "not equal"));

        await vm.LoadAsync();
        vm.IsPasswordSelected = true;
        vm.NewPassword = "short";
        vm.ConfirmPassword = "mismatch";

        vm.ChangePasswordCommand.Execute(null);

        await WaitUntilAsync(() => vm.NewPasswordError is not null && vm.ConfirmPasswordError is not null);

        Assert.Equal("validation failed", vm.AuthError);
        Assert.Equal("too short", vm.NewPasswordError);
        Assert.Equal("not equal", vm.ConfirmPasswordError);
        message.Verify(m => m.ShowSuccess(It.IsAny<string>(), It.IsAny<int?>()), Times.Never);
    }

    [Fact]
    public async Task IsHelloSelected_WhenGateSaveFails_RollsBackSelectionAndSetsError()
    {
        var (settings, _, authWorkflow, persistence, _, _, _, vm) = CreateSutWithDependencies();

        var appSettings = new AppSettings
        {
            Authorization = new AuthorizationProfile { Gate = AuthorizationGateKind.Password }
        };

        settings.SetupGet(s => s.Current).Returns(appSettings);
        persistence.Setup(p => p.ReadCurrentGeneralSettings()).Returns(CreateSnapshot());
        authWorkflow.Setup(a => a.ApplyAuthorizationGateSelectionAsync(true, true))
            .ReturnsAsync(new SettingsAuthorizationWorkflowResult(false, ErrorMessage: "gate save failed"));

        await vm.LoadAsync();
        vm.IsHelloSelected = true;

        await WaitUntilAsync(() => vm.AuthError == "gate save failed");

        Assert.False(vm.IsHelloSelected);
        Assert.True(vm.IsPasswordSelected);
    }

    [Fact]
    public async Task ResetToDefaultsCommand_WhenExecuted_AppliesDefaultsAndSavesOnce()
    {
        var (_, _, _, persistence, _, _, _, vm) = CreateSutWithDependencies();

        persistence.Setup(p => p.ReadCurrentGeneralSettings()).Returns(CreateSnapshot(lockOnSessionLock: false));
        persistence.Setup(p => p.CreateDefaultGeneralSettings()).Returns(CreateSnapshot());
        persistence.Setup(p => p.SaveGeneralSettingsAsync(It.IsAny<SettingsGeneralSnapshot>()))
            .ReturnsAsync(new SettingsPersistenceResult(true));

        await vm.LoadAsync();

        Assert.True(vm.CanResetToDefaults);
        Assert.True(vm.ResetToDefaultsCommand.CanExecute(null));

        vm.ResetToDefaultsCommand.Execute(null);

        await WaitUntilAsync(() => vm.CanResetToDefaults == false);

        Assert.True(vm.LockOnSessionLock);
        persistence.Verify(p => p.SaveGeneralSettingsAsync(It.IsAny<SettingsGeneralSnapshot>()), Times.Once);
    }

    [Fact]
    public async Task ResetToDefaultsCommand_AfterPlainExportSelection_RestoresEncryptedTotpSelection()
    {
        var (_, _, _, persistence, _, _, _, vm) = CreateSutWithDependencies();

        persistence.Setup(p => p.ReadCurrentGeneralSettings()).Returns(CreateSnapshot(exportEncrypt: false));
        persistence.Setup(p => p.CreateDefaultGeneralSettings()).Returns(CreateSnapshot(exportEncrypt: true));
        persistence.Setup(p => p.SaveGeneralSettingsAsync(It.IsAny<SettingsGeneralSnapshot>()))
            .ReturnsAsync(new SettingsPersistenceResult(true));

        await vm.LoadAsync();
        vm.ExportEncrypt = false;
        vm.SelectedExportFormat = ExportFileFormat.Csv;

        vm.ResetToDefaultsCommand.Execute(null);

        await WaitUntilAsync(() => vm.ExportEncrypt && vm.SelectedExportFormat == ExportFileFormat.Totp);

        Assert.Equal([ExportFileFormat.Totp], vm.AvailableExportFormats);
        Assert.Equal(ExportFileFormat.Totp, vm.SelectedExportFormat);
    }

    [Fact]
    public async Task ResetToDefaultsCommand_WhenEncryptedAlreadyTrue_ReNotifiesSelectedExportFormat()
    {
        var (_, _, _, persistence, _, _, _, vm) = CreateSutWithDependencies();
        persistence.Setup(p => p.ReadCurrentGeneralSettings()).Returns(CreateSnapshot(exportEncrypt: true, lockOnSessionLock: false));
        persistence.Setup(p => p.CreateDefaultGeneralSettings()).Returns(CreateSnapshot(exportEncrypt: true));
        persistence.Setup(p => p.SaveGeneralSettingsAsync(It.IsAny<SettingsGeneralSnapshot>()))
            .ReturnsAsync(new SettingsPersistenceResult(true));

        await vm.LoadAsync();

        var raised = false;
        vm.PropertyChanged += (_, e) =>
        {
            if (e.PropertyName == nameof(SettingsViewModel.SelectedExportFormat))
            {
                raised = true;
            }
        };

        vm.ResetToDefaultsCommand.Execute(null);
        await WaitUntilAsync(() => !vm.CanResetToDefaults);

        Assert.Equal(ExportFileFormat.Totp, vm.SelectedExportFormat);
        Assert.True(raised);
    }

    [Fact]
    public async Task ExportAndImportCommands_ForwardSelectedOptionsToTransferWorkflow()
    {
        var (_, _, _, persistence, transfer, _, _, vm) = CreateSutWithDependencies();

        persistence.Setup(p => p.ReadCurrentGeneralSettings()).Returns(CreateSnapshot(exportEncrypt: false));
        transfer.Setup(t => t.ExportAsync(false, ExportFileFormat.Csv)).Returns(Task.CompletedTask);
        transfer.Setup(t => t.ImportAsync(ImportConflictStrategy.KeepBoth)).Returns(Task.CompletedTask);

        await vm.LoadAsync();

        vm.ExportEncrypt = false;
        vm.SelectedExportFormat = ExportFileFormat.Csv;
        vm.SelectedImportConflictOption = vm.AvailableImportConflictOptions
            .Single(x => x.Strategy == ImportConflictStrategy.KeepBoth);

        vm.ExportCommand.Execute(null);
        vm.ImportCommand.Execute(null);

        await WaitUntilAsync(() => true, 120);

        transfer.Verify(t => t.ExportAsync(false, ExportFileFormat.Csv), Times.Once);
        transfer.Verify(t => t.ImportAsync(ImportConflictStrategy.KeepBoth), Times.Once);
    }

    [Fact]
    public async Task SaveCommand_WhenAuthorizationFails_DoesNotInvokeSaveAction()
    {
        var (_, _, authWorkflow, persistence, _, _, _, vm, saveActionCount) = CreateSutWithSaveCounter();

        persistence.Setup(p => p.ReadCurrentGeneralSettings()).Returns(CreateSnapshot());
        authWorkflow.Setup(a => a.ApplyAuthorizationSettingsAsync(It.IsAny<bool>(), It.IsAny<bool>(), It.IsAny<string>(), It.IsAny<string>()))
            .ReturnsAsync(new SettingsAuthorizationWorkflowResult(false, ErrorMessage: "auth failed"));

        await vm.LoadAsync();

        vm.SaveCommand.Execute(null);

        await WaitUntilAsync(() => vm.AuthError == "auth failed");

        Assert.Equal(0, saveActionCount());
        persistence.Verify(p => p.SaveGeneralSettingsAsync(It.IsAny<SettingsGeneralSnapshot>()), Times.Never);
    }

    [Fact]
    public async Task SaveCommand_WhenAuthorizationSucceeds_SavesGeneralSettingsAndInvokesSaveAction()
    {
        var (_, _, authWorkflow, persistence, _, _, _, vm, saveActionCount) = CreateSutWithSaveCounter();

        persistence.Setup(p => p.ReadCurrentGeneralSettings()).Returns(CreateSnapshot());
        authWorkflow.Setup(a => a.ApplyAuthorizationSettingsAsync(It.IsAny<bool>(), It.IsAny<bool>(), It.IsAny<string>(), It.IsAny<string>()))
            .ReturnsAsync(new SettingsAuthorizationWorkflowResult(true));
        persistence.Setup(p => p.SaveGeneralSettingsAsync(It.IsAny<SettingsGeneralSnapshot>()))
            .ReturnsAsync(new SettingsPersistenceResult(true));

        await vm.LoadAsync();

        vm.SaveCommand.Execute(null);

        await WaitUntilAsync(() => saveActionCount() == 1);

        persistence.Verify(p => p.SaveGeneralSettingsAsync(It.IsAny<SettingsGeneralSnapshot>()), Times.Once);
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

    private static SettingsGeneralSnapshot CreateSnapshot(
        AppLogLevel selectedLogLevel = AppLogLevel.Information,
        bool lockOnSessionLock = true,
        bool lockOnMinimize = true,
        bool lockOnIdleTimeout = true,
        int idleTimeoutMinutes = 10,
        bool clearClipboardEnabled = true,
        int clearClipboardSeconds = 15,
        double qrPreviewScaleFactor = 1.5,
        bool exportEncrypt = true,
        bool openExportFileAfterExportBeforeEncrypt = true,
        bool hideSecretsByDefault = true)
    {
        return new SettingsGeneralSnapshot(
            selectedLogLevel,
            lockOnSessionLock,
            lockOnMinimize,
            lockOnIdleTimeout,
            idleTimeoutMinutes,
            clearClipboardEnabled,
            clearClipboardSeconds,
            qrPreviewScaleFactor,
            exportEncrypt,
            openExportFileAfterExportBeforeEncrypt,
            hideSecretsByDefault);
    }

    private static (Mock<ISettingsService> settings,
        Mock<IAuthorizationService> auth,
        Mock<ISettingsAuthorizationWorkflowService> authWorkflow,
        Mock<ISettingsPersistenceService> persistence,
        Mock<ISettingsTransferWorkflowService> transfer,
        Mock<IMessageService> message,
        Mock<ILogSwitchService> logSwitch,
        SettingsViewModel vm)
        CreateSutWithDependencies()
    {
        var settings = new Mock<ISettingsService>();
        var auth = new Mock<IAuthorizationService>();
        var authWorkflow = new Mock<ISettingsAuthorizationWorkflowService>();
        var persistence = new Mock<ISettingsPersistenceService>();
        var transfer = new Mock<ISettingsTransferWorkflowService>();
        var message = new Mock<IMessageService>();
        var logSwitch = new Mock<ILogSwitchService>();

        settings.SetupGet(s => s.Current).Returns(new AppSettings());
        auth.Setup(a => a.IsHelloAvailableAsync()).ReturnsAsync(true);
        persistence.Setup(p => p.ReadCurrentGeneralSettings()).Returns(CreateSnapshot());
        persistence.Setup(p => p.SaveGeneralSettingsAsync(It.IsAny<SettingsGeneralSnapshot>()))
            .ReturnsAsync(new SettingsPersistenceResult(true));
        logSwitch.SetupGet(l => l.MinimumLevel).Returns(AppLogLevel.Information);
        logSwitch.SetupGet(l => l.IsCliOverrideActive).Returns(false);

        var vm = CreateSut(settings, auth, authWorkflow, persistence, transfer, message, logSwitch);
        return (settings, auth, authWorkflow, persistence, transfer, message, logSwitch, vm);
    }

    private static (Mock<ISettingsService> settings,
        Mock<IAuthorizationService> auth,
        Mock<ISettingsAuthorizationWorkflowService> authWorkflow,
        Mock<ISettingsPersistenceService> persistence,
        Mock<ISettingsTransferWorkflowService> transfer,
        Mock<IMessageService> message,
        Mock<ILogSwitchService> logSwitch,
        SettingsViewModel vm,
        Func<int> saveActionCount)
        CreateSutWithSaveCounter()
    {
        var deps = CreateSutWithDependencies();
        var saveActionCount = 0;

        var vm = new SettingsViewModel(
            deps.settings.Object,
            deps.auth.Object,
            deps.authWorkflow.Object,
            deps.persistence.Object,
            deps.transfer.Object,
            deps.message.Object,
            deps.logSwitch.Object,
            new RelayCommand(() => { }),
            () => saveActionCount++);

        return (deps.settings, deps.auth, deps.authWorkflow, deps.persistence, deps.transfer, deps.message, deps.logSwitch, vm, () => saveActionCount);
    }

    private static SettingsViewModel CreateSut(
        Mock<ISettingsService> settings,
        Mock<IAuthorizationService> auth,
        Mock<ISettingsAuthorizationWorkflowService> authWorkflow,
        Mock<ISettingsPersistenceService> persistence,
        Mock<ISettingsTransferWorkflowService> transfer,
        Mock<IMessageService> message,
        Mock<ILogSwitchService> logSwitch)
    {
        return new SettingsViewModel(
            settings.Object,
            auth.Object,
            authWorkflow.Object,
            persistence.Object,
            transfer.Object,
            message.Object,
            logSwitch.Object,
            new RelayCommand(() => { }),
            () => { });
    }
}
