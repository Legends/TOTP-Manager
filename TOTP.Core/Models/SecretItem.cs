namespace TOTP.Core.Models
{
    public sealed class SecretItem : IEquatable<SecretItem>
    {
        public string Platform { get; }
        public string Secret { get; }
        public string? Account { get; }

        public SecretItem(string platform, string secret, string? account = null)
        {
            //if (string.IsNullOrWhiteSpace(platform))
            //    throw new ArgumentException(nameof(platform));
            //if (string.IsNullOrWhiteSpace(secret))
            //    throw new ArgumentException(nameof(secret));

            Platform = platform;
            Secret = secret;
            Account = account;
        }

        public bool Equals(SecretItem? other) =>
            other is not null &&
            Platform == other.Platform &&
            Secret == other.Secret &&
            Account == other.Account;

        public override bool Equals(object? obj) => Equals(obj as SecretItem);
        public override int GetHashCode() => HashCode.Combine(Platform, Secret, Account);
    }
}
