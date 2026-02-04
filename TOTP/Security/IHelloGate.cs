using System.Threading;
using System.Threading.Tasks;

namespace TOTP.Security;

public interface IHelloGate
{
    Task<bool> IsAvailableAsync(CancellationToken ct = default);

    /// <summary>
    /// Prompts Windows Hello verification. Returns Success/Failed/Cancelled/NotAvailable etc.
    /// </summary>
    Task<AuthorizationResult> RequestVerificationAsync(CancellationToken ct = default);
}