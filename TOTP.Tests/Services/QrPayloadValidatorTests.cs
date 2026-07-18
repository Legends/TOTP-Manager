using TOTP.Infrastructure.Services;

namespace TOTP.Tests.Services;

public sealed class QrPayloadValidatorTests
{
    private readonly QrPayloadValidator _sut = new();

    [Fact]
    public void Validate_WhenTotpPayloadIsValid_ReturnsOnlySafeDescriptor()
    {
        var result = _sut.Validate(
            "otpauth://totp/Example:alice?secret=JBSWY3DPEHPK3PXP&issuer=Example");

        Assert.True(result.IsValid);
        Assert.Equal("Example", result.Issuer);
        Assert.Equal("alice", result.AccountName);
        Assert.DoesNotContain("JBSWY", result.ToString(), StringComparison.Ordinal);
    }

    [Theory]
    [InlineData("")]
    [InlineData("https://example.invalid/not-otp")]
    [InlineData("otpauth://totp/demo?secret=not-base32")]
    public void Validate_WhenPayloadIsUntrusted_ReturnsInvalidWithoutThrowing(string payload)
    {
        var result = _sut.Validate(payload);

        Assert.False(result.IsValid);
        Assert.Empty(result.Issuer);
        Assert.Empty(result.AccountName);
    }

    [Fact]
    public void Validate_WhenPayloadExceedsLimit_FailsClosed()
    {
        var result = _sut.Validate("otpauth://totp/" + new string('a', 4096));

        Assert.False(result.IsValid);
    }
}
