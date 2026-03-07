using Microsoft.Extensions.Logging.Abstractions;
using TOTP.Infrastructure.Security;

namespace TOTP.Tests.Security;

public sealed class MasterPasswordServiceTests
{
    private readonly MasterPasswordService _sut = new(NullLogger<MasterPasswordService>.Instance);

    [Fact]
    public async Task WrapKeyAsync_ThenUnwrapKeyAsync_WithValidParameters_RoundTripsDek()
    {
        var dek = new byte[32];
        Random.Shared.NextBytes(dek);

        var wrapped = await _sut.WrapKeyAsync(dek, "correct-password");
        var unwrapped = await _sut.UnwrapKeyAsync(
            wrapped.WrappedDek,
            "correct-password",
            wrapped.Salt,
            wrapped.Iterations,
            wrapped.MemorySize,
            wrapped.Nonce);

        Assert.NotNull(unwrapped);
        Assert.Equal(dek, unwrapped);
    }

    [Fact]
    public async Task UnwrapKeyAsync_WhenStoredMemorySizeTooHigh_ReturnsNull()
    {
        var dek = new byte[32];
        Random.Shared.NextBytes(dek);

        var wrapped = await _sut.WrapKeyAsync(dek, "correct-password");
        var unwrapped = await _sut.UnwrapKeyAsync(
            wrapped.WrappedDek,
            "correct-password",
            wrapped.Salt,
            wrapped.Iterations,
            300 * 1024,
            wrapped.Nonce);

        Assert.Null(unwrapped);
    }

    [Fact]
    public async Task UnwrapKeyAsync_WhenStoredIterationsTooHigh_ReturnsNull()
    {
        var dek = new byte[32];
        Random.Shared.NextBytes(dek);

        var wrapped = await _sut.WrapKeyAsync(dek, "correct-password");
        var unwrapped = await _sut.UnwrapKeyAsync(
            wrapped.WrappedDek,
            "correct-password",
            wrapped.Salt,
            99,
            wrapped.MemorySize,
            wrapped.Nonce);

        Assert.Null(unwrapped);
    }
}
