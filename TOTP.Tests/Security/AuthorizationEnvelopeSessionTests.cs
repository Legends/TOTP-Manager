using FluentResults;
using Microsoft.Extensions.Logging.Abstractions;
using Moq;
using TOTP.Core.Security;
using TOTP.Core.Security.Interfaces;
using TOTP.Core.Security.Models;
using TOTP.Infrastructure.Security;

namespace TOTP.Tests.Security;

public sealed class AuthorizationEnvelopeSessionTests
{
    [Fact]
    public async Task InitializeAsync_WhenEnvelopeIsMissing_ReportsUnconfiguredFirstRun()
    {
        var cancellationToken = TestContext.Current.CancellationToken;
        var store = StoreReturning(null, cancellationToken);
        using var sut = CreateSession(store);

        var result = await sut.InitializeAsync(cancellationToken);

        Assert.True(result.IsSuccess);
        Assert.Equal(new AuthorizationEnvelopeSessionState(true, false, false), result.Value);
        Assert.Equal(result.Value, sut.State);
    }

    [Fact]
    public async Task InitializeAsync_WithSupportedQuickUnlock_ReportsCapability()
    {
        var cancellationToken = TestContext.Current.CancellationToken;
        var envelope = AuthorizationEnvelopeV2CodecTests.CreateEnvelope() with
        {
            QuickUnlockWrapper = CreateSupportedQuickUnlockWrapper()
        };
        var store = StoreReturning(envelope, cancellationToken);
        using var sut = CreateSession(store);

        var result = await sut.InitializeAsync(cancellationToken);

        Assert.True(result.IsSuccess);
        Assert.Equal(new AuthorizationEnvelopeSessionState(true, true, true), result.Value);
    }

    [Fact]
    public async Task InitializeAsync_WithUnsupportedQuickUnlock_PreservesPasswordRecoveryOnly()
    {
        var cancellationToken = TestContext.Current.CancellationToken;
        var unsupported = CreateSupportedQuickUnlockWrapper() with { Provider = "future-provider" };
        var envelope = AuthorizationEnvelopeV2CodecTests.CreateEnvelope() with
        {
            QuickUnlockWrapper = unsupported
        };
        var store = StoreReturning(envelope, cancellationToken);
        using var sut = CreateSession(store);

        var result = await sut.InitializeAsync(cancellationToken);

        Assert.True(result.IsSuccess);
        Assert.Equal(new AuthorizationEnvelopeSessionState(true, true, false), result.Value);
    }

    [Fact]
    public async Task InitializeAsync_WhenStoreFails_RemainsUninitializedAndPreservesTypedFailure()
    {
        var cancellationToken = TestContext.Current.CancellationToken;
        var store = new Mock<IAuthorizationEnvelopeStore>();
        store.Setup(value => value.LoadAsync(cancellationToken))
            .ReturnsAsync(Result.Fail<AuthorizationEnvelopeV2?>(new AuthorizationEnvelopeError(
                AuthorizationEnvelopeErrorCode.ReadAccessDenied,
                "synthetic failure")));
        using var sut = CreateSession(store);

        var result = await sut.InitializeAsync(cancellationToken);

        AssertSessionError(result, AuthorizationEnvelopeSessionErrorCode.LoadFailed);
        Assert.Equal(
            AuthorizationEnvelopeErrorCode.ReadAccessDenied,
            Assert.Single(result.Errors.OfType<AuthorizationEnvelopeError>()).Code);
        Assert.Equal(AuthorizationEnvelopeSessionState.NotInitialized, sut.State);
    }

    [Fact]
    public async Task TryUnlockWithPasswordAsync_BeforeInitialization_ReturnsTypedFailure()
    {
        using var sut = CreateSession(new Mock<IAuthorizationEnvelopeStore>());

        var result = await sut.TryUnlockWithPasswordAsync(
            "password",
            TestContext.Current.CancellationToken);

        AssertSessionError(result, AuthorizationEnvelopeSessionErrorCode.NotInitialized);
    }

    [Fact]
    public async Task TryUnlockWithPasswordAsync_WhenUnconfigured_ReturnsNotConfigured()
    {
        var cancellationToken = TestContext.Current.CancellationToken;
        var store = StoreReturning(null, cancellationToken);
        using var sut = CreateSession(store);
        Assert.True((await sut.InitializeAsync(cancellationToken)).IsSuccess);

        var result = await sut.TryUnlockWithPasswordAsync("password", cancellationToken);

        Assert.True(result.IsSuccess);
        Assert.Equal(AuthorizationResult.NotConfigured, result.Value);
    }

    [Fact]
    public async Task TryUnlockWithPasswordAsync_WhenPasswordIsWrong_ReturnsInvalidCredentials()
    {
        var cancellationToken = TestContext.Current.CancellationToken;
        var store = StoreReturning(AuthorizationEnvelopeV2CodecTests.CreateEnvelope(), cancellationToken);
        var password = new Mock<IMasterPasswordService>();
        password.Setup(value => value.UnwrapKeyV2Async(
                It.IsAny<PasswordKeyWrapperV2>(),
                "wrong-password",
                cancellationToken))
            .ReturnsAsync((byte[]?)null);
        var vault = new Mock<IStoredVaultKeyVerifier>();
        var security = new Mock<ISecurityContext>();
        using var sut = CreateSession(store, password, vault, security);
        Assert.True((await sut.InitializeAsync(cancellationToken)).IsSuccess);

        var result = await sut.TryUnlockWithPasswordAsync("wrong-password", cancellationToken);

        Assert.True(result.IsSuccess);
        Assert.Equal(AuthorizationResult.InvalidCredentials, result.Value);
        vault.VerifyNoOtherCalls();
        security.VerifyNoOtherCalls();
    }

    [Theory]
    [InlineData(VaultKeyVerificationStatus.Verified)]
    [InlineData(VaultKeyVerificationStatus.VaultNotFound)]
    public async Task TryUnlockWithPasswordAsync_AfterVaultVerification_UnlocksContextAndClearsKey(
        VaultKeyVerificationStatus vaultStatus)
    {
        var cancellationToken = TestContext.Current.CancellationToken;
        var store = StoreReturning(AuthorizationEnvelopeV2CodecTests.CreateEnvelope(), cancellationToken);
        var recoveredKey = Enumerable.Range(1, 32).Select(value => (byte)value).ToArray();
        var expectedKey = (byte[])recoveredKey.Clone();
        var password = PasswordReturning(recoveredKey, cancellationToken);
        var vault = new Mock<IStoredVaultKeyVerifier>();
        vault.Setup(value => value.VerifyAsync(It.IsAny<ReadOnlyMemory<byte>>(), cancellationToken))
            .ReturnsAsync(Result.Ok(vaultStatus));
        byte[]? contextKey = null;
        var security = new Mock<ISecurityContext>();
        security.Setup(value => value.SetDek(It.IsAny<byte[]>()))
            .Callback<byte[]>(value => contextKey = (byte[])value.Clone());
        using var sut = CreateSession(store, password, vault, security);
        Assert.True((await sut.InitializeAsync(cancellationToken)).IsSuccess);

        var result = await sut.TryUnlockWithPasswordAsync("password", cancellationToken);

        Assert.True(result.IsSuccess);
        Assert.Equal(AuthorizationResult.Success, result.Value);
        Assert.Equal(expectedKey, contextKey);
        Assert.All(recoveredKey, value => Assert.Equal(0, value));
        security.Verify(value => value.SetDek(It.IsAny<byte[]>()), Times.Once);
    }

    [Fact]
    public async Task TryUnlockWithPasswordAsync_WhenRecoveredKeyLengthIsInvalid_FailsClosedAndClearsKey()
    {
        var cancellationToken = TestContext.Current.CancellationToken;
        var store = StoreReturning(AuthorizationEnvelopeV2CodecTests.CreateEnvelope(), cancellationToken);
        var recoveredKey = new byte[31];
        Array.Fill(recoveredKey, (byte)7);
        var password = PasswordReturning(recoveredKey, cancellationToken);
        var vault = new Mock<IStoredVaultKeyVerifier>();
        var security = new Mock<ISecurityContext>();
        using var sut = CreateSession(store, password, vault, security);
        Assert.True((await sut.InitializeAsync(cancellationToken)).IsSuccess);

        var result = await sut.TryUnlockWithPasswordAsync("password", cancellationToken);

        AssertSessionError(result, AuthorizationEnvelopeSessionErrorCode.VaultVerificationFailed);
        Assert.All(recoveredKey, value => Assert.Equal(0, value));
        vault.VerifyNoOtherCalls();
        security.VerifyNoOtherCalls();
    }

    [Theory]
    [InlineData(VaultKeyVerificationStatus.AuthenticationFailed)]
    [InlineData(VaultKeyVerificationStatus.InvalidVaultFormat)]
    [InlineData(VaultKeyVerificationStatus.InvalidCandidateKey)]
    public async Task TryUnlockWithPasswordAsync_WhenVaultDoesNotVerify_DoesNotUnlock(
        VaultKeyVerificationStatus vaultStatus)
    {
        var cancellationToken = TestContext.Current.CancellationToken;
        var store = StoreReturning(AuthorizationEnvelopeV2CodecTests.CreateEnvelope(), cancellationToken);
        var recoveredKey = new byte[32];
        var password = PasswordReturning(recoveredKey, cancellationToken);
        var vault = new Mock<IStoredVaultKeyVerifier>();
        vault.Setup(value => value.VerifyAsync(It.IsAny<ReadOnlyMemory<byte>>(), cancellationToken))
            .ReturnsAsync(Result.Ok(vaultStatus));
        var security = new Mock<ISecurityContext>();
        using var sut = CreateSession(store, password, vault, security);
        Assert.True((await sut.InitializeAsync(cancellationToken)).IsSuccess);

        var result = await sut.TryUnlockWithPasswordAsync("password", cancellationToken);

        AssertSessionError(result, AuthorizationEnvelopeSessionErrorCode.VaultVerificationFailed);
        security.VerifyNoOtherCalls();
    }

    [Fact]
    public async Task TryUnlockWithPasswordAsync_WhenVaultReadFails_PreservesTypedFailure()
    {
        var cancellationToken = TestContext.Current.CancellationToken;
        var store = StoreReturning(AuthorizationEnvelopeV2CodecTests.CreateEnvelope(), cancellationToken);
        var password = PasswordReturning(new byte[32], cancellationToken);
        var vault = new Mock<IStoredVaultKeyVerifier>();
        vault.Setup(value => value.VerifyAsync(It.IsAny<ReadOnlyMemory<byte>>(), cancellationToken))
            .ReturnsAsync(Result.Fail<VaultKeyVerificationStatus>(new StoredVaultVerificationError(
                StoredVaultVerificationErrorCode.ReadFailed,
                "synthetic failure")));
        var security = new Mock<ISecurityContext>();
        using var sut = CreateSession(store, password, vault, security);
        Assert.True((await sut.InitializeAsync(cancellationToken)).IsSuccess);

        var result = await sut.TryUnlockWithPasswordAsync("password", cancellationToken);

        AssertSessionError(result, AuthorizationEnvelopeSessionErrorCode.VaultVerificationFailed);
        Assert.Equal(
            StoredVaultVerificationErrorCode.ReadFailed,
            Assert.Single(result.Errors.OfType<StoredVaultVerificationError>()).Code);
        security.VerifyNoOtherCalls();
    }

    [Fact]
    public async Task InitializeAsync_WhenReloaded_ClearsPreviouslyCachedEnvelopeBuffers()
    {
        var cancellationToken = TestContext.Current.CancellationToken;
        var envelope = AuthorizationEnvelopeV2CodecTests.CreateEnvelope();
        var store = new Mock<IAuthorizationEnvelopeStore>();
        store.SetupSequence(value => value.LoadAsync(cancellationToken))
            .ReturnsAsync(Result.Ok<AuthorizationEnvelopeV2?>(envelope))
            .ReturnsAsync(Result.Ok<AuthorizationEnvelopeV2?>(null));
        using var sut = CreateSession(store);
        Assert.True((await sut.InitializeAsync(cancellationToken)).IsSuccess);

        var result = await sut.InitializeAsync(cancellationToken);

        Assert.True(result.IsSuccess);
        Assert.False(result.Value.IsConfigured);
        Assert.All(envelope.PasswordWrapper.Kdf.Salt, value => Assert.Equal(0, value));
        Assert.All(envelope.PasswordWrapper.WrappedKey.Nonce, value => Assert.Equal(0, value));
        Assert.All(envelope.PasswordWrapper.WrappedKey.Ciphertext, value => Assert.Equal(0, value));
    }

    [Fact]
    public async Task Dispose_AfterInitialization_ClearsCachedEnvelopeBuffers()
    {
        var cancellationToken = TestContext.Current.CancellationToken;
        var envelope = AuthorizationEnvelopeV2CodecTests.CreateEnvelope();
        var store = StoreReturning(envelope, cancellationToken);
        var sut = CreateSession(store);
        Assert.True((await sut.InitializeAsync(cancellationToken)).IsSuccess);

        sut.Dispose();

        Assert.All(envelope.PasswordWrapper.Kdf.Salt, value => Assert.Equal(0, value));
        Assert.All(envelope.PasswordWrapper.WrappedKey.Nonce, value => Assert.Equal(0, value));
        Assert.All(envelope.PasswordWrapper.WrappedKey.Ciphertext, value => Assert.Equal(0, value));
    }

    [Fact]
    public async Task InitializeAsync_WhenCancelled_PropagatesWithoutLoading()
    {
        var store = new Mock<IAuthorizationEnvelopeStore>();
        using var sut = CreateSession(store);
        using var cancellation = new CancellationTokenSource();
        await cancellation.CancelAsync();

        await Assert.ThrowsAnyAsync<OperationCanceledException>(
            () => sut.InitializeAsync(cancellation.Token));

        store.VerifyNoOtherCalls();
        Assert.Equal(AuthorizationEnvelopeSessionState.NotInitialized, sut.State);
    }

    private static Mock<IAuthorizationEnvelopeStore> StoreReturning(
        AuthorizationEnvelopeV2? envelope,
        CancellationToken cancellationToken)
    {
        var store = new Mock<IAuthorizationEnvelopeStore>();
        store.Setup(value => value.LoadAsync(cancellationToken))
            .ReturnsAsync(Result.Ok<AuthorizationEnvelopeV2?>(envelope));
        return store;
    }

    private static Mock<IMasterPasswordService> PasswordReturning(
        byte[] recoveredKey,
        CancellationToken cancellationToken)
    {
        var password = new Mock<IMasterPasswordService>();
        password.Setup(value => value.UnwrapKeyV2Async(
                It.IsAny<PasswordKeyWrapperV2>(),
                "password",
                cancellationToken))
            .ReturnsAsync(recoveredKey);
        return password;
    }

    private static AuthorizationEnvelopeSession CreateSession(
        Mock<IAuthorizationEnvelopeStore> store,
        Mock<IMasterPasswordService>? password = null,
        Mock<IStoredVaultKeyVerifier>? vault = null,
        Mock<ISecurityContext>? security = null) =>
        new(
            store.Object,
            (password ?? new Mock<IMasterPasswordService>()).Object,
            (vault ?? new Mock<IStoredVaultKeyVerifier>()).Object,
            (security ?? new Mock<ISecurityContext>()).Object,
            NullLogger<AuthorizationEnvelopeSession>.Instance);

    private static PlatformQuickUnlockWrapperV2 CreateSupportedQuickUnlockWrapper() => new()
    {
        Provider = PlatformQuickUnlockContract.WindowsHelloTpmProvider,
        ProviderVersion = PlatformQuickUnlockContract.WindowsHelloTpmProviderVersion,
        AuthenticationPolicy = PlatformQuickUnlockContract.UserVerificationRequired,
        KeyReference = "TOTP_TPM_SYNTHETIC_SESSION",
        WrappedKey = new PlatformWrappedKeyV2
        {
            Algorithm = PlatformQuickUnlockContract.RsaOaepSha256Algorithm,
            Ciphertext = new byte[256]
        }
    };

    private static void AssertSessionError<T>(
        Result<T> result,
        AuthorizationEnvelopeSessionErrorCode expectedCode)
    {
        Assert.False(result.IsSuccess);
        Assert.Equal(
            expectedCode,
            Assert.Single(result.Errors.OfType<AuthorizationEnvelopeSessionError>()).Code);
    }
}
