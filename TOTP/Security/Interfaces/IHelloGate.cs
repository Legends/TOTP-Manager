using System.Threading;
using System.Threading.Tasks;
using TOTP.Security.Models;

namespace TOTP.Security.Interfaces;

public interface IHelloGate
{
    Task<bool> IsAvailableAsync(CancellationToken ct = default);

    /// <summary>
    /// Prompts Windows Hello verification. Returns Success/Failed/Cancelled/NotAvailable etc.
    /// </summary>
    Task<AuthorizationResult> RequestVerificationAsync(CancellationToken ct = default);
}