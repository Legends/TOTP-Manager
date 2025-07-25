using System.Windows.Media.Imaging;
using TOTP.Services;

namespace TOTP.Tests.Services;

public class QrCodeServiceTests
{
    [Fact]
    public void GenerateQr_ShouldReturnBitmapImage()
    {
        // Arrange
        var service = new QrCodeService();
        string issuer = "TestPlatform";
        string secret = "JBSWY3DPEHPK3PXP";
        string account = "test@example.com";

        // Act
        BitmapImage image = service.GenerateQr(issuer, secret, account);

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