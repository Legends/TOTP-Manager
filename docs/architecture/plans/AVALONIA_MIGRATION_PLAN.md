# TOTP Manager Avalonia Migration Execution Plan

## Status

- **Decision:** Native Avalonia UI
- **Initial target platforms:** Windows, macOS, Linux desktop
- **Later targets:** Android and iOS
- **Possible future target:** Web, subject to a separate threat model
- **Migration style:** Incremental, side-by-side with the existing WPF client
- **Repository strategy:** One repository
- **Document scope:** Concrete execution plan rather than framework comparison

The broader framework analysis remains in [`CROSS_PLATFORM_MIGRATION_PLAN.md`](CROSS_PLATFORM_MIGRATION_PLAN.md). This document assumes the Avalonia decision is final and describes how to execute it safely.

## Executive decisions

1. Keep the migration in the existing `TOTP-Manager` repository.
2. Do not create a separate `totpmanager.avalonia` repository.
3. Keep the current WPF application buildable and releasable during migration.
4. Add the Avalonia client as a separate project beside WPF.
5. Extract portable and platform-specific services before moving most screens.
6. Complete the key-envelope and recovery migration before cross-platform release.
7. Use short-lived feature branches and incremental pull requests.
8. Avoid a single long-lived `avalonia-migration` branch where possible.
9. Create a `release/1.x` maintenance branch only when the stable WPF line must diverge from ongoing migration work.
10. Treat the first cross-platform release as a major product version, tentatively `v2.0.0`.

## Repository and branch strategy

## One repository, not two

The Avalonia client should remain in this repository because it shares:

- Domain models
- Cryptographic formats
- Authorization workflows
- Account workflows
- Storage formats
- Import/export formats
- Localization resources
- Tests and compatibility fixtures
- Release signing policy
- Security documentation
- Update feed conventions

A second repository would introduce unnecessary problems:

- Cross-repository changes for shared interfaces and implementations
- Version skew between WPF and Avalonia
- Duplicate security documentation and CI rules
- More difficult atomic changes to storage formats
- Duplicate issue tracking
- Harder regression testing against existing releases
- Greater risk that a security fix lands in only one client
- More complicated release and dependency management

A separate repository should be considered only if the Avalonia client becomes a genuinely independent product with different ownership, access controls, release cadence, or licensing. That is not the current direction.

## Recommended branch model

The repository uses `master` as the default branch, and both build and security CI target `master` and `release/1.x`. The latest production tag observed and reconciled locally is the annotated tag `v1.8.1.0`, which resolves to commit `13126f63aab8a7889e9c4f65cdfd7b0adca05f42`. The WPF maintenance baseline is therefore anchored to release history rather than the default version written in a project file.

### Before migration work begins

1. Finish or intentionally shelve the current uncommitted refactor.
2. Build and run the full test suite.
3. Identify the exact commit representing the latest production release.
4. Ensure that commit has an immutable release tag.
5. Create an architecture decision record for Avalonia.
6. Decide whether WPF maintenance releases will continue during migration.

### Preferred day-to-day workflow

Keep `master` releasable and integrate the migration incrementally:

```text
master
  ├─ feature/portable-paths
  ├─ feature/platform-secret-store
  ├─ feature/settings-envelope-v2
  ├─ feature/avalonia-shell
  ├─ feature/avalonia-unlock
  ├─ feature/avalonia-accounts
  └─ feature/avalonia-qr
```

Each feature branch should be short-lived and merged only when:

- WPF still builds.
- Existing Windows behavior remains intact.
- Relevant tests pass.
- New portable behavior has tests.
- No incomplete migration path is enabled for users.

This reduces drift and avoids repeatedly merging months of WPF changes into a long-running migration branch.

### When to create `release/1.x`

Create `release/1.x` from the exact stable WPF production commit when either condition becomes true:

- `master` begins accepting migration changes that make immediate WPF hotfix releases inconvenient.
- The WPF client enters maintenance mode while Avalonia becomes the main development line.

Recommended shape at that point:

```text
release/1.x  -> WPF maintenance, security fixes, critical regressions
master       -> shared foundation plus Avalonia migration
```

Rules:

- Security fixes land in the affected branch and are forward-ported to `master` immediately.
- Storage-format fixes must be evaluated in both directions.
- Do not add new product features only to `release/1.x`.
- Keep the existing build and security CI triggers for `release/1.x` before relying on it.
- Use explicit WPF maintenance tags and do not reuse tags.

### What not to do

Do not:

- Create `totpmanager.avalonia` as a second repository.
- Rewrite the current UI directly inside `TOTP.UI.WPF.csproj`.
- Make the Avalonia project the only startup project before it reaches parity.
- Let a migration branch remain unmerged for many months.
- Freeze all WPF security maintenance until the Avalonia port completes.
- Copy Core or Infrastructure source into the new UI project.
- Change the vault format merely to make UI migration easier.

## Versioning strategy

Recommended product-line convention:

- `1.x`: WPF Windows product line
- `2.0.0-rcN`: Avalonia cross-platform release candidates
- `2.0.0`: First supported Windows/macOS/Linux Avalonia release
- Later `2.x`: Android/iOS clients if storage and protocol compatibility remain intact

Historical WPF tags use four numeric components, ending with `v1.8.1.0`. The reviewed v2 workflow deliberately accepts only `v<major>.<minor>.<patch>` and `v<major>.<minor>.<patch>-rc<nr>`, stamps package/file versions from that tag, publishes RCs as prereleases, and never lets an RC replace the latest stable release. Existing v1 tags remain immutable historical identifiers and are not renamed.

## Migration principles

- Preserve the current encrypted account vault format unless a security review requires a change.
- Keep the master password as the universal recovery mechanism.
- Treat OS biometrics as optional quick unlock, never sole recovery.
- Keep platform APIs behind injected interfaces.
- Keep view models free of WPF and Avalonia types where practical.
- Prefer feature parity over pixel-perfect WPF reproduction.
- Preserve user data and rollback capability before optimizing visual polish.
- Do not weaken update signature verification.
- Do not allow incomplete platform security providers to fall back to plaintext storage.
- Keep synthetic historical-format fixtures as design and regression evidence.

## Target solution structure

Introduce projects incrementally. Do not perform a single bulk rename of the entire solution.

Target shape:

```text
TOTP.Core
  Domain models, errors, contracts, portable primitives

TOTP.Application
  Use cases, workflows, validation, orchestration

TOTP.Cryptography
  Vault encryption, KDF, password wrapping, format compatibility

TOTP.Storage
  Portable file formats and atomic persistence mechanics

TOTP.Platform.Abstractions
  OS service contracts

TOTP.Platform.Windows
  Hello/TPM, Windows ACLs, session events

TOTP.Platform.Unix
  Shared descriptor-based POSIX permission enforcement

TOTP.Platform.MacOS
  Keychain, LocalAuthentication, file permissions, lifecycle

TOTP.Platform.Linux
  Secret Service, Unix permissions, desktop lifecycle

TOTP.UI.WPF
  Existing Windows client retained during migration

TOTP.UI.Avalonia.Shared
  Shared Avalonia resources and reusable controls

TOTP.UI.Avalonia.Desktop
  Windows/macOS/Linux Avalonia application

TOTP.Updater.Core
  Framework-neutral update orchestration and verification

TOTP.Tests.Portable
TOTP.Tests.Windows
TOTP.Tests.MacOS
TOTP.Tests.Linux
```

Later mobile additions:

```text
TOTP.Platform.Android
TOTP.Platform.iOS
TOTP.UI.Avalonia.Mobile
TOTP.Tests.Android
TOTP.Tests.iOS
```

The final project names may be adjusted to match repository conventions. The important boundary is portable application logic versus OS adapters versus presentation.

## Dependency direction

The intended dependency flow is:

```text
UI.Avalonia.Desktop ─┐
UI.WPF              ─┼─> Application ─> Core
UI.Avalonia.Mobile  ─┘        │
                              ├─> Cryptography ─> Core
                              ├─> Storage ──────> Core
                              └─> Platform.Abstractions

Platform.Windows ─┐
Platform.MacOS   ─┼─> Platform.Abstractions + Core
Platform.Linux   ─┘
```

Prohibited dependencies:

- Core must not reference Avalonia, WPF, Win32, AppKit, Android, or iOS APIs.
- Application must not reference concrete platform projects.
- Platform projects must not own product policy.
- Portable view models must not accept `Window`, `BitmapImage`, `Dispatcher`, or framework controls.
- WPF and Avalonia UI projects must not directly read or write encrypted vault files.

## Milestone overview

| Milestone | Outcome | Estimate |
|---|---|---:|
| M0 | Stable baseline and migration governance | 1-2 weeks |
| M1 | Portable foundation and platform seams | 3-5 weeks |
| M2 | Portable settings/key-envelope migration | 6-10 weeks |
| M3 | Avalonia technical vertical slice | 2-4 weeks |
| M4 | Avalonia shell and design system | 3-5 weeks |
| M5 | Feature migration | 12-20 weeks |
| M6 | macOS/Linux platform completion | 5-9 weeks, partly parallel |
| M7 | Packaging, updates, and CI | 5-8 weeks |
| M8 | Hardening, accessibility, and security review | 6-10 weeks |
| M9 | Release candidate and cutover | 2-4 weeks |

Total expected production effort remains approximately **37-62 person-weeks**, with overlap between milestones.

## M0: stable baseline and governance

### Objectives

- Establish an immutable migration baseline.
- Ensure the current WPF product is recoverable and releasable.
- Record architectural and release decisions.

### Tasks

- [x] Finish, split, or shelve current uncommitted changes.
- [x] Run `dotnet restore TOTP.sln --configfile NuGet.config`.
- [x] Run a clean Debug build.
- [x] Run a clean Release build.
- [x] Run the complete test suite.
- [ ] Verify current WPF startup, unlock, account CRUD, QR, export, and update checks manually.
- [x] Identify the exact latest production commit and tag (`v1.8.1.0`, commit `13126f63aab8a7889e9c4f65cdfd7b0adca05f42`).
- [x] Reconcile the current project version, historical tags, and RC workflow.
- [x] Create the Avalonia decision record (`docs/architecture/ADR-0001-native-avalonia-migration.md`).
- [x] Record supported initial OS versions.
- [x] Record Linux distribution and packaging scope.
- [x] Create and wire CI for the `release/1.x` maintenance branch.
- [ ] Update branch protection for any maintenance branch.
- [ ] Add Avalonia migration labels and milestones to issue tracking.

### Exit criteria

- The baseline commit is immutable and reproducible.
- WPF can be released independently of migration work.
- Full tests pass.
- Framework, repository, branch, and platform decisions are documented.

## M1: portable foundation and platform seams

### Objectives

- Make the non-UI application genuinely portable.
- Keep existing WPF behavior unchanged through Windows adapters.

### Work packages

#### M1.1 Application paths

- [x] Introduce `IPlatformApplicationPaths`.
- [x] Move executable, settings, vault, backup, log, and update-state paths out of Core constants.
- [x] Implement Windows paths matching current behavior.
- [x] Define macOS Application Support and Logs locations.
- [x] Define Linux XDG config, data, cache, and state locations.
- [x] Add path migration and discovery tests.

#### M1.2 File security

- [x] Introduce `IPlatformFileSecurity`.
- [x] Move Windows ACL hardening out of portable DAL.
- [x] Implement Windows ACL behavior.
- [x] Define Unix permission behavior for macOS/Linux.
- [x] Fail safely when permissions cannot be hardened.
- [x] Add tests for error reporting and recovery.

#### M1.3 Clipboard

- [x] Keep the existing portable `IClipboardService` contract or refine it.
- [x] Separate clipboard policy from WPF clipboard access.
- [x] Preserve timed clearing and replacement detection.
- [x] Add platform capability/error semantics.

#### M1.4 Dispatcher and application lifetime

- [x] Replace WPF-specific dispatcher assumptions with a UI scheduler abstraction.
- [x] Keep `WpfDispatcherService` as the Windows implementation.
- [x] Remove direct `Application.Current` use from reusable workflows.
- [x] Keep process exit and shutdown behind application lifetime services.

#### M1.5 Window-independent user interaction

- [x] Keep confirmation, error, notification, and file-dialog contracts UI-neutral.
- [x] Remove `Window`, `MessageBox`, and WPF image types from contracts.
- [x] Convert window ownership into UI-layer concerns.

#### M1.6 Platform events

- [x] Introduce platform session/lifecycle contracts.
- [x] Preserve Windows session-lock monitoring.
- [x] Separate product lock policy from OS event delivery.
- [x] Define equivalent macOS/Linux lifecycle semantics.

#### M1.7 Single instance

- [x] Separate single-instance policy and activation messages from the current WPF implementation.
- [x] Preserve Windows behavior.
- [x] Define portable activation payloads.
- [x] Add stale-lock and crashed-instance recovery tests.

### Exit criteria

- Portable projects compile under a non-Windows target.
- WPF still uses the extracted contracts without user-visible regressions.
- Platform APIs are isolated to platform or UI projects.
- Portable tests run without loading WPF assemblies.

## M2: settings and key-envelope migration

### Objectives

- Make authorization and preferences portable without weakening recovery.
- Establish the clean portable format before the first public release; development-era formats have no compatibility commitment.

### Target data separation

```text
preferences.json / preferences.bin
  Non-secret user preferences where plaintext is acceptable by policy

authorization-envelope.bin
  Versioned password-wrapped DEK and platform wrapper metadata

vault.bin
  Existing AES-GCM encrypted account vault

platform secret store
  Platform-local quick-unlock key/handle
```

The exact division must be reviewed. Authorization data must not be moved into plaintext merely because it is separated from preferences.

### Tasks

- [x] Document the current DPAPI settings format.
- [x] Create synthetic fixtures for every supported historical format.
- [x] Define authorization envelope version 2.
- [x] Implement the v2 password wrapper and persist every Argon2 parameter.
- [x] Define platform quick-unlock wrapper metadata.
- [x] Introduce `IPlatformSecretStore`.
- [x] Introduce `IPlatformQuickUnlock`.
- [x] Add the strict portable v2 codec and inactive envelope store.
- [x] Add the strict portable preferences codec and inactive store.
- [x] Add explicit preference mapping that excludes authorization data.
- [x] Persist the preferred unlock method as a portable non-secret preference.
- [x] Decouple authorization state from the development-era profile shape.
- [x] Add the v2 envelope session and verified password-unlock path.
- [x] Replace the development-era DPAPI settings store before the first public release.
- [x] Preserve Windows Hello/TPM quick unlock through the new contract.
- [x] Require recovery-password readiness before platform quick unlock can be enabled.
- [x] Implement atomic write and rollback.
- [x] Add side-effect-free candidate vault-key verification.
- [x] Add bounded read-only verification of the existing vault.
- [x] Verify the v2 envelope can open the vault before it becomes active.
- [x] Preserve a bounded previous-envelope backup.
- [x] Add interrupted-write tests at every persistence boundary.
- [x] Add wrong-password, corrupt-envelope, missing-secret-store, and reset-key tests.
- [x] Zero temporary key buffers where practical.
- [x] Update the threat model and security verification documentation.

### Activation state machine

```text
NotStarted
  -> PasswordConfigured
  -> PortableEnvelopeWritten
  -> PortableEnvelopeVerified
  -> PlatformQuickUnlockRegistered
  -> Active

Any replacement failure before Active
  -> RollbackAvailable
```

### Exit criteria

- A fresh configuration writes and reopens a portable v2 envelope.
- Master-password recovery works independently of platform quick unlock.
- Interrupted writes preserve the previous verified envelope.
- No plaintext DEK or seed is persisted.
- The new envelope can be opened by platform-neutral test code.
- Security review approves the format and state machine.

## M3: Avalonia technical vertical slice

### Objectives

- Validate Avalonia, the replacement grid, camera pipeline, platform packaging, and MVVM reuse before broad UI investment.

### Project creation

- [x] Create `TOTP.UI.Avalonia.Shared`.
- [x] Create `TOTP.UI.Avalonia.Desktop`.
- [x] Add projects to `TOTP.sln`.
- [x] Register the same application services through an Avalonia composition root.
- [x] Keep WPF as the release/default client.
- [x] Add the Avalonia desktop host to cross-platform CI while keeping it out of release publishing.

### Vertical slice

- [x] Application startup and error boundary.
- [x] Password unlock against synthetic data.
- [x] Account list with at least 500 generated entries.
- [x] Search and filtering.
- [x] TOTP generation.
- [x] Copy with timed clipboard clearing.
- [x] Manual lock.
- [x] One settings page.
- [x] QR generation.
- [ ] Camera QR scan. Portable implementation and Avalonia integration are complete; packaged runtime probes and real-device target evidence remain required.
- [x] Native file picker.
- [x] Single-instance activation.
- [x] Test update check using a signed test appcast.

### Required target validation

- [ ] Windows x64
- [ ] macOS ARM64
- [x] macOS x64 evaluated and excluded from the initial product policy after the aligned native camera runtime failed its M3 Intel package probe.
- [ ] Linux x64 on the selected baseline distribution

Automated publish/native-runtime probes cover the three supported targets. The supported-target checkboxes require the real-target record in `docs/architecture/M3_TARGET_VALIDATION.md` and are not satisfied by hosted compilation alone.

### Measurements

- [x] Packaged technical-probe startup duration
- [x] Technical-probe working-set memory
- [x] Synthetic account-filtering latency at 500, 1,000, and 5,000 entries
- [ ] Interactive account-list rendering latency
- [ ] Physical-camera startup and disposal reliability
- [ ] High-DPI rendering at 100%, 150%, and 200%
- [ ] Keyboard navigation on each supported target
- [ ] Screen-reader behavior on each supported target
- [x] Package size
- [x] Native dependency footprint
- [x] Number of WPF types leaking into portable/shared code: zero

Automated budgets and evidence format are defined in `docs/architecture/M3_MEASUREMENT_BUDGETS.md`. Interactive and hardware-dependent rows remain open until recorded on the packaged supported targets.

The requirement-by-requirement status and exact closure procedure are maintained in `docs/architecture/M3_COMPLETION_AUDIT.md`.

### Decision gate

Do not proceed to full UI migration until:

- The chosen Avalonia grid/control strategy is acceptable.
- QR scanning works on all three desktop OSs.
- Packaging is feasible.
- No security-storage blocker remains.
- Accessibility has a viable implementation path.
- Startup and memory behavior are acceptable.

## M4: Avalonia shell and design system

### Application shell

- [x] Avalonia application lifetime and startup orchestration.
- [x] DI composition root and redacted logging initialization. A generic host remains optional rather than a feature-level dependency.
- [x] Global exception handling.
- [x] Main window lifecycle.
- [x] Splash/startup decision: not currently justified; use measured shell status and revisit only if interactive startup regresses.
- [x] Authorization-gated shell navigation and lock gate.
- [x] Settings navigation.
- [x] Dialog ownership and activation.
- [x] Localization startup and live language switching; session-only until the M5 settings schema review.

### Design system

- [x] Define colors, typography, spacing, radii, elevation, and motion tokens.
- [x] Port the application icon and reusable vector symbol set; feature-specific assets remain with M5 screens.
- [x] Port reusable button, input, validation, notification, and dialog styles.
- [x] Preserve the established WPF dark visual identity by default while retaining automatic high-contrast activation.
- [x] Create initial desktop keyboard/focus standards.
- [x] Define and automatically activate high-contrast behavior; physical acceptance remains a target-validation gate.
- [x] Define scale and DPI test cases; execution remains a target-validation gate.

### Shared Avalonia controls

- [x] Revealable secret input.
- [x] Busy overlay.
- [x] Account row/cell templates.
- [x] QR preview.
- [x] Validation presentation.
- [x] Confirmation/password dialogs.
- [x] Notification presentation.

### Exit criteria

- New screens can be added without inventing new styling conventions.
- Shell behavior works on all desktop platforms.
- Localization and accessibility foundations are operational.

## M5: feature migration

Migrate feature-by-feature. Each feature should include UI, view-model cleanup, platform handling, and tests in the same work package.

## M5.1 Authorization and recovery

- [x] First-run setup.
- [x] Password setup.
- [x] Password unlock.
- [x] Platform quick-unlock availability.
- [x] Quick-unlock enrollment.
- [x] Quick-unlock failure fallback.
- [x] Password change.
- [x] Lock and reauthorization.
- [x] Recovery messaging.
- [x] Migration messaging and rollback UI (automatic rollback status; no unsafe manual file restore).

Definition of done:

- Password recovery works on every OS.
- Quick unlock is optional.
- Quick-unlock invalidation never strands the vault.
- Failure paths have regression tests.

## M5.2 Account list and CRUD

- [x] Replace Syncfusion `SfDataGrid`.
- [x] Account list virtualization.
- [x] Search and filtering.
- [x] Sorting.
- [x] Selection.
- [x] Add account.
- [x] Edit account.
- [x] Delete confirmation.
- [x] Duplicate validation.
- [x] Use an explicit secret-clearing editor instead of retaining inline grid editing.
- [x] Keyboard navigation.
- [x] Context menu behavior.
- [x] Large-vault performance tests.

The WPF grid should not dictate the Avalonia interaction model. Preserve capability and efficiency, not necessarily identical control behavior.

## M5.3 TOTP and clipboard

- [x] TOTP timer scheduling.
- [x] Remaining-time progress display.
- [x] Code copy.
- [x] Timed clipboard clear.
- [x] Replacement detection where supported.
- [x] Lock-time output clearing.
- [x] Screen-reader announcement policy.

## M5.4 QR workflows

- [x] QR generation.
- [x] QR preview overlay.
- [x] Camera discovery.
- [x] Camera permission UX.
- [x] QR decoding.
- [x] Duplicate/update/keep-both workflow.
- [x] Cancellation and disposal.
- [x] Camera-unavailable recovery.

## M5.5 Settings

- [x] General preferences.
- [x] Security settings.
- [x] Lock policies.
- [x] Clipboard policies.
- [x] QR preview settings.
- [x] Logging level.
- [x] About/version information.
- [x] Update checks.
- [x] Log-folder access.
- [x] Live localization.

## M5.6 Import, export, and backup

- [x] Native open/save dialogs.
- [x] Encrypted export.
- [x] Export password prompt.
- [x] Import conflict handling.
- [x] Backup creation.
- [x] Backup discovery and recovery.
- [x] Open-export-location behavior.
- [x] File permission handling.
- [x] Cross-platform path and filename tests.
- [x] Compatibility tests using WPF-generated exports.

## M5.7 Notifications and diagnostics

- [x] Success/error/warning notifications.
- [x] Recoverable error dialogs.
- [x] Cross-platform log paths.
- [x] Log redaction verification.
- [x] Startup diagnostics.
- [x] Platform information in support output without exposing secrets.

## M5.8 Auto-update UI

- [x] Port update checking UI.
- [x] Port release notes display.
- [x] Port download progress.
- [x] Port install-ready and failure states.
- [x] Preserve strict signature verification.
- [x] Separate UI from platform installer execution.

### M5 exit criteria

- A feature-parity matrix contains no unapproved gaps.
- WPF and Avalonia read the same compatible vault/export formats.
- Relevant automated tests exist for each migrated workflow.
- All expected failure branches are user-recoverable.

## M6: complete desktop platform adapters

## Windows

- [x] Portable v2 envelope storage.
- [x] First-run/reset handling for development-era local data.
- [x] Hello/TPM quick unlock.
- [x] Windows ACL hardening.
- [x] Session-lock event handling.
- [x] Single-instance activation.
- [x] Clipboard behavior.
- [x] Native dialogs and shell integration.
- [x] Signed installer/update execution.

## macOS

- [x] Application Support, Cache, and Logs paths.
- [x] Keychain secret storage.
- [x] LocalAuthentication quick unlock.
- [x] File permission hardening.
- [x] Camera entitlement and permission handling.
- [x] App activation and single-instance behavior.
- [x] Clipboard handling.
- [x] App bundle identity.
- [x] Signing and notarization automation. Credentialed release acceptance remains an M7/M8 gate.
- [x] DMG distribution policy. PKG is intentionally not selected for the initial user-local app.

## Linux

- [x] XDG paths.
- [x] Secret Service/libsecret integration.
- [x] Master-password-only fallback.
- [x] Unix file permissions.
- [x] Desktop session-lock detection for supported GNOME/KDE-family environments.
- [ ] X11/Wayland behavior validation.
- [x] Camera/V4L2 permission handling.
- [x] Single-instance activation.
- [ ] Clipboard behavior across selected desktops.
- [x] Portable tarball and Ubuntu/Debian DEB implementation.

## Platform capability policy

Every optional capability must expose:

- Supported
- Temporarily unavailable
- Permanently unavailable
- Permission denied
- Misconfigured
- Failed

The UI must not treat every failure as equivalent to “not supported.”

### Exit criteria

- Each platform passes its integration-test suite.
- Security capabilities fail closed.
- Unsupported features have documented safe fallbacks.

## M7: packaging, updates, and CI

## CI matrix

- [x] Windows build and portable tests.
- [x] macOS build and portable tests.
- [x] Linux build and portable tests.
- [x] Platform integration tests on matching runners.
- [x] Avalonia UI smoke tests load the real application XAML on Windows, macOS, and Linux runners.
- [x] Native dependency validation.
- [x] Artifact manifest generation.
- [x] SBOM/dependency audit where appropriate. A blocking structured NuGet vulnerability audit is retained; formal SBOM publication remains a release-pipeline follow-up if required by the distribution channel.

## Windows artifacts

- [x] Fast framework-dependent ZIP artifact retained as the non-privileged initial-download option; the self-contained ZIP remains the update payload.
- [x] Signed self-contained ZIP artifact retained for the Avalonia direct/update path.
- [x] Code signing is mandatory in the Avalonia tag job.
- [x] Appcast generation.
- [x] Transactional file-upgrade and failure rollback regression tests; physical signed install/relaunch remains an acceptance gate.

## macOS artifacts

- [x] `.app` bundle.
- [x] DMG selected and structurally validated on macOS CI.
- [x] Application signing automation.
- [x] Notarization automation.
- [x] Stapling automation.
- [x] Update artifact and appcast metadata.
- [ ] Clean-machine install/update test.

## Linux artifacts

Select a narrow initial set, for example:

- [x] Tarball selected for broad direct distribution; AppImage is deferred with rationale.
- [x] DEB for Debian/Ubuntu users.
- [x] RPM and Snap deferred until demand and maintained release policy justify them.
- [x] Desktop file and target-appropriate application icons are packaged.
- [x] Package permissions.
- [x] Update behavior compatible with package ownership.

## Update feed

- [x] Per-OS and per-architecture asset selection.
- [x] Strict Ed25519 verification.
- [x] Version and channel compatibility.
- [x] Store-distributed build policy.
- [x] Invalid-signature regression tests.
- [x] Wrong-platform artifact rejection.
- [x] Interrupted-download recovery.

### Exit criteria

- Every supported platform has a reproducible signed release path.
- Appcasts cannot select the wrong OS or architecture artifact.
- Installation and update tests pass on clean environments.

## M8: hardening and release readiness

### Security

- [x] Update threat model.
- [x] Review all quick-unlock providers; native prompts/reset remain physical gates.
- [x] Review sensitive buffer lifetime; managed string-binding limitations are documented for external memory review.
- [x] Review clipboard limitations; desktop clipboard-manager behavior remains a physical gate.
- [x] Review logs for secret leakage; exact-artifact sanitized log review remains physical acceptance.
- [x] Test storage corruption and rollback.
- [x] Test secure-store reset and OS-account changes through fail-closed adapters; destructive native-account reset remains physical acceptance.
- [x] Test downgrade and replay behavior.
- [x] Test malicious appcasts and substituted installers.
- [ ] Commission external review or penetration test.

### Reliability

- [x] Startup failure recovery.
- [x] Crash recovery.
- [x] Camera failure recovery.
- [x] Missing secure-store recovery.
- [x] Update failure recovery.
- [x] Backup restore verification.
- [ ] Long-running timer and idle-lock soak tests.

### Accessibility and UX

- [ ] Keyboard-only operation.
- [ ] Screen-reader labels and announcements.
- [ ] Focus order.
- [ ] High contrast.
- [ ] DPI scaling.
- [x] Reduced motion: no animation or transition is currently applied.
- [ ] Localization overflow.
- [x] Right-to-left localization is deferred until an RTL locale enters scope; no physical readiness claim is made.

### Performance

- [ ] Cold startup budget.
- [ ] Warm startup budget.
- [x] 500/1,000/5,000-account list tests, plus a 10,000-account projection regression.
- [x] Search latency budget in all three retained M3 package-probe reports.
- [ ] Memory budget.
- [ ] Camera startup budget.
- [ ] Lock/unlock latency budget.

### Exit criteria

- No unresolved critical/high security findings.
- Performance budgets are met.
- Accessibility acceptance criteria pass.
- Recovery documentation matches tested behavior.

## M9: release candidate and cutover

### Release candidate

- [ ] Publish `v2.0.0-rc1` for all supported desktop OSs.
- [ ] Use separate test update feeds if required.
- [ ] Recruit first-release testers on every supported platform.
- [ ] Collect only redacted diagnostics.
- [ ] Run at least one extended soak period.
- [ ] Fix blockers and publish subsequent RCs.

### Cutover conditions

- [ ] Feature parity is approved.
- [ ] Fresh and reset vault setup succeeds on every supported platform.
- [ ] Rollback succeeds.
- [ ] Master-password recovery works on all platforms.
- [ ] Quick-unlock failure cannot strand the vault.
- [ ] Signed updates work on all platforms.
- [ ] Accessibility criteria pass.
- [ ] Security review findings are resolved.
- [x] Support and recovery documentation is published; exact-artifact physical acceptance records remain required.

### WPF retirement

Completed on 2026-08-28 before `v2.0.0-rc6`:

- [x] Retire the unpublished WPF implementation and release job.
- [x] Preserve historical tags and synthetic fixtures as regression and design evidence.
- [x] Do not ship development-era legacy readers in the first public release.

## Testing strategy

## Portable tests

Run on Windows, macOS, and Linux:

- Domain validation
- TOTP generation
- OTP URI parsing
- Vault encryption/decryption
- Password wrapping
- Import/export formats
- Account workflows
- Authorization state transitions
- Settings migration state machine
- Update metadata validation

## Platform tests

Run only on the matching OS:

- Secret-store operations
- Quick unlock
- File permissions
- Application paths
- Session/lifecycle events
- Clipboard behavior
- Single-instance activation
- Camera permissions and capture
- Package/update execution

## Compatibility fixture matrix

Maintain synthetic fixtures for:

- Every supported legacy settings format
- Current WPF vault format
- Current export formats
- Corrupt and truncated files
- Wrong-password envelopes
- Missing platform wrapper
- Invalid update signatures
- Old and future-unknown schema versions

Fixtures must never contain real user secrets.

## UI testing

- View-model behavior tests remain framework-neutral where possible.
- Add Avalonia view smoke tests for bindings and resource loading.
- Add manual platform checklists for native dialogs and permissions.
- Add accessibility checks.
- Add golden-image tests only where they produce stable value.
- Avoid tests that assert implementation details of the Avalonia visual tree without behavioral value.

## Feature-parity checklist

The following remains the cross-platform release-readiness checklist after the WPF retirement decision; incomplete target evidence blocks a stable release, not repository cleanup or an explicitly unsigned RC:

| Capability | Windows | macOS | Linux |
|---|---:|---:|---:|
| First-run password setup | [ ] | [ ] | [ ] |
| Master-password unlock | [ ] | [ ] | [ ] |
| Platform quick unlock | [ ] | [ ] | [ ]/N/A |
| Portable envelope storage | [ ] | [ ] | [ ] |
| Account CRUD | [ ] | [ ] | [ ] |
| Search/filter/sort | [ ] | [ ] | [ ] |
| TOTP display/copy | [ ] | [ ] | [ ] |
| Clipboard timed clear | [ ] | [ ] | [ ] |
| QR generation | [ ] | [ ] | [ ] |
| Camera QR scan | [ ] | [ ] | [ ] |
| Import/export | [ ] | [ ] | [ ] |
| Backup/restore | [ ] | [ ] | [ ] |
| Idle/lifecycle locking | [ ] | [ ] | [ ] |
| Single instance | [ ] | [ ] | [ ] |
| Localization | [ ] | [ ] | [ ] |
| Logging/redaction | [ ] | [ ] | [ ] |
| Update check | [ ] | [ ] | [ ] |
| Signed installation/update | [ ] | [ ] | [ ] |
| Accessibility acceptance | [ ] | [ ] | [ ] |

## Definition of done for migration pull requests

Every migration PR must:

- State which milestone and work package it advances.
- Describe platform and security impact.
- Preserve WPF behavior unless an intentional change is documented.
- Add or update relevant tests.
- Avoid introducing plaintext secret persistence.
- Avoid logging sensitive material.
- Compile affected portable projects on non-Windows targets where applicable.
- Pass Windows CI.
- Pass additional platform CI once those jobs exist.
- Update this plan's checklist when a work item is completed.
- Document unverified platform behavior.

Security-sensitive PRs must additionally document:

- Threat impact
- Data-flow impact
- Compatibility impact
- Migration impact
- Recovery impact
- Test evidence

## Risk register

| Risk | Impact | Mitigation |
|---|---|---|
| Development-only DPAPI storage reaches first release | Non-portable authorization and future lock-in | Replace it with v2 before public release; do not add a shipping legacy reader |
| Quick unlock becomes sole recovery path | Permanent vault loss | Require master-password recovery on every OS |
| Syncfusion grid has no equivalent behavior | UX/performance regression | Validate replacement in M3 before full port |
| Linux secret store unavailable | Unsafe key fallback | Require master password; never persist plaintext key |
| Camera native assets fail on one target | QR workflow unavailable | Validate OpenCV/native dependencies in vertical slice |
| Long-running branch diverges from WPF | Merge delays and regressions | Use incremental short-lived PRs |
| Separate repositories drift | Security/version mismatch | Keep one repository and atomic changes |
| macOS signing/notarization is deferred | Release blocked late | Build signed test artifact during M3/M7 |
| Update feed selects wrong artifact | Failed or unsafe update | Add OS/architecture filtering and verification tests |
| View models retain WPF types | Duplicated UI logic | Enforce portable presentation contracts before screen port |
| Browser goal weakens native vault security | Product trust regression | Require separate web threat model and capability limits |
| Mobile layouts reuse desktop assumptions | Poor mobile UX | Share workflows, not all views; create separate mobile presentation |

## Suggested first migration issues

Create these as separate, reviewable issues or pull requests:

1. **ADR: Select native Avalonia and one-repository migration strategy**
2. **Establish immutable WPF baseline and reconcile release versioning**
3. **Add portable application-path abstraction**
4. **Extract platform file-security abstraction**
5. **Retarget portable tests away from Windows where possible**
6. **Document current DPAPI and authorization-envelope formats**
7. **Design and review portable authorization envelope v2**
8. **Implement portable v2 envelope storage with atomic-write and rollback tests**
9. **Create Avalonia Shared/Desktop project skeletons**
10. **Implement Avalonia password-unlock vertical slice**
11. **Evaluate and benchmark Avalonia account-list/grid options**
12. **Validate QR camera scanning on Windows/macOS/Linux**
13. **Produce unsigned test packages for all desktop targets**
14. **Implement macOS Keychain provider**
15. **Implement Linux Secret Service provider and password-only fallback**

## Immediate next action

Do not create another repository or begin rewriting screens yet.

The next action should be M0:

1. Stabilize and commit the current refactor.
2. Identify the exact current production commit and tag.
3. Decide whether WPF needs an immediate `release/1.x` branch.
4. Create the Avalonia ADR.
5. Start the portable application-path and security-envelope design work through small PRs.

This preserves a releaseable Windows product while creating a migration path that can be reviewed, tested, and rolled back at every stage.
