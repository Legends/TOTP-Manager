namespace TOTP.Core.Platform;

public sealed class PlatformLifecycleChangedEventArgs(PlatformLifecycleState state) : EventArgs
{
    public PlatformLifecycleState State { get; } = state;
}
