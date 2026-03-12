using FluentResults;
using Microsoft.Extensions.Logging.Abstractions;
using Moq;
using System.Collections.ObjectModel;
using System.Windows.Input;
using TOTP.Commands;
using TOTP.Core.Common;
using TOTP.Core.Enums;
using TOTP.Core.Interfaces;
using TOTP.Core.Models;
using TOTP.Core.Security;
using TOTP.Core.Security.Interfaces;
using TOTP.Core.Security.Models;
using TOTP.Core.Services.Interfaces;
using TOTP.Infrastructure.Services;
using TOTP.Security.Interfaces;
using TOTP.Services.Interfaces;
using TOTP.ViewModels;
using TOTP.Views.Interfaces;

namespace TOTP.Tests.ViewModels;

public sealed class MainViewModelTests : IDisposable
{
    private readonly string _settingsPath = StringsConstants.AppSettingsJsonFilePath;
    private readonly string? _settingsBackup;

    public MainViewModelTests()
    {
        _settingsBackup = File.Exists(_settingsPath) ? File.ReadAllText(_settingsPath) : null;
        File.WriteAllText(_settingsPath, """{"Localization":{"Culture":"en"}}""");
    }

    [Fact]
    public async Task InitializeMainViewAsync_InitializesSessionAndClearsBusy()
    {
        using var ctx = new MainVmTestContext();
        var mainWindow = new Mock<IMainWindow>();

        await ctx.Sut.InitializeMainViewAsync(mainWindow.Object);

        Assert.False(ctx.Sut.IsBusy);
        Assert.Null(ctx.Sut.SettingsVm);
        ctx.Session.Verify(s => s.InitializeAsync(mainWindow.Object), Times.Once);
    }

    [Fact]
    public async Task OnUnlockedCallback_LoadsTokensOnlyOnce()
    {
        using var ctx = new MainVmTestContext();
        var loaded = new ObservableCollection<OtpViewModel>
        {
            new(Guid.NewGuid(), "GitHub", "JBSWY3DPEHPK3PXP")
        };
        ctx.AccountsWorkflow.Setup(s => s.LoadAllAsync()).ReturnsAsync(Result.Ok(loaded));

        await ctx.InvokeUnlockedAsync();
        await ctx.InvokeUnlockedAsync();

        Assert.Single(ctx.Sut.AllOtps);
        ctx.AccountsWorkflow.Verify(s => s.LoadAllAsync(), Times.Once);
    }

    [Fact]
    public async Task OnLockedCallback_ResetsUiAndClearsTokens()
    {
        using var ctx = new MainVmTestContext();
        var otp = new OtpViewModel(Guid.NewGuid(), "GitHub", "JBSWY3DPEHPK3PXP");
        ctx.AccountsWorkflow.Setup(s => s.LoadAllAsync()).ReturnsAsync(Result.Ok(new ObservableCollection<OtpViewModel> { otp }));
        await ctx.InvokeUnlockedAsync();

        ctx.Sut.IsSecretVisible = true;
        ctx.Sut.IsGridEditing = true;
        ctx.Sut.IsInlineEditing = true;
        ctx.Sut.SelectedAccount = otp;
        ctx.Sut.OpenFlyoutAddMode();
        ctx.Sut.SearchText = "github";
        ctx.Sut.IsSearchVisible = true;

        ctx.InvokeLocked();

        Assert.Empty(ctx.Sut.AllOtps);
        Assert.False(ctx.Sut.IsSecretVisible);
        Assert.False(ctx.Sut.IsGridEditing);
        Assert.False(ctx.Sut.IsInlineEditing);
        Assert.False(ctx.Sut.IsSettingsViewOpen);
        Assert.Null(ctx.Sut.SelectedAccount);
        Assert.False(ctx.Sut.IsEditAddFlyoutOpen);
        ctx.QrPreview.Verify(q => q.Close(), Times.Once);
    }

    [Fact]
    public async Task AddOrUpdateAccountAsync_AddMode_WhenValid_AddsAndClosesFlyout()
    {
        using var ctx = new MainVmTestContext();
        ctx.AccountsWorkflow.Setup(s => s.ValidateForCreate(It.IsAny<OtpViewModel>(), It.IsAny<IEnumerable<OtpViewModel>>()))
            .Returns([]);
        ctx.AccountsWorkflow.Setup(s => s.AddAsync(It.IsAny<OtpViewModel>())).ReturnsAsync(Result.Ok());

        ctx.Sut.OpenFlyoutAddMode();
        Assert.NotNull(ctx.Sut.CurrentSecretBeingEditedOrAdded);
        ctx.Sut.CurrentSecretBeingEditedOrAdded!.Issuer = "GitHub";
        ctx.Sut.CurrentSecretBeingEditedOrAdded!.Secret = "JBSWY3DPEHPK3PXP";

        await ctx.Sut.AddOrUpdateAccountAsync();

        Assert.Single(ctx.Sut.AllOtps);
        Assert.False(ctx.Sut.IsEditAddFlyoutOpen);
        ctx.AccountsWorkflow.Verify(s => s.AddAsync(It.IsAny<OtpViewModel>()), Times.Once);
    }

    [Fact]
    public async Task AddOrUpdateAccountAsync_AddMode_WhenValidationFails_DoesNotAdd()
    {
        using var ctx = new MainVmTestContext();
        ctx.AccountsWorkflow.Setup(s => s.ValidateForCreate(It.IsAny<OtpViewModel>(), It.IsAny<IEnumerable<OtpViewModel>>()))
            .Returns([ValidationError.PlatformRequired]);

        ctx.Sut.OpenFlyoutAddMode();
        Assert.NotNull(ctx.Sut.CurrentSecretBeingEditedOrAdded);
        ctx.Sut.CurrentSecretBeingEditedOrAdded!.Issuer = "";
        ctx.Sut.CurrentSecretBeingEditedOrAdded!.Secret = "invalid";

        await ctx.Sut.AddOrUpdateAccountAsync();

        Assert.Empty(ctx.Sut.AllOtps);
        Assert.True(ctx.Sut.IsEditAddFlyoutOpen);
        ctx.AccountsWorkflow.Verify(s => s.AddAsync(It.IsAny<OtpViewModel>()), Times.Never);
    }

    [Fact]
    public async Task UpdateAccountAsync_WhenSuccess_UpdatesEntryAndClearsPreviousVersion()
    {
        using var ctx = new MainVmTestContext();
        var id = Guid.NewGuid();
        var existing = new OtpViewModel(id, "GitHub", "AAAA", "john");
        var updated = new OtpViewModel(id, "GitHub", "BBBB", "john.doe");
        ctx.Sut.AllOtps.Add(existing);
        ctx.Sut.PreviousVersion = existing.Copy();
        ctx.Sut.ShowGenerateQrCodeLink = true; // skip qr image refresh branch
        ctx.AccountsWorkflow.Setup(s => s.UpdateAsync(It.IsAny<OtpViewModel>(), updated)).ReturnsAsync(Result.Ok());

        await ctx.Sut.UpdateAccountAsync(updated);

        Assert.Equal("BBBB", existing.Secret);
        Assert.Equal("john.doe", existing.AccountName);
        Assert.Null(ctx.Sut.PreviousVersion);
    }

    [Fact]
    public async Task DeleteAccountAsync_WhenConfirmed_RemovesItem()
    {
        using var ctx = new MainVmTestContext();
        var item = new OtpViewModel(Guid.NewGuid(), "GitHub", "JBSWY3DPEHPK3PXP");
        ctx.Sut.AllOtps.Add(item);
        ctx.Sut.SelectedAccount = item;
        ctx.Message.Setup(m => m.ConfirmWarning(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>())).Returns(true);
        ctx.AccountsWorkflow.Setup(s => s.DeleteAsync(item)).ReturnsAsync(Result.Ok());

        await ctx.Sut.DeleteAccountAsync(item);

        Assert.Empty(ctx.Sut.AllOtps);
        ctx.AccountsWorkflow.Verify(s => s.DeleteAsync(item), Times.Once);
    }

    [Fact]
    public async Task OnRowSelectionChangedAsync_WhenValid_ComputesAndCopiesCode()
    {
        using var ctx = new MainVmTestContext(clearClipboardEnabled: false);
        var item = new OtpViewModel(Guid.NewGuid(), "GitHub", "JBSWY3DPEHPK3PXP");

        await ctx.Sut.OnRowSelectionChangedAsync(item);

        Assert.Same(item, ctx.Sut.SelectedAccount);
        Assert.False(string.IsNullOrWhiteSpace(ctx.Sut.TotpCode));
        ctx.Clipboard.Verify(c => c.SetText(It.IsAny<string>()), Times.AtLeastOnce);
    }

    [Fact]
    public void CopyTotpCodeToClipboard_WhenAutoClearEnabled_UsesScheduledClear()
    {
        using var ctx = new MainVmTestContext(clearClipboardEnabled: true, clearSeconds: 42);
        ctx.Sut.TotpCode = "123456";

        ctx.Sut.CopyTotpCodeToClipboard();

        ctx.Clipboard.Verify(c => c.CopyAndScheduleClear("123456", TimeSpan.FromSeconds(42)), Times.Once);
    }

    [Fact]
    public void SearchText_WhenChanged_TriggersDebounce()
    {
        using var ctx = new MainVmTestContext();

        ctx.Sut.SearchText = "git";

        ctx.Debounce.Verify(d => d.Debounce("Search", 300, It.IsAny<Action>()), Times.Once);
    }

    [Fact]
    public async Task AddOrUpdateAccountAsync_EditMode_WhenValidationFails_DoesNotUpdateOrCloseFlyout()
    {
        using var ctx = new MainVmTestContext();
        var existing = new OtpViewModel(Guid.NewGuid(), "GitHub", "JBSWY3DPEHPK3PXP");
        ctx.Sut.AllOtps.Add(existing);
        ctx.AccountsWorkflow.Setup(s => s.ValidateForUpdate(It.IsAny<OtpViewModel>(), It.IsAny<IEnumerable<OtpViewModel>>()))
            .Returns([ValidationError.PlatformRequired]);

        ctx.Sut.OpenFlyoutEditMode(existing);
        Assert.NotNull(ctx.Sut.CurrentSecretBeingEditedOrAdded);
        ctx.Sut.CurrentSecretBeingEditedOrAdded!.Issuer = string.Empty;

        await ctx.Sut.AddOrUpdateAccountAsync();

        Assert.True(ctx.Sut.IsEditAddFlyoutOpen);
        ctx.AccountsWorkflow.Verify(s => s.UpdateAsync(It.IsAny<OtpViewModel?>(), It.IsAny<OtpViewModel>()), Times.Never);
    }

    [Fact]
    public async Task AddOrUpdateAccountAsync_EditMode_WhenValid_UpdatesAndClosesFlyout()
    {
        using var ctx = new MainVmTestContext();
        var id = Guid.NewGuid();
        var existing = new OtpViewModel(id, "GitHub", "AAAA", "john");
        ctx.Sut.AllOtps.Add(existing);
        ctx.Sut.ShowGenerateQrCodeLink = true;
        ctx.AccountsWorkflow.Setup(s => s.ValidateForUpdate(It.IsAny<OtpViewModel>(), It.IsAny<IEnumerable<OtpViewModel>>()))
            .Returns([]);
        ctx.AccountsWorkflow.Setup(s => s.UpdateAsync(It.IsAny<OtpViewModel?>(), It.IsAny<OtpViewModel>()))
            .ReturnsAsync(Result.Ok());

        ctx.Sut.OpenFlyoutEditMode(existing);
        Assert.NotNull(ctx.Sut.CurrentSecretBeingEditedOrAdded);
        ctx.Sut.CurrentSecretBeingEditedOrAdded!.Secret = "BBBB";

        await ctx.Sut.AddOrUpdateAccountAsync();

        Assert.False(ctx.Sut.IsEditAddFlyoutOpen);
        Assert.Null(ctx.Sut.CurrentSecretBeingEditedOrAdded);
        Assert.Equal("BBBB", existing.Secret);
        ctx.AccountsWorkflow.Verify(s => s.UpdateAsync(It.IsAny<OtpViewModel?>(), It.Is<OtpViewModel>(o => o.ID == id)), Times.Once);
    }

    [Fact]
    public void CancelFlyoutCommand_ClearsTransientEditorSecret()
    {
        using var ctx = new MainVmTestContext();
        ctx.Sut.OpenFlyoutAddMode();
        var editor = ctx.Sut.CurrentSecretBeingEditedOrAdded;
        Assert.NotNull(editor);
        editor!.Secret = "JBSWY3DPEHPK3PXP";

        ctx.Sut.CancelFlyoutCommand.Execute(null);

        Assert.Null(ctx.Sut.CurrentSecretBeingEditedOrAdded);
        Assert.Equal(string.Empty, editor.Secret);
    }

    [Fact]
    public async Task DeleteAccountAsync_WhenNotConfirmed_DoesNotDelete()
    {
        using var ctx = new MainVmTestContext();
        var item = new OtpViewModel(Guid.NewGuid(), "GitHub", "JBSWY3DPEHPK3PXP");
        ctx.Sut.AllOtps.Add(item);
        ctx.Message.Setup(m => m.ConfirmWarning(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>())).Returns(false);

        await ctx.Sut.DeleteAccountAsync(item);

        Assert.Single(ctx.Sut.AllOtps);
        ctx.AccountsWorkflow.Verify(s => s.DeleteAsync(It.IsAny<OtpViewModel>()), Times.Never);
    }

    [Fact]
    public async Task OnRowSelectionChangedAsync_WhenGridEditing_ReturnsWithoutSelection()
    {
        using var ctx = new MainVmTestContext(clearClipboardEnabled: false);
        var item = new OtpViewModel(Guid.NewGuid(), "GitHub", "JBSWY3DPEHPK3PXP");
        ctx.Sut.IsGridEditing = true;

        await ctx.Sut.OnRowSelectionChangedAsync(item);

        Assert.Null(ctx.Sut.SelectedAccount);
        ctx.Clipboard.Verify(c => c.SetText(It.IsAny<string>()), Times.Never);
    }

    [Fact]
    public void CopyTotpCodeToClipboard_WhenAutoClearSecondsInvalid_UsesDefault15Seconds()
    {
        using var ctx = new MainVmTestContext(clearClipboardEnabled: true, clearSeconds: 0);
        ctx.Sut.TotpCode = "123456";

        ctx.Sut.CopyTotpCodeToClipboard();

        ctx.Clipboard.Verify(c => c.CopyAndScheduleClear("123456", TimeSpan.FromSeconds(15)), Times.Once);
    }

    [Fact]
    public void ToggleSearchBoxCommand_WhenClosing_ClearsSearchText()
    {
        using var ctx = new MainVmTestContext();
        ctx.Sut.SearchText = "github";
        ctx.Sut.IsSearchVisible = true;

        ctx.Sut.ToggleSearchBoxCommand.Execute(null);

        Assert.False(ctx.Sut.IsSearchVisible);
        Assert.Equal(string.Empty, ctx.Sut.SearchText);
    }

    [Fact]
    public void ToggleSearchBoxCommand_CanExecute_TracksGridEditingState()
    {
        using var ctx = new MainVmTestContext();
        Assert.True(ctx.Sut.ToggleSearchBoxCommand.CanExecute(null));

        ctx.Sut.IsGridEditing = true;

        Assert.False(ctx.Sut.ToggleSearchBoxCommand.CanExecute(null));
    }

    [Fact]
    public void ToggleSearchBoxCommand_WhenOpening_SetsVisibleAndFocus()
    {
        using var ctx = new MainVmTestContext();
        Assert.False(ctx.Sut.IsSearchVisible);
        Assert.False(ctx.Sut.IsSearchFocused);

        ctx.Sut.ToggleSearchBoxCommand.Execute(null);

        Assert.True(ctx.Sut.IsSearchVisible);
        Assert.True(ctx.Sut.IsSearchFocused);
    }

    [Fact]
    public void ClearSearchCommand_WhenExecuted_ClearsSearchTextAndKeepsVisibleState()
    {
        using var ctx = new MainVmTestContext();
        ctx.Sut.IsSearchVisible = true;
        ctx.Sut.SearchText = "github";

        ctx.Sut.ClearSearchCommand.Execute(null);

        Assert.True(ctx.Sut.IsSearchVisible);
        Assert.Equal(string.Empty, ctx.Sut.SearchText);
        Assert.True(ctx.Sut.IsSearchFocused);
    }

    [Fact]
    public async Task AddOrUpdateAccountAsync_AddMode_WhenCurrentItemMissing_DoesNothing()
    {
        using var ctx = new MainVmTestContext();
        ctx.Sut.OpenFlyoutAddMode();
        ctx.Sut.CurrentSecretBeingEditedOrAdded = null;

        await ctx.Sut.AddOrUpdateAccountAsync();

        Assert.Empty(ctx.Sut.AllOtps);
        ctx.AccountsWorkflow.Verify(s => s.AddAsync(It.IsAny<OtpViewModel>()), Times.Never);
    }

    [Fact]
    public async Task AddOrUpdateAccountAsync_EditMode_WhenCurrentItemMissing_DoesNothing()
    {
        using var ctx = new MainVmTestContext();
        var existing = new OtpViewModel(Guid.NewGuid(), "GitHub", "AAAA");
        ctx.Sut.AllOtps.Add(existing);
        ctx.Sut.OpenFlyoutEditMode(existing);
        ctx.Sut.CurrentSecretBeingEditedOrAdded = null;

        await ctx.Sut.AddOrUpdateAccountAsync();

        ctx.AccountsWorkflow.Verify(s => s.UpdateAsync(It.IsAny<OtpViewModel?>(), It.IsAny<OtpViewModel>()), Times.Never);
    }

    [Fact]
    public async Task DeleteAccountAsync_WhenItemNull_DoesNothing()
    {
        using var ctx = new MainVmTestContext();

        await ctx.Sut.DeleteAccountAsync(null);

        ctx.Message.Verify(m => m.ConfirmWarning(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>()), Times.Never);
        ctx.AccountsWorkflow.Verify(s => s.DeleteAsync(It.IsAny<OtpViewModel>()), Times.Never);
    }

    [Fact]
    public async Task DeleteAccountAsync_WhenDeleteFails_ShowsResultErrorAndKeepsItem()
    {
        using var ctx = new MainVmTestContext();
        var item = new OtpViewModel(Guid.NewGuid(), "GitHub", "JBSWY3DPEHPK3PXP");
        ctx.Sut.AllOtps.Add(item);
        ctx.Message.Setup(m => m.ConfirmWarning(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>())).Returns(true);
        ctx.AccountsWorkflow.Setup(s => s.DeleteAsync(item)).ReturnsAsync(Result.Fail("delete failed"));

        await ctx.Sut.DeleteAccountAsync(item);

        Assert.Single(ctx.Sut.AllOtps);
        ctx.Message.Verify(m => m.ShowResultError(It.IsAny<Result>(), item.Issuer), Times.Once);
    }

    [Fact]
    public async Task UpdateAccountAsync_WhenFailed_DoesNotMutateEntryOrClearPreviousVersion()
    {
        using var ctx = new MainVmTestContext();
        var id = Guid.NewGuid();
        var existing = new OtpViewModel(id, "GitHub", "AAAA", "john");
        var updated = new OtpViewModel(id, "GitHub", "BBBB", "john.doe");
        ctx.Sut.AllOtps.Add(existing);
        ctx.Sut.PreviousVersion = existing.Copy();
        ctx.AccountsWorkflow.Setup(s => s.UpdateAsync(It.IsAny<OtpViewModel?>(), updated)).ReturnsAsync(Result.Fail("update failed"));

        await ctx.Sut.UpdateAccountAsync(updated);

        Assert.Equal("AAAA", existing.Secret);
        Assert.Equal("john", existing.AccountName);
        Assert.NotNull(ctx.Sut.PreviousVersion);
        ctx.Message.Verify(m => m.ShowResultError(It.IsAny<Result>(), updated.Issuer), Times.Once);
    }

    [Fact]
    public async Task OnRowSelectionChangedAsync_WhenItemNull_DoesNothing()
    {
        using var ctx = new MainVmTestContext(clearClipboardEnabled: false);

        await ctx.Sut.OnRowSelectionChangedAsync(null);

        Assert.Null(ctx.Sut.SelectedAccount);
        Assert.Equal(string.Empty, ctx.Sut.TotpCode);
        ctx.Clipboard.Verify(c => c.SetText(It.IsAny<string>()), Times.Never);
    }

    [Fact]
    public void OpenFlyoutAddModeCommand_CanExecute_TracksGridEditingState()
    {
        using var ctx = new MainVmTestContext();
        Assert.True(ctx.Sut.OpenFlyoutAddModeCommand.CanExecute(null));

        ctx.Sut.IsGridEditing = true;

        Assert.False(ctx.Sut.OpenFlyoutAddModeCommand.CanExecute(null));
    }

    [Fact]
    public void ClearSearchCommand_CanExecute_TracksSearchVisibility()
    {
        using var ctx = new MainVmTestContext();
        Assert.False(ctx.Sut.ClearSearchCommand.CanExecute(null));

        ctx.Sut.IsSearchVisible = true;

        Assert.True(ctx.Sut.ClearSearchCommand.CanExecute(null));
    }

    [Fact]
    public void ExportSecretsCommand_CanExecute_TracksUnlockEditAndDataState()
    {
        using var ctx = new MainVmTestContext();
        var item = new OtpViewModel(Guid.NewGuid(), "GitHub", "JBSWY3DPEHPK3PXP");

        Assert.False(ctx.Sut.ExportSecretsCommand.CanExecute(null));

        ctx.Sut.AllOtps.Add(item);
        Assert.True(ctx.Sut.ExportSecretsCommand.CanExecute(null));

        ctx.Sut.IsGridEditing = true;
        Assert.False(ctx.Sut.ExportSecretsCommand.CanExecute(null));

        ctx.Sut.IsGridEditing = false;
        ctx.InvokeLocked();
        Assert.False(ctx.Sut.ExportSecretsCommand.CanExecute(null));
    }

    [Fact]
    public void CopyCodeCommand_CanExecute_RequiresSelectionAndCode()
    {
        using var ctx = new MainVmTestContext();
        var item = new OtpViewModel(Guid.NewGuid(), "GitHub", "JBSWY3DPEHPK3PXP");

        Assert.False(ctx.Sut.CopyCodeCommand.CanExecute(item));

        ctx.Sut.SelectedAccount = item;
        Assert.False(ctx.Sut.CopyCodeCommand.CanExecute(item));

        ctx.Sut.TotpCode = "123456";
        Assert.True(ctx.Sut.CopyCodeCommand.CanExecute(item));
    }

    [Fact]
    public void DeleteSecretCommand_CanExecute_RespectsGridEditMode()
    {
        using var ctx = new MainVmTestContext();
        var item = new OtpViewModel(Guid.NewGuid(), "GitHub", "JBSWY3DPEHPK3PXP");

        Assert.True(ctx.Sut.DeleteSecretCommand.CanExecute(item));

        ctx.Sut.IsGridEditing = true;
        Assert.False(ctx.Sut.DeleteSecretCommand.CanExecute(item));
    }

    [Fact]
    public void OpenSettingsCommand_CanExecute_RespectsLockState()
    {
        using var ctx = new MainVmTestContext();

        Assert.True(ctx.Sut.OpenSettingsCommand.CanExecute(null));

        ctx.Session.Setup(s => s.IsUnlocked).Returns(false);

        Assert.False(ctx.Sut.OpenSettingsCommand.CanExecute(null));
    }

    public void Dispose()
    {
        try
        {
            if (_settingsBackup is null)
            {
                if (File.Exists(_settingsPath))
                {
                    File.Delete(_settingsPath);
                }
            }
            else
            {
                File.WriteAllText(_settingsPath, _settingsBackup);
            }
        }
        catch
        {
            // best-effort cleanup
        }
    }

    private sealed class MainVmTestContext : IDisposable
    {
        public MainVmTestContext(bool clearClipboardEnabled = true, int clearSeconds = 15)
        {
            var appSettings = new AppSettings
            {
                ClearClipboardEnabled = clearClipboardEnabled,
                ClearClipboardSeconds = clearSeconds
            };

            SettingsService.SetupGet(s => s.Current).Returns(appSettings);
            LogSwitch.SetupGet(l => l.MinimumLevel).Returns(AppLogLevel.Information);
            LogSwitch.Setup(l => l.GetLevel()).Returns(AppLogLevel.Information);

            Session.Setup(s => s.IsUnlocked).Returns(true);
            Session.Setup(s => s.ConfigureCallbacks(It.IsAny<Func<Task>>(), It.IsAny<Action>()))
                .Callback<Func<Task>, Action>((u, l) =>
                {
                    Unlocked = u;
                    Locked = l;
                });

            var settingsVm = CreateSettingsViewModel();
            var unlockVm = CreateUnlockViewModel();
            SettingsDialog.Setup(s => s.CreateAndLoadAsync(It.IsAny<ICommand>(), It.IsAny<Action>(), It.IsAny<IAccountsCollectionContext>()))
                .ReturnsAsync(settingsVm);

            Sut = new MainViewModel(
                NullLogger<MainViewModel>.Instance,
                QrCode.Object,
                ExportService.Object,
                Message.Object,
                Clipboard.Object,
                AccountsWorkflow.Object,
                TransferWorkflow.Object,
                Debounce.Object,
                FileDialogs.Object,
                PasswordPrompt.Object,
                QrPreview.Object,
                ScannerWarmup.Object,
                AutoUpdate.Object,
                Session.Object,
                unlockVm,
                () => QrScannerDialog.Object,
                SettingsDialog.Object,
                SettingsService.Object);
        }

        public MainViewModel Sut { get; }
        public Mock<IAccountsWorkflowService> AccountsWorkflow { get; } = new();
        public Mock<IMessageService> Message { get; } = new();
        public Mock<IClipboardService> Clipboard { get; } = new();
        public Mock<IDebounceService> Debounce { get; } = new();
        public Mock<IQrPreviewService> QrPreview { get; } = new();
        public Mock<IScannerWarmupService> ScannerWarmup { get; } = new();
        public Mock<IAutoUpdateService> AutoUpdate { get; } = new();
        public Mock<ISettingsDialogOrchestrationService> SettingsDialog { get; } = new();
        public Mock<IMainViewSessionController> Session { get; } = new();
        public Mock<ISettingsService> SettingsService { get; } = new();

        private Mock<IQrCodeService> QrCode { get; } = new();
        private Mock<IExportService> ExportService { get; } = new();
        private Mock<IAccountTransferWorkflowService> TransferWorkflow { get; } = new();
        private Mock<IFileDialogService> FileDialogs { get; } = new();
        private Mock<IPasswordPromptService> PasswordPrompt { get; } = new();
        private Mock<IQrScannerDialogService> QrScannerDialog { get; } = new();
        private Mock<ILogSwitchService> LogSwitch { get; } = new();

        private Mock<IAuthorizationService> Authorization { get; } = new();
        private Mock<ISettingsAuthorizationWorkflowService> SettingsAuthWorkflow { get; } = new();
        private Mock<ISettingsPersistenceService> SettingsPersistence { get; } = new();
        private Mock<ISettingsTransferWorkflowService> SettingsTransferWorkflow { get; } = new();

        public Func<Task> Unlocked { get; private set; } = () => Task.CompletedTask;
        public Action Locked { get; private set; } = () => { };

        public Task InvokeUnlockedAsync() => Unlocked();
        public void InvokeLocked() => Locked();

        public void Dispose()
        {
            Sut.TotpUiTimer?.Dispose();
            Sut.TotpUiTimer = null;
        }

        private SettingsViewModel CreateSettingsViewModel()
        {
            return new SettingsViewModel(
                SettingsService.Object,
                Authorization.Object,
                SettingsAuthWorkflow.Object,
                SettingsPersistence.Object,
                SettingsTransferWorkflow.Object,
                AutoUpdate.Object,
                Message.Object,
                LogSwitch.Object,
                new RelayCommand(() => { }),
                () => { });
        }

        private UnlockViewModel CreateUnlockViewModel()
        {
            var state = new AuthorizationState();
            Authorization.SetupGet(a => a.State).Returns(state);

            var hello = new HelloUnlockViewModel(Authorization.Object);
            var pwd = new PasswordUnlockViewModel(
                Authorization.Object,
                Mock.Of<IPasswordValidationService>(),
                NullLogger<PasswordUnlockViewModel>.Instance);

            return new UnlockViewModel(Authorization.Object, hello, pwd, SettingsService.Object);
        }
    }
}
