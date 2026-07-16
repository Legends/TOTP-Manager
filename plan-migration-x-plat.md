# Cross-Platform Migration Plan

## Executive recommendation

Use **native Avalonia UI** as the long-term cross-platform UI framework.

It best fits the current application because TOTP Manager is desktop-first, already uses WPF/XAML and MVVM, and needs trustworthy local OS integration on Windows, macOS, and Linux. Most C#, dependency injection, workflows, domain models, cryptography, and view-model behavior can remain in .NET while the WPF presentation layer is replaced.

Avalonia officially supports Windows, macOS, Linux, iOS, Android, and WebAssembly:

- <https://docs.avaloniaui.net/docs/supported-platforms>

The initial product scope should be limited to:

- Windows
- macOS
- Linux desktop

The intended longer-term sequence is:

1. Establish production-quality support for Windows, macOS, and Linux desktop.
2. Add Android and iOS clients on the same portable application and security foundation.
3. Evaluate a web client separately, with its own threat model and deliberately limited capabilities unless secure cross-device synchronization is introduced.

If mobile and browser support become primary product requirements rather than possible future aspirations, **Uno Platform** becomes the strongest alternative.

Do not select .NET MAUI for a three-desktop-OS target because Linux is not an officially supported MAUI platform:

- <https://learn.microsoft.com/en-us/dotnet/maui/supported-platforms>

## Goals

The migration must preserve the current product posture:

- Local-first operation
- No plaintext secret persistence
- Password-based recovery on every supported OS
- Optional OS-backed quick unlock
- Existing encrypted vault compatibility
- Safe import/export and backup behavior
- Signed and verified updates
- Reliable single-instance behavior
- Predictable lock and idle behavior
- QR generation and camera scanning
- Regression coverage for security-sensitive workflows

The migration is not complete merely when the UI runs on another OS. It is complete when storage, authorization, recovery, updates, packaging, and release verification work safely across supported platforms.

## Product rollout strategy: desktop first, then mobile, then optional web

The desktop-first sequence is appropriate for the current product and codebase. It allows the existing WPF application to remain releasable while portable security and application layers are extracted, and it avoids combining a desktop migration with a simultaneous mobile UX redesign.

The architecture created during the desktop migration must nevertheless be mobile-ready. This means platform boundaries should be established for all OS services from the beginning, even when the first implementations cover only Windows, macOS, and Linux.

### Recommended rollout

#### Stage 1: the three main desktop operating systems

Deliver equivalent, production-ready clients for:

- Windows
- macOS
- Linux

Desktop parity includes:

- Vault setup and password recovery
- Platform quick unlock where safely available
- Account CRUD and search
- TOTP generation
- QR scanning and generation
- Clipboard clearing
- Idle and session locking
- Import/export and backup
- Signed updates
- Native packaging and installation

#### Stage 2: Android and iOS

Add Android and iOS after the desktop storage format, security model, platform contracts, and recovery workflow have stabilized.

Avalonia supports Android and iOS, so mobile does not inherently require adopting a second UI framework. However, desktop and mobile views should not be forced into identical layouts. The goal is to reuse application behavior, not to maximize XAML reuse at the expense of platform usability.

Mobile parity should initially include:

- Password setup and recovery
- Biometric quick unlock
- Account list and search
- TOTP generation and copy
- Camera-first QR import
- QR display for account transfer
- Import/export through platform sharing facilities
- App lifecycle locking
- Secure backup and recovery

Desktop-only concepts such as multiple windows, context menus, hover behavior, desktop installers, and session-switch events should not leak into shared mobile view models.

#### Stage 3: optional web client

Treat web support as a separate product and security decision rather than the automatic next target of a cross-platform UI framework.

A first web client could safely focus on capabilities such as:

- Documentation and onboarding
- OTP URI and encrypted-backup validation
- QR generation
- Temporary in-memory authentication sessions
- An encrypted backup inspector that never receives an unwrapped persistent key
- A companion interface for a future end-to-end encrypted synchronization service

Do not promise full offline-vault equivalence in the browser until a dedicated browser threat model is accepted. A browser cannot provide the same persistent non-exportable local key guarantees as DPAPI, Keychain, Secure Enclave, Android KeyStore, or a Linux Secret Service.

### Recommended multi-client project structure

Prepare for desktop and mobile clients without forcing a single presentation project:

```text
TOTP.Domain
TOTP.Application
TOTP.Cryptography
TOTP.Storage
TOTP.Platform.Abstractions

TOTP.Platform.Windows
TOTP.Platform.MacOS
TOTP.Platform.Linux
TOTP.Platform.Android
TOTP.Platform.iOS

TOTP.UI.Avalonia.Shared
TOTP.UI.Avalonia.Desktop
TOTP.UI.Avalonia.Mobile
```

The Android and iOS platform projects do not need to be created during the first desktop iteration, but the abstractions should avoid assumptions that make their later introduction difficult.

### What should be shared between desktop and mobile

- Domain models
- Cryptographic formats and algorithms
- Authorization workflows
- Account CRUD workflows
- Validation
- TOTP generation
- OTP URI parsing
- QR payload generation
- Import/export formats
- Backup formats
- Localization resources
- Theme tokens such as colors, spacing, and typography
- Form-factor-neutral view-model state
- Error codes and recoverable failure behavior
- Security logging and redaction rules

### What should remain form-factor-specific

- Navigation structure
- Window, page, dialog, and sheet composition
- Desktop account grid versus mobile account list
- Context menus versus swipe or long-press actions
- Keyboard shortcuts and pointer interactions
- Camera permission and scanning UX
- File dialogs versus mobile document/share pickers
- Desktop session-lock events versus mobile foreground/background lifecycle
- Desktop update installers versus App Store and Play Store update behavior
- Multi-window behavior
- Notification presentation
- Biometric enrollment UX

### Mobile security providers

#### Android

- Store non-exportable key material through Android KeyStore.
- Use the platform biometric prompt for quick unlock.
- Require the master password when biometric or protected key access is unavailable.
- Exclude sensitive encrypted-storage metadata from inappropriate automatic backup behavior.
- Test device migration, biometric enrollment changes, lock-screen changes, and KeyStore invalidation.

#### iOS

- Store key material through Keychain.
- Use Secure Enclave-backed keys where the design benefits and recovery semantics remain clear.
- Use LocalAuthentication for Face ID or Touch ID quick unlock.
- Define Keychain accessibility and device-only migration behavior explicitly.
- Test biometric enrollment changes, device restore, application reinstall, and Keychain access-group configuration.

#### Shared mobile rule

The master password remains the universal recovery mechanism. Biometric authentication is a platform-local convenience and must not become the only way to recover or migrate the vault.

Do not add automatic cross-device vault synchronization as an incidental part of mobile support. Synchronization would be a separate encrypted data protocol, conflict-resolution model, recovery design, and threat-model project.

### Framework decision timing

The staged product direction affects when the framework choice should be revisited:

- If mobile follows only after the desktop clients have matured, native Avalonia remains the preferred option.
- If Android and iOS must ship shortly after desktop, approximately within the same 12-month program, prototype the critical mobile workflows in both Avalonia and Uno before final commitment.
- If browser delivery becomes equally important to desktop and mobile, Uno becomes more compelling from a target-coverage perspective, although it does not solve persistent browser-vault security.

The mobile prototype should cover biometric unlock, camera QR import, lifecycle locking, a large account list, secure storage, export through the share sheet, and recovery after protected-key invalidation.

## Current repository assessment

At the time of this assessment, the repository contains approximately:

- 38 Core C# files
- 28 Infrastructure C# files
- 5 DAL C# files
- 161 UI C# files
- 33 XAML files
- 3,321 lines of XAML
- 98 files referencing `System.Windows`
- 32 Syncfusion integration points
- 432 passing tests compiled through a Windows target

### Expected reuse

| Area | Expected reuse | Notes |
|---|---:|---|
| Domain models and enums | 95-100% | Already mostly platform-neutral |
| TOTP generation | 95-100% | Otp.NET implementation is portable |
| Vault AES-GCM format | 90-100% | NSec and the current `TVLT` format are portable |
| Password KDF and DEK wrapping | 85-95% | Argon2id logic is portable |
| Account workflows | 75-90% | Requires removal of remaining presentation types |
| Import/export logic | 75-90% | Dialog and shell actions need adapters |
| View models | 55-75% | Some types and interaction patterns remain WPF-oriented |
| DAL | 50-65% | File I/O is portable; DPAPI and Windows ACL handling are not |
| XAML and views | 20-45% | Layout concepts transfer; markup does not transfer unchanged |
| Updater UI | 30-50% | NetSparkle core can remain; WPF UI/helper must change |
| Platform integrations | 10-30% | Hello, session lock, clipboard, notifications, and single-instance behavior need adapters |

## Critical security migration issue

The largest migration risk is not XAML. It is the storage and authorization envelope.

The account vault is relatively portable. `VaultService` uses AES-256-GCM, and the current encrypted vault format can remain compatible across supported operating systems.

The settings and authorization envelope are Windows-specific:

- `AppSettingsDAL` encrypts and decrypts the entire settings file with Windows DPAPI.
- `HelloGate` creates a TPM-backed CNG key for Windows Hello quick unlock.
- `AuthorizationProfile` persists Windows Hello-wrapped DEK information.
- The DAL applies Windows-specific file ACL hardening.

A macOS or Linux process cannot simply open the current DPAPI settings blob. A deliberate, versioned migration is required.

### Recommended security architecture

Introduce explicit platform contracts:

```text
IPlatformSecretStore
IPlatformQuickUnlock
IPlatformFileSecurity
IPlatformSessionMonitor
IPlatformClipboard
IPlatformSingleInstance
IPlatformNotificationService
IPlatformApplicationPaths
IPlatformUpdateInstaller
```

Implement them in dedicated projects:

```text
TOTP.Platform.Windows
TOTP.Platform.MacOS
TOTP.Platform.Linux
```

The intended security model should be:

- The master password remains the universal recovery and portability mechanism.
- The password-wrapped DEK remains platform-independent.
- Quick unlock remains optional and platform-local.
- Windows uses Hello/TPM.
- macOS uses Keychain and LocalAuthentication/Touch ID where available.
- Linux uses Secret Service/libsecret where available.
- If trustworthy platform secret storage is unavailable, require the master password.
- Never fall back to storing an unprotected quick-unlock key.
- Separate non-sensitive preferences from the sensitive authorization/key envelope.
- Preserve the existing account vault format unless a security review requires versioning it.

### Windows-to-portable migration flow

The migration should execute on Windows while DPAPI access is still available:

1. Decrypt the existing settings blob with DPAPI.
2. Require or verify the user's master password.
3. Confirm that a valid password recovery envelope exists.
4. Generate a versioned, portable authorization envelope.
5. Register the local Windows quick-unlock wrapper separately.
6. Reopen and validate the migrated vault.
7. Retain a rollback backup until validation succeeds.
8. Remove temporary plaintext buffers as soon as practical.
9. Never export or persist a plaintext DEK.

Existing users who only have a platform quick-unlock configuration must be required to establish a portable password recovery method before migration.

## Framework comparison

### Summary

| Framework | Windows | macOS | Linux | Mobile | Browser | WPF affinity | Fit for this repository |
|---|---:|---:|---:|---:|---:|---:|---|
| Avalonia UI | Yes | Yes | Yes | Yes | Yes | High | Best overall |
| Avalonia XPF | Yes | Yes | Yes | Limited/future | Limited/future | Very high | Fast commercial bridge |
| Uno Platform | Yes | Yes | Yes | Strong | Strong | Medium | Best if mobile/web becomes primary |
| .NET MAUI XAML | Yes | Yes | No official Linux | Strong | No | Medium-low | Not suitable for three desktop OSs |
| MAUI Blazor Hybrid | Yes | Yes | No official Linux | Strong | Shared Razor possible | None | Good web fit, wrong desktop coverage |
| Pure browser/PWA | Browser | Browser | Browser | Browser | Yes | None | Reject for primary vault |

## Option 1: native Avalonia UI

### Advantages

- Closest conceptual match to WPF.
- XAML, resources, converters, commands, bindings, styles, custom controls, and MVVM remain familiar.
- First-class Windows, macOS, and Linux desktop support.
- Supports desktop-native concepts such as tray icons.
- Retains the existing .NET service and DI architecture.
- Does not require an embedded browser runtime.
- NetSparkle already offers an Avalonia UI.
- NetSparkle supports Windows, macOS, and Linux update package types.

References:

- <https://docs.avaloniaui.net/controls/navigation/trayicon>
- <https://github.com/NetSparkleUpdater/NetSparkle>
- <https://docs.avaloniaui.net/docs/get-started/wpf/>

### Migration costs

- WPF XAML is similar but not source-compatible.
- `System.Windows.*` types become `Avalonia.*` types.
- WPF styling, triggers, dependency properties, markup extensions, and some template behavior need adaptation.
- Syncfusion WPF controls do not become native Avalonia controls automatically.
- `SfDataGrid`, behaviors, editing support, filtering, and grid adapters need replacement.
- Window ownership, dispatcher, bitmap, clipboard, and dialog types need conversion.
- View models must stop exposing WPF-specific types such as `BitmapImage`, `Window`, or `ICollectionView`.

### Recommendation within Avalonia

- Use native Avalonia for the final architecture.
- Create a new `TOTP.UI.Avalonia` project.
- Do not convert the WPF project in place.
- Keep `TOTP.UI.WPF` releasable until the Avalonia client reaches parity.

## Option 2: Avalonia XPF

Avalonia XPF is a commercial WPF compatibility product. It supports Windows, macOS, and Linux and provides a hybrid mode intended to support existing WPF control suites, including Syncfusion.

Reference:

- <https://v11.docs.avaloniaui.net/xpf/welcome/>

### Advantages

- Lowest initial UI migration effort.
- Potential reuse of existing Syncfusion controls and WPF XAML.
- Useful for quickly proving macOS and Linux execution.
- Could support staged conversion from WPF controls to native Avalonia controls.

### Disadvantages

- Commercial licensing and vendor dependency.
- WPF compatibility does not make DPAPI, Windows Hello, CNG, ACLs, or session events portable.
- Can conceal Windows-oriented assumptions instead of removing them.
- Less attractive if mobile becomes a primary target.
- Requires careful verification of accessibility, startup performance, control behavior, and Linux/macOS integration.

### Recommended use

Use XPF only as a time-boxed proof of concept or migration bridge. Do not make it the permanent architecture without a licensing and technical evaluation.

## Option 3: Uno Platform

Uno supports Windows, macOS, Linux, Android, iOS, and WebAssembly. Its Skia Desktop target provides a common desktop shell for Windows, Linux, and macOS.

References:

- <https://platform.uno/docs/articles/getting-started/requirements.html>
- <https://platform.uno/docs/articles/features/using-skia-desktop.html>
- <https://platform.uno/docs/articles/wpf-migration.html>

### Advantages

- Strong Microsoft and WinUI programming model.
- Good fit if mobile and WebAssembly are actual product targets.
- C#, XAML, MVVM, DI, and Microsoft tooling remain central.
- Integrated cross-platform packaging.
- Supports incremental WPF migration through Uno Islands.

Packaging reference:

- <https://platform.uno/docs/articles/uno-publishing-overview.html>

### Costs and risks

- The current app is WPF, not WinUI.
- WPF XAML requires conversion to the WinUI/Uno model.
- `System.Windows` becomes `Microsoft.UI.Xaml`.
- Styling, resources, events, binding behavior, navigation, and windowing differ.
- Supported framework API coverage varies by target.
- Third-party control compatibility requires a target-by-target audit.
- Uno `PasswordVault` is not implemented for Linux Skia or WebAssembly.
- A custom Linux secure-store implementation is still mandatory.

Credential storage reference:

- <https://platform.uno/docs/articles/features/PasswordVault.html>

### Recommended use

Choose Uno over Avalonia only if Android, iOS, or browser delivery is likely to become a first-class requirement in the next 12-24 months.

## Option 4: .NET MAUI XAML

### Advantages

- Microsoft-supported .NET application stack.
- Strong Android and iOS story.
- Built-in secure-storage abstraction on supported platforms.
- Familiar DI, XAML, binding, and MVVM concepts.
- Separate Syncfusion MAUI controls may help replace some current controls.

MAUI SecureStorage uses platform facilities such as Apple Keychain, Android KeyStore-backed encrypted preferences, and Windows DataProtectionProvider:

- <https://learn.microsoft.com/en-us/dotnet/maui/platform-integration/storage/secure-storage>

### Disadvantages

- No official Linux desktop target.
- Desktop windowing is not its primary strength.
- WPF XAML still requires a substantial rewrite.
- Current Syncfusion WPF APIs are not reusable as MAUI APIs.
- Single-instance behavior, updates, session locking, and advanced window interactions need platform-specific work.
- Mac Catalyst behavior is not identical to a conventional AppKit desktop application.

### Recommended use

Select MAUI only if Linux is removed from scope and Android/iOS become more important than a desktop-native experience.

## Option 5: MAUI Blazor Hybrid

Blazor Hybrid runs Razor components in the native .NET process and renders through an embedded WebView. It retains access to native .NET APIs and permits component sharing with browser applications.

Reference:

- <https://learn.microsoft.com/en-us/aspnet/core/blazor/hosting-models>

### Advantages

- Razor, CSS, and web component experience transfer directly.
- Strong productivity for a web-oriented team.
- Razor components can be shared with a future web client.
- Business logic executes in-process on .NET rather than WebAssembly.

### Costs

- All existing WPF XAML must be rewritten as Razor and CSS.
- Existing WPF styles, converters, behaviors, and controls offer almost no direct reuse.
- Inherits MAUI's lack of official Linux support.
- Adds WebView lifecycle, DOM, frontend asset, CSS, and content-security concerns.
- Grid-heavy UI likely needs a commercial Blazor component suite.
- Native dialogs, camera, clipboard, update installation, and window management still require platform abstractions.

### Recommended use

Consider this only for Windows/macOS/mobile when web-team productivity outweighs desktop coverage and native-XAML continuity.

## Option 6: pure browser or PWA

Reject a pure browser implementation as the primary authenticator.

A browser cannot provide an equivalent to DPAPI, Keychain, TPM-backed keys, or reliable Linux Secret Service integration for a fully offline vault. Persisting both encrypted vault data and its locally usable key in the same browser origin does not meet the intended security posture.

A browser companion could be considered later, but it requires a separate threat model and must not be treated as equivalent to the local desktop vault.

## Comparative effort estimates

The following estimates are person-weeks for one experienced senior engineer and assume:

- Windows, macOS, and Linux desktop parity where the framework supports it.
- Preservation of current user-visible functionality.
- Security regression testing.
- CI packaging and signing work.
- Normal discovery and technical uncertainty.
- No external penetration-test duration.
- No app-store review waiting time.

| Workstream | Avalonia | Avalonia XPF | Uno | MAUI XAML | MAUI Blazor |
|---|---:|---:|---:|---:|---:|
| Portable project/layer extraction | 3-5 | 3-5 | 3-5 | 3-5 | 3-5 |
| Settings/key-envelope migration | 6-10 | 6-10 | 6-10 | 6-10 | 6-10 |
| OS integration adapters | 5-8 | 5-8 | 6-10 | 5-9 | 5-9 |
| Main UI and style migration | 6-10 | 2-5 | 8-13 | 10-16 | 9-15 |
| Grid, behaviors, and custom controls | 4-7 | 1-3 | 5-8 | 5-9 | 4-7 |
| QR camera and image pipeline | 2-4 | 2-4 | 3-5 | 3-5 | 3-5 |
| Updater, packaging, and release CI | 5-8 | 5-8 | 6-10 | 6-10 | 6-10 |
| Testing, accessibility, and polish | 6-10 | 5-8 | 7-11 | 7-12 | 7-12 |
| Approximate production total | **37-62** | **29-51** | **44-72** | **45-76** | **43-73** |

Two engineers will not halve the calendar duration. With sensible division of platform and UI work, plan for approximately 60-70% of the single-engineer duration.

A functional but non-release-ready Avalonia vertical slice could reasonably take 6-10 weeks. A trustworthy production migration is longer because storage migration, recovery, packaging, signing, and update verification dominate the final stages.

## Detailed implementation workstreams

## 1. Define the support policy

- Decide whether cross-platform means desktop only or includes mobile/browser.
- Define minimum Windows, macOS, and Linux versions.
- Select supported Linux distributions and desktop environments.
- Decide whether macOS App Store sandboxing is required.
- Select Linux package formats.
- Define whether biometric quick unlock is required on each OS.
- Define behavior when an OS secret store is unavailable or locked.
- Document unsupported feature combinations.
- Define x64 and ARM64 support per platform.

Estimated effort: 1-2 weeks.

## 2. Restructure the solution

Recommended target structure:

```text
TOTP.Domain
TOTP.Application
TOTP.Cryptography
TOTP.Storage
TOTP.Platform.Abstractions
TOTP.Platform.Windows
TOTP.Platform.MacOS
TOTP.Platform.Linux
TOTP.Platform.Android
TOTP.Platform.iOS
TOTP.UI.WPF
TOTP.UI.Avalonia.Shared
TOTP.UI.Avalonia.Desktop
TOTP.UI.Avalonia.Mobile
TOTP.Updater.Core
TOTP.Tests.Portable
TOTP.Tests.Platform.Windows
TOTP.Tests.Platform.MacOS
TOTP.Tests.Platform.Linux
TOTP.Tests.Platform.Android
TOTP.Tests.Platform.iOS
```

The Android, iOS, mobile UI, and mobile test projects are introduced in the mobile phase. They are shown here so the desktop project boundaries do not preclude them.

Tasks:

- Retarget portable projects to plain `netX.0`.
- Move application-path constants out of Core.
- Remove platform-specific packages from Core.
- Split portable Infrastructure services from Windows implementations.
- Remove WPF types from service and view-model contracts.
- Split portable and platform integration tests.
- Run portable tests on every supported OS.
- Keep composition roots in the platform UI projects.

Estimated effort: 3-5 weeks.

## 3. Redesign settings and authorization persistence

- Separate non-sensitive preferences from sensitive authorization data.
- Define a versioned key-envelope schema.
- Preserve password-wrapped DEK compatibility.
- Define platform-local quick-unlock wrapper records.
- Implement DPAPI-to-portable migration.
- Add atomic migration and rollback.
- Add corruption and interrupted-write recovery.
- Add fixtures generated by previous production releases.
- Test password changes before and after migration.
- Test missing, reset, unavailable, and locked secure stores.
- Document threat impact, compatibility, and recovery behavior.

Estimated effort: 6-10 weeks.

## 4. Implement platform security providers

### Windows

- Preserve DPAPI migration support.
- Preserve Windows Hello/TPM quick unlock.
- Preserve Windows ACL hardening.
- Ensure existing users can upgrade without reimporting accounts.
- Validate application identity and key-name continuity.

### macOS

- Add Keychain-backed key storage.
- Add optional LocalAuthentication/Touch ID quick unlock.
- Configure entitlements.
- Apply appropriate file permissions.
- Handle Keychain reset and application identity changes.
- Sign and notarize release artifacts.

### Linux

- Add Secret Service/libsecret support.
- Detect missing, unavailable, or locked secret services.
- Fall back to master-password-only unlock.
- Apply Unix file modes and ownership checks.
- Test GNOME, KDE, and minimal desktop environments.
- Avoid assuming biometric support is present.

Estimated effort: 5-9 weeks.

## 5. Port the presentation layer

- Recreate application and desktop lifetimes.
- Port theme dictionaries and styles.
- Port all XAML views.
- Replace Syncfusion chromeless windows.
- Replace `SfDataGrid`.
- Reimplement sorting, filtering, editing, validation, and selection.
- Port flyouts, prompts, and settings tabs.
- Port the QR preview overlay.
- Port updater views.
- Replace WPF attached properties and behaviors.
- Replace WPF-specific localization markup.
- Replace `BitmapImage` and other WPF types in view-model contracts.
- Verify high-DPI and multi-monitor behavior.
- Verify keyboard-only navigation.
- Verify screen-reader and accessibility behavior.

Estimated Avalonia effort: 10-17 weeks including control replacement.

## 6. Port desktop integrations

- Clipboard copy and timed clearing.
- Idle and user-activity monitoring.
- Screen/session lock behavior.
- Lock-on-minimize behavior.
- Single-instance enforcement.
- Second-instance activation and argument forwarding.
- Native file and folder pickers.
- Opening export and log locations.
- Native notifications.
- Application shutdown and restart.
- Window ownership, positioning, centering, and focus.
- Global exception boundaries.
- Platform-specific application and log paths.

Estimated effort: 4-7 weeks.

## 7. Port QR scanning

- Verify OpenCvSharp native assets for every runtime identifier.
- Abstract camera enumeration and capture.
- Add macOS camera entitlements and permission handling.
- Handle Linux V4L2 and device permission differences.
- Replace WPF bitmap conversion.
- Test multiple cameras and hot-plug behavior.
- Test denied permissions and unavailable cameras.
- Preserve cancellation, timeout, and disposal behavior.
- Evaluate alternate decoding/camera libraries only if native OpenCV distribution becomes unreliable.

Estimated Avalonia desktop effort: 2-4 weeks.

## 8. Rework update installation

NetSparkle core can remain and provides an Avalonia UI option.

Tasks:

- Replace the WPF NetSparkle UI package.
- Preserve strict Ed25519 verification.
- Generate per-OS appcast items and artifacts.
- Preserve channel and version filtering.
- Windows: retain signed installer behavior.
- macOS: support signed and notarized DMG, PKG, or archive delivery.
- Linux: define DEB, RPM, AppImage, Snap, or tarball policy.
- Handle elevation and relaunch independently per OS.
- Disable in-app updating for store-distributed builds when required.
- Extend release scripts and CI matrices.
- Test interrupted downloads, invalid signatures, rollback, and failed installation.
- Keep signing credentials isolated by platform.

Estimated effort: 5-8 weeks.

## 9. Expand CI and release engineering

Add runners for:

- `windows-latest`
- `macos-latest`
- `ubuntu-latest`

Add pipelines for:

- Portable tests on every OS.
- Platform integration tests on the matching OS.
- x64 and ARM64 builds where supported.
- Native dependency validation.
- macOS signing and notarization.
- Linux package smoke tests.
- Install, update, relaunch, and uninstall verification.
- Artifact and appcast signature verification.
- Migration tests against prior production fixtures.
- Dependency and license review per target.

Estimated effort: 3-6 weeks, overlapping updater work.

## 10. Security verification

- Update the threat model for Keychain and Secret Service.
- Define quick-unlock guarantees per OS.
- Review sensitive buffer lifetime in the new UI toolkit.
- Verify clipboard-clearing limitations per OS.
- Document swap and core-dump exposure assumptions.
- Verify appcast signatures independently of package signatures.
- Test migration rollback and partial migration.
- Test OS-account changes and secure-store resets.
- Test copied vault files on a second machine.
- Test downgrade and replay attempts.
- Test malicious appcasts and substituted installers.
- Test logs for accidental secret disclosure.
- Obtain an external security review before declaring cross-platform release readiness.

Estimated internal effort: 3-5 weeks, excluding external assessment.

## Recommended phased migration

## Phase 0: architectural decision record

Create an ADR that records:

- Chosen framework.
- Supported platforms.
- Security storage design.
- Quick-unlock policy.
- Packaging formats.
- Migration and rollback requirements.
- Desktop delivery scope and the phased mobile roadmap.
- Conditions that must be met before starting Android and iOS work.
- Browser capabilities that remain deferred pending a dedicated threat model.

Exit criteria:

- Product and technical scope is agreed.
- No ambiguity remains about Linux or mobile support.

## Phase 1: portability foundation

Keep the WPF client shipping while extracting:

- Platform paths.
- Secret storage.
- File hardening.
- Session monitoring.
- Clipboard.
- Single-instance behavior.
- Update installation.
- Camera access.

Exit criteria:

- WPF consumes Windows implementations through platform contracts.
- Portable projects compile without Windows target frameworks.
- Portable tests run on Windows, macOS, and Linux.

## Phase 2: security migration proof

Implement and test the settings/key-envelope migration before broad UI work.

Using synthetic secrets:

1. Open a Windows-created vault.
2. Migrate its authorization envelope.
3. Open it on Windows after migration.
4. Open the migrated vault on macOS.
5. Open the same vault on Linux.
6. Confirm quick-unlock wrappers remain platform-local.
7. Confirm the master password works everywhere.
8. Confirm no plaintext secret is written during migration.
9. Confirm rollback works after simulated interruption.

Exit criteria:

- No user is at risk of being locked out by the UI migration.

## Phase 3: Avalonia vertical slice

Create `TOTP.UI.Avalonia.Shared` and `TOTP.UI.Avalonia.Desktop`, then implement:

1. Startup.
2. Password unlock.
3. Account list.
4. TOTP generation.
5. Copy with timed clearing.
6. Lock.
7. One settings page.
8. QR camera scan.

Run the slice on Windows, macOS, and Linux.

Exit criteria:

- Core workflows function on all three operating systems.
- Grid and camera technology choices are validated.
- Startup performance and memory usage are acceptable.
- No framework blocker remains.

## Phase 4: UI feature parity

Port:

- Account CRUD.
- Inline editing and validation.
- Search, filtering, and selection.
- Import/export.
- Password prompts.
- Settings tabs.
- Localization.
- QR generation and preview.
- Auto-update views.
- Notifications.
- Accessibility and keyboard navigation.

Exit criteria:

- Functional parity with WPF is documented and tested.

## Phase 5: platform release parity

- Produce signed Windows artifacts.
- Produce signed and notarized macOS artifacts.
- Produce selected Linux package formats.
- Generate compatible per-OS appcasts.
- Test update and rollback on clean machines.
- Document installation, recovery, migration, and uninstall behavior.

Exit criteria:

- Each supported OS has a reproducible release pipeline.

## Phase 6: cutover

Retire WPF only after:

- Feature parity is reached.
- Migration and rollback are tested.
- Signed updates work on all targets.
- Accessibility and keyboard navigation pass.
- Existing Windows users can upgrade without losing vault access.
- Security review findings are resolved.
- Recovery documentation is complete.
- At least one release candidate has completed cross-platform soak testing.

## Phase 7: Android and iOS

Begin mobile implementation only after the portable vault, authorization envelope, and recovery behavior have proven stable in desktop releases.

Tasks:

- Introduce `TOTP.Platform.Android` and `TOTP.Platform.iOS`.
- Introduce `TOTP.UI.Avalonia.Mobile`.
- Reuse shared application workflows and form-factor-neutral presentation state.
- Implement mobile navigation and touch-oriented account views.
- Implement Android KeyStore and iOS Keychain providers.
- Implement biometric quick unlock.
- Implement camera permissions and mobile-first QR scanning.
- Map desktop idle/session locking to mobile foreground/background lifecycle.
- Implement document-picker and share-sheet import/export.
- Replace desktop updater behavior with Play Store/App Store release behavior where appropriate.
- Add Android and iOS build, signing, UI, lifecycle, recovery, and migration tests.

Exit criteria:

- Mobile users retain master-password recovery.
- Biometric invalidation cannot strand the vault.
- Backgrounding reliably locks sensitive state according to policy.
- Import/export formats remain compatible with desktop.
- Store-ready signed builds are reproducible.

## Phase 8: optional web evaluation

Do not begin by porting the full vault UI. First produce a security and product ADR defining what a web client is permitted to do.

Evaluate:

- Whether the web client is temporary and memory-only.
- Whether it is a backup/QR utility rather than an authenticator.
- Whether end-to-end encrypted synchronization is a separate prerequisite.
- How browser compromise, XSS, extension access, origin storage, and session persistence affect the threat model.
- Which application workflows can be shared without moving long-lived secret keys into browser storage.

Exit criteria:

- The browser threat model is explicitly accepted.
- Web capabilities are narrower than desktop/mobile unless equivalent recovery and key-protection guarantees can be demonstrated.
- Web delivery cannot silently weaken the guarantees documented for native clients.

## Framework proof-of-concept plan

Before committing to the full migration, run a 2-4 week Avalonia proof of concept with no real secrets.

The proof must include:

- Dependency injection and application startup.
- Password unlock against a synthetic migrated envelope.
- Rendering and filtering at least 500 accounts.
- Inline edit validation.
- Clipboard copy and timed clearing.
- QR generation.
- Webcam QR scan.
- Native file picker.
- Single-instance activation.
- One NetSparkle update check using a test appcast.
- Windows, macOS, and Linux builds.
- At least one packaged artifact per OS.

Decision metrics:

- Startup duration.
- Working-set memory.
- Grid responsiveness.
- Camera reliability.
- DPI and font rendering.
- Accessibility behavior.
- Packaging complexity.
- Native integration gaps.
- Amount of WPF-specific code remaining in shared view models.

Optionally perform the same vertical slice with an Avalonia XPF trial to quantify whether its lower initial migration cost justifies the commercial dependency.

## Decision rule

Choose:

- **Avalonia UI** for a high-quality Windows/macOS/Linux authenticator.
- **Avalonia XPF** when commercial licensing is acceptable and minimizing the initial UI rewrite is the overriding concern.
- **Uno Platform** when mobile and WebAssembly are strategic first-class targets.
- **.NET MAUI XAML** when Linux is not required and mobile is more important than desktop.
- **MAUI Blazor Hybrid** when Linux is not required and Razor/CSS productivity is more important than native-XAML continuity.

For the current product direction, the selected path should be:

> Native Avalonia UI with explicit Windows, macOS, and Linux platform adapters, implemented alongside the existing WPF client until security, feature, and release parity are achieved.
