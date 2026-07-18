using TOTP.Core.Platform;

namespace TOTP.Infrastructure.Services;

public sealed class UnavailablePlatformSessionEventSource : IPlatformSessionEventSource
{
    public bool IsSupported => false;
    public event EventHandler<PlatformSessionChangedEventArgs>? SessionChanged
    {
        add { }
        remove { }
    }

    public void Start() { }
    public void Stop() { }
}
