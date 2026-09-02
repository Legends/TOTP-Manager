# Android foundation

Android development is isolated from the pending desktop `v2.0.0` release on the temporary
`feature/android-foundation` branch. This is not a permanent platform branch. After the desktop
release, reviewed Android work should reach `master` through small vertical-slice pull requests.

## Current scope

The foundation contains:

- a platform-neutral Avalonia mobile application project
- a minimal .NET Android host
- reuse of the existing shared Avalonia controls and visual resources
- secure-by-default Android manifest settings
- screenshot and recent-app preview protection through `FLAG_SECURE`

It intentionally does not yet connect to the production vault. Android-specific key storage,
biometric authorization, lifecycle locking, camera access, clipboard behavior, file import/export,
and release signing require separate security-reviewed slices.

`NSec.Cryptography` remains part of the desktop infrastructure but is intentionally not pulled into
the empty mobile host. Its current native `libsodium` dependency emits an Android 16 compatibility
warning for 16 KB memory pages. Before Android reuses the vault implementation, that dependency must
be upgraded, replaced, or supplied with verified Android-native binaries. Suppressing the warning is
not an acceptable release solution.

The Android project is deliberately not included in `TOTP.sln` yet. Existing desktop CI and the
pending `v2.0.0` release therefore remain unchanged. Build the foundation explicitly:

```powershell
dotnet restore .\TOTP.UI.Avalonia.Android\TOTP.UI.Avalonia.Android.csproj --configfile .\NuGet.config
dotnet build .\TOTP.UI.Avalonia.Android\TOTP.UI.Avalonia.Android.csproj -c Debug --no-restore
```

## Branch and merge policy

1. Keep the foundation branch rebased on `master` while desktop `v2.0.0` is pending.
2. Do not create Android release artifacts or Play Store credentials on this branch.
3. After desktop `v2.0.0`, merge reviewed vertical slices rather than maintaining a long-lived
   Android fork.
4. Add the Android project to supported CI only when the Android security adapters and tests are
   ready to become a maintained product surface.

The initial Android application ID is `io.github.legends.otpharbor`. It must receive explicit review
before the first store publication because a published application ID is effectively permanent.

## Security review notes

- **Threat impact:** the host disables Android backup and cleartext network traffic and marks its
  window as secure to prevent ordinary screenshots and recent-app previews. It does not claim to
  resist a rooted or otherwise compromised device.
- **Data-flow impact:** none yet. The mobile host does not resolve persistence, vault, import/export,
  clipboard, camera, update, or authorization services and therefore handles no OTP secrets.
- **Compatibility impact:** none for existing desktop packages. The Android projects are not part of
  `TOTP.sln`, and the unused `NSec.Cryptography` reference was removed only from `TOTP.Core`; the
  infrastructure project that uses NSec retains its own direct dependency.
- **Test evidence:** Debug and Release Android builds must complete without warnings. The existing
  desktop solution build and targeted PR-like test subset must remain green.
