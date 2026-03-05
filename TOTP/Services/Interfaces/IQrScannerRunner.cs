using System;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Media.Imaging;

namespace TOTP.Services.Interfaces;

public interface IQrScannerRunner
{
    Task RunAsync(
        CancellationToken token,
        Action<BitmapSource> onPreview,
        Action onFirstFrame,
        Action<string> onDecoded);
}
