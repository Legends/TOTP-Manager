using System.Windows.Media.Imaging;

namespace TOTP.Services.Interfaces;

public interface IQrPreviewService
{
    double PreviewScaleFactor { get; set; }
    void Toggle(BitmapSource? source);
    void Close();
}
