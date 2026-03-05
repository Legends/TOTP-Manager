using System.Threading;
using System.Threading.Tasks;
using TOTP.Core.Enums;
using TOTP.Core.Security.Models;

namespace TOTP.Core.Security.Interfaces;

public interface IHelloGate
{
    Task<bool> IsAvailableAsync(CancellationToken ct = default);
    Task<AuthorizationResult> RequestVerificationAsync(CancellationToken ct = default);

    // Hardware-bound persistence methods
    Task<byte[]> ProtectKeyAsync(byte[] rawDek, string keyId);
    Task<byte[]?> UnprotectKeyAsync(byte[] wrappedDek, string keyId);
}
