using System;

namespace TOTP.Core.Models
{
    public sealed class AccountItem : IEquatable<AccountItem>
    {
        public string Platform { get; }
        public string Secret { get; }
        public string? Account { get; }

        public Guid ID { get; set; }

        public AccountItem(Guid id, string platform, string secret, string? account = null)
        {
            ID = id;
            Platform = platform;
            Secret = secret;
            Account = account;
        }

        public bool Equals(AccountItem? other) => other is not null && ID == other.ID;

        public override bool Equals(object? obj) => Equals(obj as AccountItem);
        public override int GetHashCode() => ID.GetHashCode();
    }
}
