# M5 feature-parity matrix

This matrix closes the M5 feature-migration work package. “Complete” means the capability is present behind the portable contracts and Avalonia presentation boundary with automated evidence. It does not mean every M6 platform adapter or release package is available.

| Capability | Avalonia status | Compatibility / evidence | Approved remaining platform work |
|---|---|---|---|
| Password setup and unlock | Complete | Shared portable authorization envelope and regression suite | Physical target validation remains postponed M3 evidence |
| Quick unlock and recovery | Complete | Optional adapter, password fallback, rotation and rollback status | macOS Keychain/LocalAuthentication is M6 |
| Account list and CRUD | Complete | Native virtualized list, secret-free rows, validation and 10k tests | Physical scale/accessibility evidence remains postponed M3 evidence |
| Continuous TOTP and clipboard | Complete | Shared generator; conditional ownership-aware clearing | Wayland capability remains fail-closed until an ownership adapter exists |
| QR generation and camera import | Complete | Shared strict QR validation, conflict workflow, cancellation and disposal | Physical cameras/permissions remain postponed M3 evidence |
| Settings and localization | Complete | Shared allowlisted preference schema; English/German dynamic resources | Additional locales are product scope, not parity gaps |
| Import/export/backup | Complete | WPF path and Avalonia stream formats interoperate; native storage providers supported | Native-provider behavior requires later physical macOS/Linux evidence |
| Notifications and diagnostics | Complete | Severity-aware banners/dialogs, allowlisted startup/support output, redaction tests | None |
| Signed update UI and download | Complete | Explicit check/download/install states; appcast and package Ed25519 verification | Feed remains disabled for preview packages; installer execution adapters and target-qualified release artifacts are M6/release work |

## Deliberate differences from WPF

- Avalonia does not offer plaintext export. It imports the legacy plaintext formats for compatibility, but creates encrypted portable backups only.
- Unsupported QR algorithm, digit, and period values are rejected because the current encrypted account schema cannot persist them accurately.
- Remote release notes are displayed as bounded plain text, not embedded HTML.
- A generic WPF appcast enclosure is never accepted by the portable update client. Portable entries must identify both OS and architecture.
- The M5 installer adapter is deliberately unavailable. A verified download can reach the ready state, but cannot execute until an M6 adapter reverifies the package and performs the platform-specific handoff.

All differences above are explicit security or platform-boundary decisions. There are no unapproved M5 capability gaps.
