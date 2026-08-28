using TOTP.Infrastructure.Services;

namespace TOTP.Tests.Services;

public class QrCodeServiceTests
{
    [Fact]
    public void GenerateQr_ShouldReturnPngBytes()
    {
        // Arrange
        var qrCodeService = new QrCodeService();
        string issuer = "TestPlatform";
        string secret = "JBSWY3DPEHPK3PXP";
        string account = "test@example.com";

        // Act
        var uri = qrCodeService.BuildOtpAuthUri(issuer, secret, account);
        byte[] pngBytes = qrCodeService.GenerateQr(uri);

        // Assert
        Assert.True(pngBytes.Length > 8);
        Assert.Equal(new byte[] { 137, 80, 78, 71, 13, 10, 26, 10 }, pngBytes[..8]);
    }

}
