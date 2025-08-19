namespace TOTP.Core.Enums
{
    public enum ValidationError
    {
        None,
        PlatformRequired,
        SecretRequired,
        SecretInvalidFormat
    }
}
