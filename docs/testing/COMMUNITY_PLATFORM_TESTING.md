# Community platform testing

Physical platform testing complements CI; it does not replace automated security and regression checks. Use only synthetic TOTP accounts created for testing. Never share real OTP seeds, QR codes, one-time codes, passwords, vaults, backups, personal paths, or unreviewed logs.

## Help wanted: MacBook acceptance

OTP Harbor needs a contributor with an Apple Silicon MacBook running macOS 14 or newer to validate:

1. application installation, first launch, restart, and clean removal;
2. master-password setup, lock, unlock, and failed-password behavior;
3. Keychain quick-unlock enrollment and user-presence prompts through Touch ID, Apple Watch, or the macOS password fallback available on that Mac;
4. safe recovery after canceling authentication and after deleting/resetting the test Keychain item;
5. camera permission, QR scan, manual account entry, edit, delete, search, and code copy;
6. conditional clipboard clearing without overwriting clipboard content replaced by another app;
7. idle, minimize, and macOS session-lock behavior;
8. encrypted backup export and restore using synthetic accounts; and
9. DMG packaging, Finder installation, Gatekeeper behavior, and—once credentials exist—signature, notarization, and stapling.

Record the macOS version, Mac model/chip, OTP Harbor commit or release, the completed checklist items, and sanitized results. Report functional problems with the bug template. Report security vulnerabilities privately through [SECURITY.md](../../SECURITY.md).

The detailed maintainer checklist remains in [M6 physical acceptance](../architecture/M6_PHYSICAL_ACCEPTANCE.md).
