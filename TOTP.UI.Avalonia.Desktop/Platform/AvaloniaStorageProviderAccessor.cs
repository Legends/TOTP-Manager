using Avalonia.Platform.Storage;

namespace TOTP.Avalonia.Desktop.Platform;

public sealed class AvaloniaStorageProviderAccessor
{
    public IStorageProvider? Current { get; private set; }

    public void Set(IStorageProvider storageProvider) =>
        Current = storageProvider ?? throw new ArgumentNullException(nameof(storageProvider));
}
