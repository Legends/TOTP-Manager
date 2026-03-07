# Automatic Update Setup (No Code-Signing Cert Required)

This project uses NetSparkle with Ed25519 update signatures.

## 1. Keep It Disabled Until Configured
Default in `TOTP/appsettings.json`:
- `AutoUpdate:Enabled = false`

## 2. Generate NetSparkle Ed25519 Keys
Install tool once:

```powershell
dotnet tool install --global NetSparkleUpdater.Tools.AppCastGenerator
```

Generate keys:

```powershell
netsparkle-generate-appcast --generate-keys
```

This creates:
- public key (place into app config/user-secrets as `AutoUpdate:PublicKey`)
- private key file (keep private; use only in release pipeline)

## 3. Configure App for Update Checks
Recommended via user-secrets for local testing:

```powershell
dotnet user-secrets set "AutoUpdate:Enabled" "true" --project TOTP/TOTP.UI.WPF.csproj
dotnet user-secrets set "AutoUpdate:AppcastUrl" "https://example.com/appcast.xml" --project TOTP/TOTP.UI.WPF.csproj
dotnet user-secrets set "AutoUpdate:PublicKey" "<your-public-key>" --project TOTP/TOTP.UI.WPF.csproj
```

## 4. Publish Release + Appcast
Use helper script:

```powershell
pwsh ./scripts/release/Generate-Appcast.ps1 `
  -ReleaseFolder "./publish" `
  -BaseDownloadUrl "https://github.com/<owner>/<repo>/releases/download/<tag>/" `
  -PrivateKeyPath "C:\secure\NetSparkle_Ed25519.priv" `
  -PublicKeyPath "C:\secure\NetSparkle_Ed25519.pub"
```

## 5. Host Files
Host these over HTTPS:
- appcast XML
- release artifact(s) referenced by appcast

## 6. CI Secrets (for automatic appcast publishing)
Set these repository secrets:
- `NETSPARKLE_PUBLIC_KEY`
- `NETSPARKLE_PRIVATE_KEY`

When present, publish workflow will:
- generate `publish/appcast.xml`
- upload `TOTP.UI.WPF.exe`, `appcast.xml`, and `appcast.xml.signature` to GitHub Releases

## 7. Security Notes
- Ed25519 appcast signatures protect update integrity/authenticity.
- Keep private key out of repo and only in secure CI secret storage.
- If key is exposed: rotate immediately and publish new appcast signed with new key.
