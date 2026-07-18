namespace TOTP.Avalonia.Desktop.Platform;

public interface IAvaloniaFilePicker
{
    Task<string?> PickImportFileNameAsync(CancellationToken cancellationToken = default);
}
