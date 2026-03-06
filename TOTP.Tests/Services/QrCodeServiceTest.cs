using System.Windows.Media.Imaging;
using TOTP.Infrastructure.Services;

namespace TOTP.Tests.Services;

public class QrCodeServiceTests
{
    [Fact]
    public void GenerateQr_ShouldReturnBitmapImage()
    {
        // Arrange
        var qrCodeService = new QrCodeService();
        string issuer = "TestPlatform";
        string secret = "JBSWY3DPEHPK3PXP";
        string token = "test@example.com";

        // Act
        var uri = qrCodeService.BuildOtpAuthUri(issuer, secret, token);
        byte[] pngBytes = qrCodeService.GenerateQr(uri);

        using MemoryStream ms = new(pngBytes);
        BitmapImage image = new();
        image.BeginInit();
        image.StreamSource = ms;
        image.CacheOption = BitmapCacheOption.OnLoad;
        image.EndInit();

        // Force the image to load into memory
        image.Freeze(); // optional: to make it thread-safe

        // Assert
        Assert.NotNull(image);
        Assert.True(image.PixelHeight > 0, "Image height is 0");
        Assert.True(image.PixelWidth > 0, "Image width is 0");
        Assert.True(image.CanFreeze); // Optional WPF check
    }

    //If you want to mock it in other ViewModel tests:
    // var mock = new Mock<IQrCodeService>();
    // mock.Setup(x => x.GenerateQr(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>()))
    // .Returns(new BitmapImage()); // or load from embedded resource


}
