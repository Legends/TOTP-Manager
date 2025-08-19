namespace TOTP.Core.Models
{
    public sealed class Secret : IEquatable<Secret>
    {
        public string Platform { get; }
        public string SecretBase32 { get; }
        public string? Account { get; }

        public Secret(string platform, string secretBase32, string? account = null)
        {
            if (string.IsNullOrWhiteSpace(platform))
                throw new ArgumentException(nameof(platform));
            if (string.IsNullOrWhiteSpace(secretBase32))
                throw new ArgumentException(nameof(secretBase32));

            Platform = platform;
            SecretBase32 = secretBase32;
            Account = account;
        }

        public bool Equals(Secret? other) =>
            other is not null &&
            Platform == other.Platform &&
            SecretBase32 == other.SecretBase32 &&
            Account == other.Account;

        public override bool Equals(object? obj) => Equals(obj as Secret);
        public override int GetHashCode() => HashCode.Combine(Platform, SecretBase32, Account);
    }
}
