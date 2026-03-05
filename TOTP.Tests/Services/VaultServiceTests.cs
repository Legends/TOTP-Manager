using Moq;
using System.Linq;
using System.Security.Cryptography;
using TOTP.Core.Models;
using TOTP.Core.Security.Interfaces;
using TOTP.Infrastructure.Security;

namespace TOTP.Tests.Services;

public sealed class VaultServiceTests
{
    [Fact]
    public void EncryptVault_WhenSecurityContextLocked_ThrowsInvalidOperationException()
    {
        var security = new Mock<ISecurityContext>();
        security.SetupGet(s => s.IsUnlocked).Returns(false);
        var sut = new VaultService(security.Object);

        Assert.Throws<InvalidOperationException>(() => { sut.EncryptVault([]); });
    }

    [Fact]
    public void DecryptVault_WhenSecurityContextLocked_ThrowsInvalidOperationException()
    {
        var security = new Mock<ISecurityContext>();
        security.SetupGet(s => s.IsUnlocked).Returns(false);
        var sut = new VaultService(security.Object);

        Assert.Throws<InvalidOperationException>(() => { sut.DecryptVault([1, 2, 3]); });
    }

    [Fact]
    public void EncryptThenDecryptVault_WithSameDek_ReturnsOriginalEntries()
    {
        var dek = RandomNumberGenerator.GetBytes(32);
        var security = new Mock<ISecurityContext>();
        security.SetupGet(s => s.IsUnlocked).Returns(true);
        security.Setup(s => s.GetDek()).Returns(dek);
        var sut = new VaultService(security.Object);
        List<OtpEntry> input =
        [
            new(Guid.NewGuid(), "GitHub", "AAAA", "john"),
            new(Guid.NewGuid(), "Google", "BBBB")
        ];

        var blob = sut.EncryptVault(input);
        var output = sut.DecryptVault(blob);

        Assert.Equal(2, output.Count);
        Assert.Equal(input[0].ID, output[0].ID);
        Assert.Equal(input[0].Issuer, output[0].Issuer);
        Assert.Equal(input[0].Secret, output[0].Secret);
        Assert.Equal(input[0].AccountName, output[0].AccountName);
    }

    [Fact]
    public void DecryptVault_WhenBlobTooSmall_ThrowsCryptographicException()
    {
        var dek = RandomNumberGenerator.GetBytes(32);
        var security = new Mock<ISecurityContext>();
        security.SetupGet(s => s.IsUnlocked).Returns(true);
        security.Setup(s => s.GetDek()).Returns(dek);
        var sut = new VaultService(security.Object);

        Assert.Throws<CryptographicException>(() => { sut.DecryptVault([1, 2, 3]); });
    }

    [Fact]
    public void DecryptVault_WhenHeaderInvalid_ThrowsCryptographicException()
    {
        var dek = RandomNumberGenerator.GetBytes(32);
        var security = new Mock<ISecurityContext>();
        security.SetupGet(s => s.IsUnlocked).Returns(true);
        security.Setup(s => s.GetDek()).Returns(dek);
        var sut = new VaultService(security.Object);
        var wrongHeader = "XXXX"u8.ToArray();
        var nonce = RandomNumberGenerator.GetBytes(12);
        var blob = wrongHeader.Concat(nonce).Concat(new byte[] { 1, 2, 3 }).ToArray();

        Assert.Throws<CryptographicException>(() => { sut.DecryptVault(blob); });
    }

    [Fact]
    public void DecryptVault_WhenCiphertextTampered_ThrowsCryptographicException()
    {
        var dek = RandomNumberGenerator.GetBytes(32);
        var security = new Mock<ISecurityContext>();
        security.SetupGet(s => s.IsUnlocked).Returns(true);
        security.Setup(s => s.GetDek()).Returns(dek);
        var sut = new VaultService(security.Object);
        var blob = sut.EncryptVault([new OtpEntry(Guid.NewGuid(), "GitHub", "SECRET")]);

        blob[^1] ^= 0xFF;

        Assert.Throws<CryptographicException>(() => { sut.DecryptVault(blob); });
    }
}
