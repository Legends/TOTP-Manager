namespace TOTP.Core.Security.Models;

public enum VaultKeyVerificationStatus
{
    InvalidCandidateKey = 0,
    Verified,
    InvalidVaultFormat,
    AuthenticationFailed
}
