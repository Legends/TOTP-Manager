using FluentResults;
using TOTP.Core.Models;
using TOTP.Core.Services.Interfaces;

namespace TOTP.Infrastructure.Services;

public sealed class AccountTotpService(
    IAccountManager accountManager,
    ITotpGenerator totpGenerator) : IAccountTotpService
{
    public async Task<Result<TotpGenerationResult>> GenerateAsync(Guid accountId)
    {
        if (accountId == Guid.Empty)
            return Result.Fail<TotpGenerationResult>("An account must be selected.");

        var accounts = await accountManager.GetAllOtpEntriesSortedAsync();
        if (accounts.IsFailed)
            return Result.Fail<TotpGenerationResult>(accounts.Errors);

        var account = accounts.Value.FirstOrDefault(candidate => candidate.ID == accountId);
        if (account is null)
            return Result.Fail<TotpGenerationResult>("The selected account no longer exists.");

        try
        {
            return Result.Ok(totpGenerator.Generate(account.Secret));
        }
        catch (FormatException)
        {
            return Result.Fail<TotpGenerationResult>("The selected account seed is invalid.");
        }
    }
}
