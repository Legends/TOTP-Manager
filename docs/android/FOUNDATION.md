# Android development

Android development is isolated from the pending desktop `v2.0.0` release on the temporary
`feature/android-foundation` branch. This is not a permanent platform branch. After the desktop
release, reviewed Android work should reach `master` through small vertical-slice pull requests.

## Implemented development MVP

The current Android application provides:

- password-based creation and unlocking of the production encrypted vault
- account listing, manual creation, editing, and deletion
- TOTP generation and countdown display
- conditional clipboard clearing without retaining the copied code as adapter state
- automatic locking and clearing of the in-memory account projection when the app enters the
  background
- English and German localization
- private Android application storage with verified owner-only Unix permissions
- disabled Android backup and cleartext network traffic
- screenshot and recent-app preview protection through `FLAG_SECURE`

Account rows contain identifiers and display metadata only. OTP seeds remain in the encrypted
vault and are loaded through the existing authorization-aware services when required.

This is a development build, not an Android release candidate. The following capabilities are
deliberately deferred until the base workflow has been verified on physical hardware:

- biometric quick unlock backed by Android Keystore
- QR scanning and camera permission handling
- import, export, and Android document-picker integration
- production signing, Play Store packaging, upgrade policy, and supported Android CI

## Build and install

The Android project is deliberately not included in `TOTP.sln` yet, so existing desktop CI and the
pending `v2.0.0` release remain unchanged.

Build explicitly:

```powershell
dotnet restore .\TOTP.UI.Avalonia.Android\TOTP.UI.Avalonia.Android.csproj --configfile .\NuGet.config
dotnet build .\TOTP.UI.Avalonia.Android\TOTP.UI.Avalonia.Android.csproj -c Debug --no-restore
```

To install on one USB-connected, authorized Android device and launch the app:

```powershell
.\scripts\testing\Install-AndroidDevelopmentBuild.ps1
```

The script performs an in-place development APK install and does not clear app data. USB debugging
must be enabled and the computer must be authorized on the device.

## Branch and merge policy

1. Keep the foundation branch based on `master` while desktop `v2.0.0` is pending.
2. Do not create production Android artifacts or Play Store credentials on this branch.
3. After desktop `v2.0.0`, merge reviewed vertical slices rather than maintaining a long-lived
   Android fork.
4. Add the Android projects to supported CI only when the platform adapters and device tests are
   ready to become a maintained product surface.

The initial Android application ID is `io.github.legends.otpharbor`. It must receive explicit review
before the first store publication because a published application ID is effectively permanent.

## Security review notes

- **Threat impact:** the host prevents ordinary backup, screenshots, recent-app previews, and
  cleartext network traffic. It does not claim to resist a rooted or otherwise compromised device.
- **Data-flow impact:** the existing authorization, encryption, vault, account, settings, TOTP, and
  clipboard services now process production data on Android. Files stay below the private app data
  root. Copied codes enter the system clipboard, are marked sensitive on Android 13 and newer, and
  are conditionally cleared according to the existing setting.
- **Compatibility impact:** the Android projects remain outside `TOTP.sln`. The infrastructure
  dependency on `NSec.Cryptography` was upgraded from 25.4.0 to 26.4.0 for current Android native
  runtime and 16 KB page-size support; vault formats and cryptographic algorithms were not changed.
  Desktop and Android vault files are not yet presented as an interchange format because no mobile
  import/export workflow exists.
- **Recovery impact:** password unlock remains the only Android recovery and authorization method in
  this slice. Android backup is intentionally unavailable; deleting app data deletes the local
  vault. Users must not treat this development build as the sole copy of an authenticator account.
- **Test evidence:** localized resource completeness and mobile startup, locking, account projection,
  and account validation behavior have automated coverage. Debug and Release Android builds plus
  desktop regression tests must remain green. Private storage, NSec runtime behavior, lifecycle
  transitions, and clipboard clearing still require physical-device verification.
