namespace TOTP.Core.Services.Interfaces;

public interface IQrCodeService
{
    string BuildOtpAuthUri(string issuer, string secret, string? token = "");
    byte[] GenerateQr(string uri);
    //BitmapImage GenerateQrCodeImage(string uri);
}