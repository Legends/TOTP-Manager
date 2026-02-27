using System.Threading;
using System.Threading.Tasks;
using TOTP.Security.Models;

namespace TOTP.Security.Interfaces;

public interface IHelloGate
{
    Task<bool> IsAvailableAsync(CancellationToken ct = default);
    Task<AuthorizationResult> RequestVerificationAsync(CancellationToken ct = default);

    // New methods for Key Wrapping
    byte[] ProtectKey(byte[] rawDek);
    byte[] UnprotectKey(byte[] wrappedDek);
}