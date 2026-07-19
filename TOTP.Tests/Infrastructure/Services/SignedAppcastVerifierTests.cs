using System.Text;
using NetSparkleUpdater.Enums;
using NetSparkleUpdater.SignatureVerifiers;
using NSec.Cryptography;
using TOTP.Core.Services.Interfaces;
using TOTP.Infrastructure.Services;

namespace TOTP.Tests.Infrastructure.Services;

public sealed class SignedAppcastVerifierTests
{
    private const string Appcast = "<?xml version=\"1.0\" encoding=\"utf-8\"?><rss version=\"2.0\" xmlns:sparkle=\"http://www.andymatuschak.org/xml-namespaces/sparkle\"><channel><item><title>TOTP Manager M3 Test</title><sparkle:version>99.0.0</sparkle:version><enclosure url=\"https://example.invalid/totp-manager-m3-test.zip\" sparkle:version=\"99.0.0\" /></item></channel></rss>";
    private const string PublicKey = "A6EHv/POEL4dcN0Y50vAmWfk1jCbpQ1fHdyGZBJVMbg=";
    private const string Signature = "sqd9vwOpK+U2OJJMdQQBKN+RQCnUmv6uaYLuLSwiZISFj5ZS0fg/jylTSjL5vWwOYRjtHm4MGJEoQn19JChZCg==";
    private readonly SignedAppcastVerifier _sut = new();

    [Fact]
    public void Verify_WhenTestAppcastSignatureIsValid_SelectsUpdateWithoutDownloading()
    {
        var result = Verify(Encoding.UTF8.GetBytes(Appcast), Signature);

        Assert.Equal(SignedAppcastCheckStatus.UpdateAvailable, result.Status);
        Assert.Equal(new Version(99, 0, 0), result.Version);
        Assert.Equal("https://example.invalid/totp-manager-m3-test.zip", result.ArtifactUri?.AbsoluteUri);
    }

    [Fact]
    public void SignedFixture_IsCompatibleWithNetSparkleStrictEd25519Verification()
    {
        var checker = new Ed25519Checker(SecurityMode.Strict, PublicKey);

        var result = checker.VerifySignature(Signature, Encoding.UTF8.GetBytes(Appcast));

        Assert.Equal(ValidationResult.Valid, result);
    }

    [Fact]
    public void Verify_WhenSignedContentIsChanged_RejectsBeforeParsing()
    {
        var tampered = Encoding.UTF8.GetBytes(Appcast.Replace("99.0.0", "98.0.0", StringComparison.Ordinal));

        var result = Verify(tampered, Signature);

        Assert.Equal(SignedAppcastCheckStatus.InvalidSignature, result.Status);
    }

    [Theory]
    [InlineData("")]
    [InlineData("not-base64")]
    [InlineData("AA==")]
    public void Verify_WhenSignatureIsMalformed_FailsClosed(string signature)
    {
        var result = Verify(Encoding.UTF8.GetBytes(Appcast), signature);

        Assert.Equal(SignedAppcastCheckStatus.InvalidSignature, result.Status);
    }

    [Fact]
    public void Verify_WhenCurrentVersionIsNewer_ReturnsNoApplicableUpdate()
    {
        var bytes = Encoding.UTF8.GetBytes(Appcast);
        var result = _sut.Verify(new SignedAppcastCheckRequest(
            bytes,
            Signature,
            PublicKey,
            new Version(100, 0, 0),
            "windows",
            "x64"));

        Assert.Equal(SignedAppcastCheckStatus.NoApplicableUpdate, result.Status);
    }

    [Fact]
    public void Verify_WhenPortableClientRequiresExplicitTarget_RejectsGenericArtifact()
    {
        var result = _sut.Verify(new SignedAppcastCheckRequest(
            Encoding.UTF8.GetBytes(Appcast),
            Signature,
            PublicKey,
            new Version(2, 0, 0),
            "windows",
            "x64",
            RequireExplicitTarget: true));

        Assert.Equal(SignedAppcastCheckStatus.NoApplicableUpdate, result.Status);
    }

    [Fact]
    public void Verify_WhenSignedArtifactTargetsAnotherOperatingSystem_RejectsIt()
    {
        var result = VerifyDynamic(
            "<sparkle:os>linux</sparkle:os><sparkle:architecture>x64</sparkle:architecture>",
            operatingSystem: "windows");

        Assert.Equal(SignedAppcastCheckStatus.NoApplicableUpdate, result.Status);
    }

    [Fact]
    public void Verify_WhenStableClientReceivesReleaseCandidate_RejectsIt()
    {
        var result = VerifyDynamic(
            "<sparkle:os>windows</sparkle:os><sparkle:architecture>x64</sparkle:architecture><sparkle:channel>rc</sparkle:channel>",
            channel: "stable");

        Assert.Equal(SignedAppcastCheckStatus.NoApplicableUpdate, result.Status);
    }

    [Fact]
    public void Verify_WhenReleaseCandidateClientReceivesMatchingChannel_SelectsIt()
    {
        var result = VerifyDynamic(
            "<sparkle:os>windows</sparkle:os><sparkle:architecture>x64</sparkle:architecture><sparkle:channel>rc</sparkle:channel>",
            channel: "rc");

        Assert.Equal(SignedAppcastCheckStatus.UpdateAvailable, result.Status);
    }

    [Fact]
    public void Verify_WhenAppcastExceedsBound_RejectsFormatBeforeSignatureWork()
    {
        var result = Verify(new byte[256 * 1024 + 1], Signature);

        Assert.Equal(SignedAppcastCheckStatus.InvalidFormat, result.Status);
    }

    private SignedAppcastCheckResult Verify(byte[] appcast, string signature) =>
        _sut.Verify(new SignedAppcastCheckRequest(
            appcast,
            signature,
            PublicKey,
            new Version(2, 0, 0),
            "windows",
            "x64"));

    private SignedAppcastCheckResult VerifyDynamic(
        string targetElements,
        string operatingSystem = "windows",
        string channel = "stable")
    {
        using var key = Key.Create(SignatureAlgorithm.Ed25519, new KeyCreationParameters
        {
            ExportPolicy = KeyExportPolicies.AllowPlaintextExport
        });
        var appcast = $"<?xml version=\"1.0\"?><rss version=\"2.0\" xmlns:sparkle=\"http://www.andymatuschak.org/xml-namespaces/sparkle\"><channel><item><sparkle:version>99.0.0</sparkle:version>{targetElements}<enclosure url=\"https://example.invalid/update.zip\" sparkle:version=\"99.0.0\" /></item></channel></rss>";
        var bytes = Encoding.UTF8.GetBytes(appcast);
        var signature = Convert.ToBase64String(SignatureAlgorithm.Ed25519.Sign(key, bytes));
        var publicKey = Convert.ToBase64String(key.PublicKey.Export(KeyBlobFormat.RawPublicKey));
        return _sut.Verify(new SignedAppcastCheckRequest(
            bytes,
            signature,
            publicKey,
            new Version(2, 0, 0),
            operatingSystem,
            "x64",
            RequireExplicitTarget: true,
            Channel: channel));
    }
}
