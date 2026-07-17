namespace TOTP.Core.Services.Interfaces;

public interface IQrScannerRunner
{
    Task RunAsync(
        CancellationToken token,
        Action<byte[]> onPreview,
        Action onFirstFrame,
        Action<string> onDecoded);
}
