using System;
using System.Collections.Generic;
using System.Linq;
using TOTP.ViewModels;
// If you have OtpNet referenced, uncomment the using and the decode block.
// using OtpNet;

namespace TOTP.Comparer;

/// <summary>
/// Compares SecretItemViewModel by value (Platform, Account, Secret),
/// case-insensitive, ignoring whitespace/padding in Secret.
/// </summary>
public sealed class SecretItemValueComparer : IEqualityComparer<SecretItemViewModel>
{
    public static readonly SecretItemValueComparer Default = new();

    public bool Equals(SecretItemViewModel? x, SecretItemViewModel? y)
    {
        return ReferenceEquals(x, y) || x is not null && y is not null && StringComparer.OrdinalIgnoreCase.Equals(Norm(x.Platform), Norm(y.Platform))
            && StringComparer.OrdinalIgnoreCase.Equals(Norm(x.Account), Norm(y.Account))
            && SecretsEqual(x.Secret, y.Secret);
    }

    public int GetHashCode(SecretItemViewModel obj)
    {
        var hc = new HashCode();

        hc.Add(Norm(obj.Platform), StringComparer.OrdinalIgnoreCase);
        hc.Add(Norm(obj.Account), StringComparer.OrdinalIgnoreCase);
        hc.Add(NormSecret(obj.Secret), StringComparer.OrdinalIgnoreCase);

        return hc.ToHashCode();
    }

    private static string Norm(string? s) => (s ?? string.Empty).Trim();

    // Normalize Base32-like secret: remove spaces/hyphens, trim '=', uppercase.
    private static string NormSecret(string? s) =>
        new string((s ?? "").Where(ch => !char.IsWhiteSpace(ch) && ch != '-').ToArray())
            .TrimEnd('=')
            .ToUpperInvariant();

    private static bool SecretsEqual(string? a, string? b)
    {
        var sa = NormSecret(a);
        var sb = NormSecret(b);

        return StringComparer.Ordinal.Equals(sa, sb);
    }
}
