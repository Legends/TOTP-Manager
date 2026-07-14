using TOTP.Core.Models;

namespace TOTP.Core.Services.Interfaces;

public interface ITotpGenerator
{
    TotpGenerationResult Generate(string base32Secret);
}
