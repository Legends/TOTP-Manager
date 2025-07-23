using QRCoder;
using System.Drawing;
using System.IO;
using System.Windows.Media.Imaging;
using TOTP.Interfaces;

namespace TOTP.Services;

public class QrCodeService : IQrCodeService
{
    /// <summary>
    /// string issuer = "platform-name";
    /// string account = "user@example.com or username";
    /// string secret = "JBSWY3DPEHPK3PXP"; // Base32 encoded
    /// string uri = $"otpauth://totp/{issuer}:{account}?secret={secret}&issuer={issuer}&algorithm=SHA1&digits=6&period=30";
    /// </summary>
    /// <param name="uri"></param>
    /// <returns></returns>
    static BitmapImage GenerateQrCodeImage(string uri)
    {
        QRCodeGenerator qrGenerator = new();
        QRCodeData qrCodeData = qrGenerator.CreateQrCode(uri, QRCodeGenerator.ECCLevel.Q);
        QRCode qrCode = new(qrCodeData);
        Bitmap qrBitmap = qrCode.GetGraphic(20);

        using MemoryStream ms = new();
        qrBitmap.Save(ms, System.Drawing.Imaging.ImageFormat.Png);
        ms.Position = 0;

        BitmapImage bitmapImage = new();
        bitmapImage.BeginInit();
        bitmapImage.StreamSource = ms;
        bitmapImage.CacheOption = BitmapCacheOption.OnLoad;
        bitmapImage.EndInit();

        return bitmapImage;
    }

    // otpauth://totp/{issuer}:{account}?secret={secret}&issuer={issuer}
    // otpauth://totp/?secret={secret}&issuer={issuer}

    public BitmapImage GenerateQr(string issuer, string secret, string? account = "")
    {
        string uri = !string.IsNullOrWhiteSpace(account) ?
                        $"otpauth://totp/{issuer}:{account}?secret={secret}&issuer={issuer}&algorithm=SHA1&digits=6&period=30" :
                        $"otpauth://totp/?secret={secret}&issuer={issuer}";
        return GenerateQrCodeImage(uri);
    }

}
