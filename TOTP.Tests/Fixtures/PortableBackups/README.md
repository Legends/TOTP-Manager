# Synthetic portable-backup fixture

`synthetic-ui-test-backup.totp` is an encrypted backup created exclusively from
synthetic accounts for manual live UI import and restore testing.

Rules for this fixture:

- It must never contain a real account, OTP seed, password, user name, or other
  personal data.
- Do not replace it with a backup from a production vault.
- Its decryption password is intentionally not stored in the repository.
- Automated tests should create purpose-specific temporary exports unless they
  explicitly require a stable compatibility fixture.
