using System.Globalization;
using FluentResults;
using Moq;
using TOTP.Avalonia.Mobile.Localization;
using TOTP.Avalonia.Mobile.Presentation;
using TOTP.Core.Models;
using TOTP.Core.Security;
using TOTP.Core.Security.Interfaces;
using TOTP.Core.Security.Models;
using TOTP.Core.Services.Interfaces;
using TOTP.Core.Services.Models;

namespace TOTP.Tests.Avalonia.Mobile;

public sealed class MobileShellViewModelTests
{
    private const string ValidSecret = "JBSWY3DPEHPK3PXP";

    [Fact]
    public async Task InitializeAsync_WhenNoEnvelopeExists_ShowsPasswordSetup()
    {
        var context = CreateContext(isConfigured: false);

        await context.Sut.InitializeAsync();

        Assert.True(context.Sut.IsSetupVisible);
        Assert.False(context.Sut.IsUnlockVisible);
        Assert.Empty(context.Sut.NotificationText);
    }

    [Fact]
    public async Task ConfigureAsync_WhenSuccessful_OpensEmptyEncryptedVault()
    {
        var context = CreateContext(isConfigured: false);
        context.Authorization
            .Setup(value => value.ConfigurePasswordAsync("synthetic password", "synthetic password"))
            .ReturnsAsync(AuthorizationResult.Success);
        await context.Sut.InitializeAsync();
        context.Sut.SetupPassword = "synthetic password";
        context.Sut.SetupConfirmation = "synthetic password";

        await context.Sut.ConfigureAsync();

        Assert.True(context.Sut.IsAccountsVisible);
        Assert.True(context.Sut.HasNoAccounts);
        Assert.Empty(context.Sut.SetupPassword);
        Assert.Empty(context.Sut.SetupConfirmation);
    }

    [Fact]
    public async Task OnEnteredBackground_WhenUnlocked_LocksAndClearsAccountProjection()
    {
        var account = new Account(Guid.NewGuid(), "Example", ValidSecret, "user@example.test");
        var context = CreateContext(isConfigured: true, [account]);
        context.Authorization
            .Setup(value => value.TryUnlockWithPasswordAsync("synthetic password"))
            .Callback(context.State.Unlock)
            .ReturnsAsync(AuthorizationResult.Success);
        context.AccountTotp
            .Setup(value => value.GenerateAsync(account.ID))
            .ReturnsAsync(Result.Ok(new TotpGenerationResult("123456", 20, 30)));
        await context.Sut.InitializeAsync();
        context.Sut.UnlockPassword = "synthetic password";
        await context.Sut.UnlockAsync();

        context.Sut.OnEnteredBackground();

        context.Authorization.Verify(value => value.Lock(), Times.Once);
        Assert.True(context.Sut.IsUnlockVisible);
        Assert.Empty(context.Sut.Accounts);
        Assert.Empty(context.Sut.SelectedCode);
        Assert.Null(context.Sut.SelectedAccount);
    }

    [Fact]
    public async Task UnlockAsync_ProjectsNoSecretIntoMobileAccountRows()
    {
        var account = new Account(Guid.NewGuid(), "Example", ValidSecret, "user@example.test");
        var context = CreateContext(isConfigured: true, [account]);
        context.Authorization
            .Setup(value => value.TryUnlockWithPasswordAsync(It.IsAny<string>()))
            .ReturnsAsync(AuthorizationResult.Success);
        context.AccountTotp
            .Setup(value => value.GenerateAsync(account.ID))
            .ReturnsAsync(Result.Ok(new TotpGenerationResult("123456", 20, 30)));
        await context.Sut.InitializeAsync();
        context.Sut.UnlockPassword = "synthetic password";

        await context.Sut.UnlockAsync();

        var projected = Assert.Single(context.Sut.Accounts);
        Assert.Equal(account.ID, projected.Id);
        Assert.DoesNotContain(
            typeof(MobileAccountItem).GetProperties(),
            property => property.Name.Contains("Secret", StringComparison.OrdinalIgnoreCase));
    }

    [Fact]
    public async Task SaveAccountAsync_WithInvalidSecret_DoesNotPersistAccount()
    {
        var context = CreateContext(isConfigured: false);
        context.Authorization
            .Setup(value => value.ConfigurePasswordAsync(It.IsAny<string>(), It.IsAny<string>()))
            .ReturnsAsync(AuthorizationResult.Success);
        await ConfigureAndBeginAddAsync(context);
        context.Sut.EditorIssuer = "Example";
        context.Sut.EditorSecret = "not-valid-*";

        await context.Sut.SaveAccountAsync();

        context.AccountManager.Verify(value => value.AddNewAsync(It.IsAny<Account>()), Times.Never);
        Assert.Equal(
            context.Strings.Get(MobileStringKeys.SecretInvalid),
            context.Sut.NotificationText);
        Assert.Empty(context.Sut.EditorSecret);
    }

    [Fact]
    public async Task SaveAccountAsync_WithValidInput_PersistsOnlyNormalizedAccount()
    {
        var context = CreateContext(isConfigured: false);
        context.Authorization
            .Setup(value => value.ConfigurePasswordAsync(It.IsAny<string>(), It.IsAny<string>()))
            .ReturnsAsync(AuthorizationResult.Success);
        Account? persisted = null;
        context.AccountManager
            .Setup(value => value.AddNewAsync(It.IsAny<Account>()))
            .Callback<Account>(account => persisted = account)
            .ReturnsAsync(Result.Ok());
        await ConfigureAndBeginAddAsync(context);
        context.Sut.EditorIssuer = "  Example  ";
        context.Sut.EditorAccountName = " user@example.test ";
        context.Sut.EditorSecret = "JBSW Y3DP EHPK 3PXP";

        await context.Sut.SaveAccountAsync();

        Assert.NotNull(persisted);
        Assert.Equal("Example", persisted.Issuer);
        Assert.Equal("user@example.test", persisted.AccountName);
        Assert.Equal(ValidSecret, persisted.Secret);
        Assert.Empty(context.Sut.EditorSecret);
        Assert.False(context.Sut.IsEditorVisible);
    }

    private static async Task ConfigureAndBeginAddAsync(TestContext context)
    {
        await context.Sut.InitializeAsync();
        context.Sut.SetupPassword = "synthetic password";
        context.Sut.SetupConfirmation = "synthetic password";
        await context.Sut.ConfigureAsync();
        await context.Sut.BeginAddAsync();
    }

    private static TestContext CreateContext(
        bool isConfigured,
        IReadOnlyList<Account>? accounts = null)
    {
        accounts ??= [];
        var state = new AuthorizationState();
        state.SetConfiguration(isConfigured, TOTP.Core.Enums.PreferredUnlockMethod.Password);
        var authorization = new Mock<IAuthorizationService>();
        authorization.SetupGet(value => value.State).Returns(state);
        authorization.Setup(value => value.InitializeAsync()).Returns(Task.CompletedTask);

        var passwordValidation = new Mock<IPasswordValidationService>();
        passwordValidation.SetupGet(value => value.MinimumLength).Returns(8);

        var accountManager = new Mock<IAccountManager>();
        accountManager.Setup(value => value.GetAllOtpEntriesSortedAsync())
            .ReturnsAsync(() => Result.Ok(accounts));

        var accountTotp = new Mock<IAccountTotpService>();
        var clipboard = new Mock<IAsyncClipboardService>();
        clipboard.SetupGet(value => value.Capabilities)
            .Returns(ClipboardCapabilities.WriteText | ClipboardCapabilities.ConditionalClear);

        var settingsValue = new AppSettings();
        var settings = new Mock<ISettingsService>();
        settings.SetupGet(value => value.Current).Returns(settingsValue);
        settings.Setup(value => value.LoadAsync())
            .ReturnsAsync(Result.Ok<IAppSettings>(settingsValue));

        var paths = new Mock<IPlatformApplicationPaths>();
        paths.SetupGet(value => value.AuthorizationEnvelopeFilePath)
            .Returns(Path.Combine(Path.GetTempPath(), $"missing-{Guid.NewGuid():N}.bin"));

        var strings = new MobileStringCatalog(CultureInfo.GetCultureInfo("en"));
        var sut = new MobileShellViewModel(
            authorization.Object,
            passwordValidation.Object,
            accountManager.Object,
            accountTotp.Object,
            clipboard.Object,
            settings.Object,
            paths.Object,
            strings);
        return new TestContext(
            sut,
            state,
            authorization,
            accountManager,
            accountTotp,
            strings);
    }

    private sealed record TestContext(
        MobileShellViewModel Sut,
        AuthorizationState State,
        Mock<IAuthorizationService> Authorization,
        Mock<IAccountManager> AccountManager,
        Mock<IAccountTotpService> AccountTotp,
        MobileStringCatalog Strings);
}
