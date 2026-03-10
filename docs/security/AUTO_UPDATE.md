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

Install flow details:
- see `docs/security/AUTO_UPDATE_INSTALL_PROCESS.md` for the current in-place update and relaunch process used by the custom NetSparkle UI

## 5. Host Files
Host these over HTTPS:
- appcast XML
- release artifact(s) referenced by appcast

## 5a. Local IIS Test Strategy
Use this when the GitHub repo or release assets are private and you want to debug NetSparkle locally.

1. Publish two local builds with different versions.
   First build: install or run the older version that will act as the currently installed app.
   Second build: generate the package referenced by the appcast with a higher version.

2. Host the update payload and `appcast.xml` from local IIS.
   Put `appcast.xml`, its `.signature`, and the referenced installer or executable in the same IIS site or virtual directory.

3. Point the app at the local feed via user-secrets.

```powershell
dotnet user-secrets set "AutoUpdate:Enabled" "true" --project TOTP/TOTP.UI.WPF.csproj
dotnet user-secrets set "AutoUpdate:AppcastUrl" "http://localhost/appcast.xml" --project TOTP/TOTP.UI.WPF.csproj
dotnet user-secrets set "AutoUpdate:PublicKey" "<your-public-key>" --project TOTP/TOTP.UI.WPF.csproj
dotnet user-secrets set "AutoUpdate:CheckOnStartup" "true" --project TOTP/TOTP.UI.WPF.csproj
dotnet user-secrets set "AutoUpdate:ForceStartupCheck" "true" --project TOTP/TOTP.UI.WPF.csproj
dotnet user-secrets set "AutoUpdate:InteractiveDebugCheckOnStartup" "true" --project TOTP/TOTP.UI.WPF.csproj
dotnet user-secrets set "AutoUpdate:CheckIntervalMinutes" "5" --project TOTP/TOTP.UI.WPF.csproj
```

4. Start the app with verbose logging enabled.

```powershell
dotnet run --project TOTP/TOTP.UI.WPF.csproj -- --debug
```

5. Inspect the log file in the published app folder under `Logs\app.log`.
   Look for:
   - `appcast fetch status=200`
   - `update detected`
   - `user responded to update prompt`
   - `download had an error`

6. Once local testing is complete, turn off the debug-only settings.
   `InteractiveDebugCheckOnStartup` should be set back to `false`.
   `ForceStartupCheck` should be set back to `false` unless you explicitly want to bypass NetSparkle's normal throttling.

## 5b. Common Local Failure Modes
- The installed app version is not lower than the version in `appcast.xml`.
- The appcast signature matches an old file but not the currently hosted XML.
- The enclosure `url` points to GitHub or another location that is still private.
- NetSparkle skips a startup check because the last successful check was too recent.
- The app can fetch the appcast but cannot download the referenced installer from IIS.

## 5c. Switching Back To GitHub Hosting
Once local testing is complete and the GitHub repository/releases are public again:

```powershell
powershell -ExecutionPolicy Bypass -File D:\Repos\TOTP-Manager\scripts\release\Setup-GitHubAutoUpdate.ps1
```

That restores the hosted appcast URL, the production NetSparkle public key, disables the local forcing flags, and returns the check interval to a daily cadence.

After switching back:
- verify `https://github.com/Legends/TOTP-Manager/releases/latest/download/appcast.xml` returns `200`
- verify the app log shows the GitHub URL as the final appcast URI
- verify signature validation succeeds with the production public key

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
