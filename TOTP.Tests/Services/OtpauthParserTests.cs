using TOTP.Infrastructure.Parser;

namespace TOTP.Tests.Services;

public sealed class OtpauthParserTests
{
    [Fact]
    public void NormalizeBase32SecretForUri_RemovesSeparatorsAndPadding()
    {
        var normalized = OtpauthParser.NormalizeBase32SecretForUri(" abcd-ef gh== ");

        Assert.Equal("ABCDEFGH", normalized);
    }

    [Fact]
    public void NormalizeBase32SecretForUri_WhenNull_ThrowsArgumentNullException()
    {
        Assert.Throws<ArgumentNullException>(() => OtpauthParser.NormalizeBase32SecretForUri(null!));
    }

    [Fact]
    public void Parse_WhenValidUriWithPathIssuer_ReturnsExpectedData()
    {
        var uri = "otpauth://totp/Acme:John%20Doe?secret=ABC123&algorithm=SHA256&digits=8&period=60";

        var parsed = OtpauthParser.Parse(uri);

        Assert.Equal("John Doe", parsed.Label);
        Assert.Equal("Acme", parsed.Issuer);
        Assert.Equal("ABC123", parsed.SecretBase32);
        Assert.Equal("SHA256", parsed.Algorithm);
        Assert.Equal(8, parsed.Digits);
        Assert.Equal(60, parsed.Period);
    }

    [Fact]
    public void Parse_WhenIssuerInQuery_OverridesPathIssuer()
    {
        var uri = "otpauth://totp/Acme:John?secret=ABC123&issuer=GitHub";

        var parsed = OtpauthParser.Parse(uri);

        Assert.Equal("John", parsed.Label);
        Assert.Equal("GitHub", parsed.Issuer);
    }

    [Fact]
    public void Parse_WhenDigitsAndPeriodInvalid_UsesDefaults()
    {
        var uri = "otpauth://totp/John?secret=ABC123&digits=abc&period=xyz";

        var parsed = OtpauthParser.Parse(uri);

        Assert.Equal(6, parsed.Digits);
        Assert.Equal(30, parsed.Period);
        Assert.Equal("SHA1", parsed.Algorithm);
    }

    [Theory]
    [InlineData("")]
    [InlineData("https://example.com")]
    public void Parse_WhenUriInvalid_ThrowsArgumentException(string uri)
    {
        Assert.Throws<ArgumentException>(() => OtpauthParser.Parse(uri));
    }

    [Fact]
    public void Parse_WhenTypeIsNotTotp_ThrowsArgumentException()
    {
        var uri = "otpauth://hotp/Acme:John?secret=ABC123";

        Assert.Throws<ArgumentException>(() => OtpauthParser.Parse(uri));
    }

    [Fact]
    public void Parse_WhenSecretMissing_ThrowsArgumentException()
    {
        var uri = "otpauth://totp/Acme:John?issuer=Acme";

        Assert.Throws<ArgumentException>(() => OtpauthParser.Parse(uri));
    }
}
