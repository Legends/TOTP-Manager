namespace TOTP.Core.Platform;

public interface IPlatformLifecycleEventSource
{
    bool IsSupported { get; }
    event EventHandler<PlatformLifecycleChangedEventArgs>? LifecycleChanged;
    void Start();
    void Stop();
}
