namespace TOTP.Core.Security.Models;

public enum AuthorizationEnvelopeSessionErrorCode
{
    Unknown = 0,
    LoadFailed,
    NotInitialized,
    VaultVerificationFailed,
    UnexpectedFailure
}
