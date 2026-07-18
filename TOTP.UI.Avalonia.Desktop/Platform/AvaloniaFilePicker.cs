using Avalonia.Platform.Storage;

namespace TOTP.Avalonia.Desktop.Platform;

public sealed class AvaloniaFilePicker(AvaloniaStorageProviderAccessor accessor) : IAvaloniaFilePicker
{
    public async Task<string?> PickImportFileNameAsync(
        CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();
        var provider = accessor.Current;
        if (provider is null || !provider.CanOpen) return null;

        var files = await provider.OpenFilePickerAsync(new FilePickerOpenOptions
        {
            Title = "Select TOTP import file",
            AllowMultiple = false,
            FileTypeFilter =
            [
                new FilePickerFileType("TOTP files")
                {
                    Patterns = ["*.totp", "*.json"]
                }
            ]
        });
        cancellationToken.ThrowIfCancellationRequested();

        return files.Count == 1 ? files[0].Name : null;
    }
}
