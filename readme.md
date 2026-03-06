# TOTP Manager

TOTP Manager is a Windows desktop app (WPF) for managing and generating Time-based One-Time Passwords (TOTP) locally.

## Scope

- Local token storage and code generation
- Import/export workflows for backup and migration
- Authorization flows based on master password and optional Windows Hello support

## Core Security Design

- Secrets are encrypted before they are written to disk
- Password-derived key material uses Argon2id
- Additional local protection uses Windows DPAPI
- Sensitive actions require explicit authorization

See also:
- `docs/security/THREAT_MODEL.md`
- `docs/security/SECURITY_VERIFICATION.md`
- `docs/security/PENTEST_PLAN.md`

## Features

- Secure local vault for tokens
- Create, edit, and delete TOTP tokens
- Generate rotating 6-digit TOTP codes
- Search and manage tokens in the main grid
- Encrypted export/import for backups (`.totp`)
- Backup rotation support
- Localization resources (English/German)

## Requirements

- Windows 10/11
- .NET 9 SDK (for local build/test)

## Build

```powershell
dotnet restore TOTP.sln
dotnet build TOTP.sln -c Debug
```

## Test

```powershell
dotnet test TOTP.sln -c Debug
```

## Run (Local)

```powershell
dotnet run --project .\TOTP\TOTP.UI.WPF.csproj
```

## Release Installation

1. Download the latest release archive from GitHub.
2. Extract it to a local folder.
3. Start `TOTP.UI.WPF.exe`.
4. Complete first-run security setup.

## Token Management (CRUD + QR)

These are the primary workflows in the app.

### Create token

1. Click the `+` button or press `Ctrl + A`.
2. Enter issuer, account label, secret, digits, and period.
3. Save to create the token.

### Read token

1. Open the main token list.
2. Select a token to view current code and metadata.
3. Use search to quickly filter by issuer/account.

### Update token

1. Select a token.
2. Right-click the row and choose `Edit` for full edit.
3. For quick inline edit, double-click the row to edit only the issuer name.
4. Save changes.

### Delete token

1. Select a token.
2. Right-click the row and choose `Delete`.
3. Confirm removal.

### Generate QR code from token

1. Select an existing token.
2. Click "Show QR code".
3. The app generates a QR code from the token's OTP configuration.
4. Click the QR code in order to enlarge it.

### Scan QR code to add TOTP

1. Click the camera symbol.
2. The camera activates and is ready to scan.
3. Scan the `otpauth://` QR code from the provider.
4. Review parsed token details and save.

## Backup and Recovery Notes

- Keep encrypted backups in a protected location
- Validate restore procedure regularly
- Keep master password and Windows account recovery options available

## Contributing

See `CONTRIBUTING.md` for contribution and workflow details.

## Support

- Bugs and feature requests: GitHub Issues
- Security topics: follow the repository security process/documentation
