using QRCoder;
using TOTP.Interfaces;

namespace TOTP.Services;

public class QrCodeService : IQrCodeService
{
    // otpauth://totp/{issuer}:{account}?secret={secret}&issuer={issuer}
    // otpauth://totp/?secret={secret}&issuer={issuer}

    //public BitmapImage GenerateQr(string issuer, string secret, string? account = "")
    //{
    //    var uri = !string.IsNullOrWhiteSpace(account)
    //        ? $"otpauth://totp/{issuer}:{account}?secret={secret}&issuer={issuer}&algorithm=SHA1&digits=6&period=30"
    //        : $"otpauth://totp/?secret={secret}&issuer={issuer}";
    //    return GenerateQrCodeImage(uri);
    //}

    public string BuildOtpAuthUri(string issuer, string secret, string? account = "")
    {
        return !string.IsNullOrWhiteSpace(account)
            ? $"otpauth://totp/{issuer}:{account}?secret={secret}&issuer={issuer}&algorithm=SHA1&digits=6&period=30"
            : $"otpauth://totp/?secret={secret}&issuer={issuer}";
    }

    /// <summary>
    ///     string issuer = "platform-name";
    ///     string account = "user@example.com or username";
    ///     string secret = "JBSWY3DPEHPK3PXP"; // Base32 encoded
    ///     string uri = $"otpauth://totp/{issuer}:{account}?secret={secret}&issuer={issuer}&algorithm=SHA1&digits=6&period
    ///     =30";
    /// </summary>
    /// <param name="uri"></param>
    /// <returns></returns>     
    public byte[] GenerateQr(string uri)
    {
        using QRCodeGenerator qrGenerator = new();
        var qrCodeData = qrGenerator.CreateQrCode(uri, QRCodeGenerator.ECCLevel.Q);

        using PngByteQRCode qrCode = new(qrCodeData);
        byte[] qrCodeImage = qrCode.GetGraphic(20);
        return qrCodeImage;
    }

}