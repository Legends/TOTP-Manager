using System.Windows.Media.Imaging;

namespace Github2FA.Interfaces;

 public interface IQrCodeService
{
    BitmapImage GenerateQr(string issuer, string secret, string account = "");
    //BitmapImage GenerateQrCodeImage(string uri);
}