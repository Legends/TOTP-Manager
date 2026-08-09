using Avalonia.Platform.Storage;

namespace TOTP.Avalonia.Desktop.Platform;

public sealed class AvaloniaFilePicker(AvaloniaWindowCoordinator windowCoordinator) : IAvaloniaFilePicker
{
    private static readonly FilePickerFileType TotpFiles = new("TOTP files")
    {
        Patterns = ["*.totp", "*.json", "*.txt", "*.csv"]
    };

    public async Task<INativeStorageFile?> PickImportFileAsync(
        CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();
        var provider = windowCoordinator.GetRequiredDialogOwner().StorageProvider;
        if (!provider.CanOpen) return null;

        var files = await provider.OpenFilePickerAsync(new FilePickerOpenOptions
        {
            Title = "Select TOTP import file",
            AllowMultiple = false,
            FileTypeFilter =
            [
                new FilePickerFileType("TOTP files")
                {
                    Patterns = TotpFiles.Patterns
                }
            ]
        });
        cancellationToken.ThrowIfCancellationRequested();

        return files.Count == 1 ? new AvaloniaStorageFile(files[0]) : null;
    }

    public async Task<INativeStorageFile?> PickEncryptedExportFileAsync(
        string suggestedFileName,
        CancellationToken cancellationToken = default)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(suggestedFileName);
        cancellationToken.ThrowIfCancellationRequested();
        var provider = windowCoordinator.GetRequiredDialogOwner().StorageProvider;
        if (!provider.CanSave) return null;

        var file = await provider.SaveFilePickerAsync(new FilePickerSaveOptions
        {
            Title = "Export encrypted TOTP backup",
            SuggestedFileName = Path.GetFileNameWithoutExtension(suggestedFileName),
            DefaultExtension = "totp",
            ShowOverwritePrompt = true,
            FileTypeChoices =
            [
                new FilePickerFileType("Encrypted TOTP backup")
                {
                    Patterns = ["*.totp"]
                }
            ]
        });
        cancellationToken.ThrowIfCancellationRequested();
        return file is null ? null : new AvaloniaStorageFile(file);
    }
}
