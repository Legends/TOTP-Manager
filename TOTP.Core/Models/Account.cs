using System;
using System.Text.Json.Serialization;

namespace TOTP.Core.Models
{
    public sealed class OtpEntry : IEquatable<OtpEntry>
    {
        [JsonPropertyName("id")]
        public Guid ID { get; set; }

        [JsonPropertyName("issuer")]
        public string Issuer { get; }

        [JsonPropertyName("secret")]
        public string Secret { get; }

        [JsonPropertyName("account_name")]
        public string? TokenName { get; }

        // JsonConstructor wird benötigt, da die Properties nur 'get' haben
        [JsonConstructor]
        public OtpEntry(Guid id, string issuer, string secret, string? tokenName = null)
        {
            ID = id;
            Issuer = issuer;
            Secret = secret;
            TokenName = tokenName;
        }

        public bool Equals(OtpEntry? other) => other is not null && ID == other.ID;
        public override bool Equals(object? obj) => Equals(obj as OtpEntry);
        public override int GetHashCode() => ID.GetHashCode();
    }
}