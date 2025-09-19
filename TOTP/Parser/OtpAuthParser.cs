using System;
using System.Collections.Generic;
using System.Net;

namespace TOTP.Parser
{
    public static class OtpauthParser
    {
        public sealed class TOTPData
        {
            /// <summary>
            /// aka Account
            /// </summary>
            public string Label { get; init; } = "";
            public string? Issuer { get; init; }
            public string SecretBase32 { get; init; } = "";
            public string Algorithm { get; init; } = "SHA1";
            public int Digits { get; init; } = 6;
            public int Period { get; init; } = 30;
        }

        public static TOTPData Parse(string otpauthUri)
        {
            // Basic checks
            if (string.IsNullOrWhiteSpace(otpauthUri) || !otpauthUri.StartsWith("otpauth://", StringComparison.OrdinalIgnoreCase))
                throw new ArgumentException("Not an otpauth URI.");

            var uri = new Uri(otpauthUri);
            if (!string.Equals(uri.Host, "totp", StringComparison.OrdinalIgnoreCase))
                throw new ArgumentException("Only TOTP is supported in this parser.");

            // The 'path' is like "/Issuer:Label" or "/Label"
            var path = Uri.UnescapeDataString(uri.AbsolutePath.TrimStart('/'));
            string label = path;
            string? issuerFromPath = null;
            var colonIdx = path.IndexOf(':');
            if (colonIdx >= 0)
            {
                issuerFromPath = path.Substring(0, colonIdx);
                label = path.Substring(colonIdx + 1);
            }

            var query = ParseQuery(uri.Query);
            if (!query.TryGetValue("secret", out var secret) || string.IsNullOrWhiteSpace(secret))
                throw new ArgumentException("Missing 'secret' parameter.");

            query.TryGetValue("issuer", out var issuerParam);
            query.TryGetValue("algorithm", out var alg);
            query.TryGetValue("digits", out var digitsStr);
            query.TryGetValue("period", out var periodStr);

            var issuer = issuerParam ?? issuerFromPath;
            var digits = int.TryParse(digitsStr, out var d) ? d : 6;
            var period = int.TryParse(periodStr, out var p) ? p : 30;

            return new TOTPData
            {
                Label = label,
                Issuer = issuer,
                SecretBase32 = secret,
                Algorithm = string.IsNullOrWhiteSpace(alg) ? "SHA1" : alg!,
                Digits = digits,
                Period = period
            };
        }

        private static Dictionary<string, string> ParseQuery(string query)
        {
            var dict = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
            if (string.IsNullOrEmpty(query)) return dict;

            foreach (var part in query.TrimStart('?').Split('&', StringSplitOptions.RemoveEmptyEntries))
            {
                var kv = part.Split('=', 2);
                var key = WebUtility.UrlDecode(kv[0]);
                var val = kv.Length > 1 ? WebUtility.UrlDecode(kv[1]) : "";
                dict[key] = val;
            }
            return dict;
        }
    }
}
