namespace TOTP.Core.Models
{
    public sealed class SecretItem : IEquatable<SecretItem>
    {
        public string Platform { get; }
        public string Secret { get; }
        public string? Account { get; }

        public Guid ID { get; set; }

        public SecretItem(Guid id, string platform, string secret, string? account = null)
        {
            //if (string.IsNullOrWhiteSpace(platform))
            //    throw new ArgumentException(nameof(platform));
            //if (string.IsNullOrWhiteSpace(secret))
            //    throw new ArgumentException(nameof(secret));
            ID = id;
            Platform = platform;
            Secret = secret;
            Account = account;
        }

        public bool Equals(SecretItem? other) => other is not null && ID == other.ID;

        public override bool Equals(object? obj) => Equals(obj as SecretItem);
        public override int GetHashCode() => ID.GetHashCode();
    }
}
