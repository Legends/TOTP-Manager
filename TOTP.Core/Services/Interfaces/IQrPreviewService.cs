namespace TOTP.Core.Services.Interfaces;

public interface IQrPreviewService
{
    double PreviewScaleFactor { get; set; }
    void Toggle(ReadOnlyMemory<byte> pngImage);
    void Close();
}
