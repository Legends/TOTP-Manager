namespace TOTP.Core.Platform;

public sealed record ApplicationActivationRequest(int Version, ApplicationActivationKind Kind)
{
    public const int CurrentVersion = 1;

    public static ApplicationActivationRequest ActivateMainWindow() =>
        new(CurrentVersion, ApplicationActivationKind.ActivateMainWindow);

    public bool IsSupported => Version == CurrentVersion && Enum.IsDefined(Kind);
}
