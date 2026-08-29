# Automatic Update Setup

TOTP Manager's direct Avalonia packages use an Ed25519-signed, target-qualified `appcast-v2.xml`. Windows and macOS releases also require platform signing/notarization; Ed25519 package signatures do not replace operating-system trust.

Linux DEB and future store-managed builds are stamped as externally managed and do not use application-owned updates.

## Trust model

- The client accepts only `appcast-v2.xml` entries whose OS, architecture, channel, and package policy match the running package.
- Every direct payload, the release manifest, and the appcast are signed with the configured NetSparkle Ed25519 key.
- Stable Windows executables require Authenticode signing. Stable macOS artifacts require Developer ID signing and notarization.
- Unsigned RC packages disable automatic updates and are distributed only as explicit manual-download previews.

## Generate Ed25519 keys

Install the pinned tool:

```powershell
dotnet tool install --global NetSparkleUpdater.Tools.AppCastGenerator --version 2.9.0
```

Generate keys:

```powershell
netsparkle-generate-appcast --generate-keys
```

Keep `NetSparkle_Ed25519.pub` and `NetSparkle_Ed25519.priv` together in a protected directory. Commit neither file. The public key configured in `TOTP.UI.Avalonia.Desktop/appsettings.json` must exactly match `NETSPARKLE_PUBLIC_KEY` in CI.

## Configure a development feed

Use user-secrets on the Avalonia desktop project:

```powershell
dotnet user-secrets set "AutoUpdate:Enabled" "true" --project TOTP.UI.Avalonia.Desktop/TOTP.UI.Avalonia.Desktop.csproj
dotnet user-secrets set "AutoUpdate:AppcastUrl" "https://example.com/appcast-v2.xml" --project TOTP.UI.Avalonia.Desktop/TOTP.UI.Avalonia.Desktop.csproj
dotnet user-secrets set "AutoUpdate:PublicKey" "<your-public-key>" --project TOTP.UI.Avalonia.Desktop/TOTP.UI.Avalonia.Desktop.csproj
```

Run the client with:

```powershell
dotnet run --project TOTP.UI.Avalonia.Desktop/TOTP.UI.Avalonia.Desktop.csproj
```

Do not enable application-owned updates in DEB, Microsoft Store, or other externally managed packages.

## Generate and validate the portable appcast

The release workflow first creates and validates `release-artifacts-v2.json`, then runs:

```powershell
./scripts/release/Generate-AvaloniaAppcast.ps1 `
  -ManifestPath ./release-assets/release-artifacts-v2.json `
  -ArtifactDirectory ./release-assets `
  -BaseDownloadUrl "https://github.com/<owner>/<repo>/releases/download/<tag>/" `
  -PrivateKeyPath "C:\secure\NetSparkle_Ed25519.priv" `
  -PublicKeyPath "C:\secure\NetSparkle_Ed25519.pub" `
  -ExpectedPublicKey "<your-public-key>" `
  -OutputDirectory ./release-feed

./scripts/release/Test-AvaloniaAppcast.ps1 `
  -AppcastPath ./release-feed/appcast-v2.xml `
  -ManifestPath ./release-assets/release-artifacts-v2.json `
  -BaseDownloadUrl "https://github.com/<owner>/<repo>/releases/download/<tag>/"
```

The private key path is supplied to tooling; private key contents must never appear in process arguments or logs.

## Required CI secrets

- `NETSPARKLE_PUBLIC_KEY`
- `NETSPARKLE_PRIVATE_KEY`
- `SIGNPATH_API_TOKEN` (submitter permission only)
- `SIGNPATH_ORGANIZATION_ID` (GitHub Actions repository variable)
- macOS Developer ID/notarization secrets documented by the release workflow

The SignPath project, policy, and artifact-configuration slugs are fixed in the workflow and documented in [SIGNPATH_FOUNDATION_ONBOARDING.md](SIGNPATH_FOUNDATION_ONBOARDING.md). Every stable Windows signing request requires separate manual approval in SignPath. Stable tags fail closed when required configuration or approval is absent. RC tags publish only explicitly labeled unsigned Windows/Linux previews with automatic updates disabled.

## Release behavior

For a stable version tag, CI:

1. Builds and tests all supported projects.
2. Produces target-qualified Avalonia packages.
3. Applies platform signatures where required.
4. Signs every direct payload and the aggregate release manifest.
5. Generates and verifies `appcast-v2.xml`.
6. Uploads the complete asset set to a draft and publishes it only after validation succeeds.

See [AUTO_UPDATE_INSTALL_PROCESS.md](AUTO_UPDATE_INSTALL_PROCESS.md) for the verified installation handoff and [SIGNING_KEY_ROTATION.md](SIGNING_KEY_ROTATION.md) for rotation procedures.

## Incident response

If an Ed25519 private key or platform certificate is exposed, stop publishing, revoke/rotate the affected credential, update the embedded trust material through a reviewed release, and document the impact. Never weaken signature verification to recover from a rotation failure.
