# TOTP Manager

A security-first desktop authenticator for managing Time-based One-Time Passwords (TOTP) on Windows.

## Project Overview

TOTP Manager is a Windows WPF application for storing and generating 2FA/TOTP codes locally.  
It is designed for users who want stronger protection than typical authenticator apps by combining:

- Modern password-based key derivation (Argon2id)
- Windows-native local secret protection (DPAPI)
- Secure backup/export workflows
- Strict local-first handling of secrets

### Why it is more secure than many standard apps

- Secrets are encrypted at rest, not stored in readable form.
- Local credential material is protected with OS-backed protection (DPAPI).
- Password hardening uses Argon2id, which is resilient against brute-force attacks.
- Security-sensitive actions use explicit authorization flows.
- Secure-by-design is the baseline, not an optional mode.

## Key Features

- TOTP generation for standard authenticator-compatible services
- Secure local vault for OTP seeds/accounts
- Encrypted export/import for backup and migration
- Backup rotation support to reduce risk from stale backups
- Authorization mode switching (master password and/or Windows Hello flow)
- Security-focused settings and recovery flows

## Installation

1. Go to the latest release page on GitHub.
2. Download the current Windows package (`.zip` release artifact).
3. Extract to a trusted local folder.
4. Launch `TOTP.UI.WPF.exe`.
5. On first run, complete initial security setup (master password / authorization settings).

## Usage Guide

### Add a new token

1. Open the app and choose **Add Token**.
2. Enter issuer/account details.
3. Paste secret (or import from supported format).
4. Save and verify generated code updates every 30 seconds.

### Manage backups

1. Open **Settings > Export/Import**.
2. Choose encrypted export (`.totp`) for recommended backups.
3. Select destination and confirm password/authorization.
4. Store backups in a protected location (offline copy recommended).
5. Periodically rotate old backups and keep only required recovery points.

### Configure security settings

1. Go to **Settings > Security**.
2. Change master password when needed.
3. Configure preferred authorization mode.
4. Review export/import and recovery options after changes.

## Security Transparency (High-Level)

Your secrets are protected in two layers:

- **Layer 1: Password hardening (Argon2id)**  
  Your password is transformed into strong cryptographic material that is expensive to brute-force.

- **Layer 2: Windows local protection (DPAPI)**  
  Local credential artifacts are additionally protected using your Windows account context.

In short: your TOTP data is encrypted before storage, and decryption is only possible through authorized flows.

## Important Notes

- Keep your master password in a safe place.
- Use encrypted backups regularly.
- Do not share exported backup files without encryption.
- Keep your Windows account and device security (PIN/biometric/OS updates) in good health.

## Support

- Open an issue in the GitHub repository for bugs or feature requests.
- For security reports, use the project’s responsible disclosure/security channel if available.
