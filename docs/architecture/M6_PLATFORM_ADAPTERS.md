# M6 desktop platform adapters

## Baseline audit

M6 begins from a stronger baseline than the original checklist assumed. Earlier vertical slices already delivered and tested Windows v2 authorization storage, development-vault conflict handling, Hello-backed quick unlock with password recovery, ACL hardening, named-mutex/named-pipe activation, conditional clipboard handling, native dialogs, and shell integration. macOS application paths and Unix permission enforcement are implemented, as are Linux XDG paths and the fail-closed master-password fallback.

Those implementations and their automated contract evidence are complete. Physical macOS/Linux behavior remains part of the target acceptance evidence; automated checks do not claim that target-host acceptance has run.

## Windows lifecycle lock slice

The Avalonia process now registers a Windows `IPlatformSessionEventSource` backed by `Microsoft.Win32.SystemEvents.SessionSwitch`. Lock, unlock/logon/connect, disconnect/logoff, and unknown reasons are mapped to the closed Core session-state enum. Subscription is idempotent and is removed during application shutdown.

`SessionLockPolicyBackgroundService` is now part of the Avalonia composition root and is explicitly started/stopped with the desktop lifetime. A configured Windows session lock first clears authorization state, then raises a metadata-free application-locked event. `MainWindowViewModel` marshals that event through `IUiScheduler` and clears account, OTP, QR, camera, tools, and authorized-shell state. This closes the prior gap where the key could be locked while secret-bearing presentation state remained visible.

Window minimization now delegates to the same view-model lock transition when the persisted `LockOnMinimize` policy is enabled. Unsaved settings edits do not alter the policy because the decision reads `ISettingsService.Current`, not the settings editor projection.

- Threat impact: Windows session lock and configured minimization both remove authorization and secret-bearing presentation output. A platform event cannot leave the UI visibly authorized after the vault key is cleared.
- Data-flow impact: only the closed session-state enum and a metadata-free lock event cross platform/presentation boundaries. No session identifier, username, desktop name, or exception message is retained or logged.
- Compatibility impact: storage formats are unchanged. A host without a reliable session source reports the capability as unavailable rather than claiming lock monitoring.
- Recovery impact: unlocking always returns through the existing password/optional quick-unlock recovery gate; session events never modify the authorization envelope.
- Test evidence: policy tests cover enabled/disabled behavior, non-lock states, source lifetime, UI notification, and sanitized lock failure logging. Main-window tests cover configured minimize lock and secret-surface teardown. The complete solution continues to compile without warnings.

Physical Windows session-switch acceptance remains required before the M6 exit criteria can be signed off.

## Windows signed update execution slice

The Windows Avalonia host now replaces the portable fail-closed installer placeholder with a Windows adapter. The adapter accepts only a regular ZIP package produced by the portable update service, opens it without write/delete sharing, enforces the same 128 MiB ceiling, and repeats Ed25519 package verification immediately before process handoff. A failed or malformed signature never reaches process execution.

The existing `TOTP.Updater` helper remains isolated from Avalonia and is bundled under the Windows desktop output. Before staging, the adapter requires a regular helper directory and executable and rejects reparse points anywhere in the copied helper bundle. It resolves the running executable only when it is directly inside the selected installation directory, constructs each helper argument through `ProcessStartInfo.ArgumentList`, and does not request application shutdown until the helper signals that its window is ready.

Production update enablement remains subject to the target-qualified release and signing policy. This adapter does not authorize an unqualified release artifact.

- Threat impact: a package modified after download verification, an unsigned package, a non-ZIP payload, a redirected helper bundle, or an executable outside the active installation directory fails closed before application shutdown.
- Data-flow impact: package bytes are read once more at the platform boundary and cleared after verification. Only local package/install paths and the parent process ID are passed to the bundled helper; none are exposed by the Avalonia view model or written to the application log.
- Compatibility impact: authorization envelope, vault, export, and appcast formats are unchanged. Windows artifacts include the updater runtime in a dedicated subdirectory.
- Recovery impact: failure before helper readiness leaves the application running and reports the existing sanitized installer error. After readiness, the helper owns visible progress, in-place replacement, and relaunch.
- Test evidence: adapter tests cover signature revalidation, structured helper handoff followed by shutdown, rejection of non-ZIP payloads, and rejection of an executable outside the selected installation directory. Build output inspection confirms the helper executable is bundled.

Physical install/relaunch acceptance against a target-qualified signed Avalonia ZIP remains required before the M6 exit criteria can be signed off.

## macOS authorization adapters

The macOS host now registers a `MacOSKeychainSecretStore` and `MacOSPlatformQuickUnlock`. The native adapter targets the data-protection Keychain and creates a generic-password item guarded by `SecAccessControlCreateWithFlags` using `userPresence`. Availability is evaluated with LocalAuthentication's device-owner policy, which permits Touch ID, Apple Watch, or the macOS account password according to system policy.

The 32-byte vault key exists in caller-owned memory only long enough to cross the Keychain call. The authorization envelope stores an opaque item reference plus a SHA-256 reference binding, never the vault key or a software-encrypted substitute. The closed v2 contract now recognizes this exact provider/version/policy/algorithm shape. Retrieval prompts through Keychain access control and returns a disposable `SensitiveBuffer`; missing or deleted items fall back to master-password recovery.

- Threat impact: quick unlock cannot silently degrade to plaintext or a software-only local key. Copying the envelope to another device does not copy the Keychain item. A modified reference binding is rejected before Keychain access.
- Data-flow impact: registration sends the vault key directly to Security.framework on a background worker. Temporary native and managed buffers are cleared. Logs contain operation and exception types only.
- Compatibility impact: the password wrapper and vault format do not change. Windows Hello wrappers remain valid and provider-routed; Linux remains master-password-only.
- Recovery impact: enrollment still requires the verified recovery password. Missing, cancelled, unavailable, or reset Keychain state returns to password recovery.
- Test evidence: contract, store, provider, tampered-binding, missing-item, idempotent-delete, and native-framework smoke tests are present. Physical Keychain/LocalAuthentication prompts remain in the macOS acceptance checklist.

## Linux Secret Service and session lock

Linux now exposes `LinuxSecretServiceStore` through the existing platform-secret contract. The adapter requires `secret-tool` plus a live session D-Bus. Binary secrets are base64-encoded into clearable UTF-8 buffers and written only to standard input; references are non-secret structured arguments. Lookup output is bounded, decoded without immutable secret strings, copied into a disposable buffer, and cleared. This capability is not wired as Linux quick unlock: the approved Linux authorization policy remains master-password-only.

`LinuxSessionEventSource` monitors the session bus for the standard freedesktop or GNOME `ScreenSaver.ActiveChanged` signal on selected GNOME, KDE/Plasma, Cinnamon, and MATE sessions. It maps only the signal's boolean payload to `Locked` or `Active`, suppresses duplicates, and does not infer locking from focus loss. Unsupported/headless desktops report unavailable instead of claiming protection.

- Threat impact: Secret Service values never enter process arguments or application logs. Session locking reacts only to a recognized OS/desktop signal.
- Data-flow impact: the secret store passes an opaque reference and clearable encoded bytes to `secret-tool`. The session adapter emits only the closed session-state enum.
- Compatibility impact: Linux authorization remains password-only. Absence of `libsecret-tools`, D-Bus, or a selected desktop is an explicit safe capability state.
- Recovery impact: Secret Service failure cannot block master-password unlock. Session-monitor failure leaves manual/minimize locking available and is reported as temporarily unavailable.
- Test evidence: store tests cover stdin-only transfer, bounded decoding, missing items, malformed output, absent D-Bus, and idempotent deletion. Session tests cover signal mapping, unrelated payload rejection, duplicate suppression, and monitor lifetime.

## Camera, clipboard, activation, and capability reporting

macOS camera preflight reads AVFoundation authorization state without prompting; the app bundle retains `NSCameraUsageDescription`, and first OpenCV use remains the explicit user-triggered prompt. Linux enumerates `/dev/video*` and tests read/write access before opening OpenCV, distinguishing no device from denied V4L2 access. Both feed the existing typed QR failure states.

The existing current-user named mutex/pipe activation transport is already exercised on Ubuntu and macOS CI. Clipboard writing remains available on all targets; ownership-safe conditional clearing is enabled on macOS and X11 and deliberately reported unavailable on Wayland where Avalonia cannot prove ownership.

`IPlatformCapabilityReport` exposes the required closed states: `Supported`, `TemporarilyUnavailable`, `PermanentlyUnavailable`, `PermissionDenied`, `Misconfigured`, and `Failed`. Avalonia support diagnostics include only capability name/state pairs and continue to omit paths, account data, and secrets.

## Distribution implementation

The initial macOS channel is an ARM64 Developer ID app in a notarized DMG. The release script creates stable bundle metadata, enables the minimum .NET hardened-runtime entitlements plus camera access, signs nested Mach-O files and the app without `--deep` signing, signs the DMG, submits with `notarytool`, staples, and asks Gatekeeper to assess it. CI builds and verifies an unsigned structural DMG; credentialed signing/notarization remains a release gate.

Linux ships as a self-contained x64 portable tarball and an Ubuntu/Debian DEB. The DEB installs under `/opt/totp-manager`, supplies a launcher and desktop entry, and declares `libsecret-tools` plus native graphics/runtime prerequisites. AppImage is deferred until it has a maintained update, desktop-integration, and D-Bus policy; PKG is not selected on macOS because the app has no privileged daemon or system-wide payload.

Detailed commands and physical acceptance records are in `docs/architecture/AVALONIA_DESKTOP_DISTRIBUTION.md` and `docs/architecture/M6_PHYSICAL_ACCEPTANCE.md`.
