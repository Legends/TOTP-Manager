# Security Verification Evidence

## Goal
Provide auditable evidence that security controls are continuously validated.

## Automated Evidence (GitHub Actions)
Workflow: `.github/workflows/security-audit.yml`

- Workflow linting:
  - Tool: `actionlint`
  - Evidence: `Workflow Lint (actionlint)` job result
- SAST:
  - Tool: CodeQL (C#)
  - Evidence: Code scanning alerts in GitHub Security tab
- SCA:
  - Tool: `dotnet list package --vulnerable --include-transitive`
  - Evidence artifacts:
    - `sca-evidence/dependency-vulnerabilities.json`
    - `sca-evidence/dependency-deprecated.json`
- Secret scanning:
  - Tool: Gitleaks
  - Evidence: workflow run status and findings in logs
- Security regression tests:
  - Tool: targeted `dotnet test` filter in `build-and-test.yml`
  - Evidence: workflow step `Run security regression tests`
- DAST (optional, manual trigger):
  - Tool: OWASP ZAP baseline
  - Input: `dast_target_url` in workflow dispatch
  - Evidence artifacts:
    - `dast-zap-evidence/zap-report.html`
    - `dast-zap-evidence/zap-report.json`

## Required Release Gates
- `build-and-test` workflow passes
- `security-audit` workflow passes
- No High/Critical open CodeQL alerts for touched code
- No known High/Critical vulnerable dependencies without approved exception
- Stable Windows releases require `SIGNING_CERT_BASE64` and `SIGNING_CERT_PASSWORD` and are Authenticode-signed in CI using an ephemeral certificate file.
- Until platform signing credentials are funded, RC tags may publish only explicitly labeled unsigned Windows/Linux development previews. Their package configuration disables automatic updates, their manifest permits only manual preview downloads, and the workflow excludes appcasts, macOS, and legacy WPF assets.

## Portable Authorization Cutover Evidence

- Runtime composition resolves `PortableSettingsService`, `PortableAuthorizationService`, the v2 password lifecycle, and the Windows quick-unlock adapter as one graph.
- The development-era DPAPI settings DAL is absent from the runtime service graph; it remains only in historical-format tests and documentation.
- Strict codec, store, activation, password lifecycle, session, quick-unlock enrollment, facade, and WPF prompt regressions cover malformed inputs, wrong credentials, missing platform state, atomic replacement, rollback, vault verification, cancellation, and temporary-key clearing.
- A supported-hardware Windows Hello/TPM registration and unlock smoke test remains a manual pre-release gate.

## Avalonia Presentation Boundary Evidence

- Password unlock delegates to the portable authorization facade, removes the bound password before awaiting verification, and never echoes credential or exception text.
- The account-list projection retains only account ID, issuer, and account name. OTP seeds remain outside the Avalonia row model and are never required for list rendering.
- Synthetic 500-account coverage verifies the list capacity and the absence of a `Secret` member on the presentation type. Search coverage verifies that filtering is limited to issuer and account name. Failure tests verify that account loading is recoverable and does not expose underlying exception text.
- `IAccountTotpService` resolves the selected account and invokes the existing zeroing `ITotpGenerator` inside Infrastructure. Avalonia supplies only the non-secret account ID and never receives the seed. Generated codes are never logged and are removed from presentation state when their current time step expires, selection changes, or the view model is disposed. Managed code strings cannot be deterministically zeroed, so their lifetime is minimized rather than claimed to be zeroized.
- TOTP tests verify account-ID selection, seed confinement to the infrastructure service, invalid-seed redaction, generic UI failures, and expiring-code projection.
- Manual lock delegates to `IAuthorizationService.Lock`, clears generated codes, selection, search text, and projected account rows, then returns to the configured quick-unlock or password gate. Quick-unlock cancellation remains locked, while the explicit master-password fallback changes only the current presentation and does not silently rewrite the persisted unlock preference. Regression coverage verifies password and quick-unlock gate projection, successful reauthorization, cancellation, preference preservation, authorization-state locking, and presentation-state cleanup.
- Avalonia settings are grouped into Security, Import/Export, Miscellaneous, and About tabs. Valid general-preference edits are coalesced through the same 200 ms auto-save behavior as WPF and persist through `ISettingsService`; authorization changes continue through `IAuthorizationService` and its recovery-password confirmation dialogs. No new authorization metadata or secret material is bound or serialized by the tab layout. Loading does not trigger persistence, and failed or exceptional preference saves restore the prior active values and expose only generic UI text; tests cover coalescing, successful persistence, and rollback.
- Avalonia clipboard writes use a separate asynchronous contract and a receipt tied to the exact in-process `DataTransfer` instance. Timed clearing occurs only while that object remains the current clipboard owner; a user or another application changing the clipboard prevents clearing. The adapter releases its ownership reference after clear, ownership loss, replacement, or disposal and never logs code text.
- Conditional ownership is enabled only on Windows, macOS, and X11, matching Avalonia's documented in-process ownership support. Wayland and unknown Linux sessions advertise write-only capability, so the security-sensitive copy action fails before writing rather than copying a code it cannot safely clear. Adapter and scheduler tests cover exact-receipt clearing, changed-clipboard preservation, unsupported-capability fail-closed behavior, and UI duration forwarding.
- QR generation follows an account-ID boundary parallel to TOTP generation. Infrastructure alone resolves the account seed and constructs the `otpauth` URI, returns PNG bytes in an owned `SensitiveBuffer`, and zeroes its temporary PNG array. Avalonia zeroes its decoding copy, displays the bitmap directly in a temporary owned preview dialog, and disposes it as soon as that dialog closes. The dialog creates no additional secret-bearing copy and closes on Escape, selection change, lock, or view-model disposal. Neither seed, URI, PNG, nor exception text is logged. Tests cover seed confinement, temporary-buffer clearing, generic failures, direct preview routing and cleanup, Escape closure, and lock cleanup; no storage or export format changes are involved.
- The M3 native file-picker probe uses Avalonia's platform storage provider with an allowlisted `*.totp`/`*.json` filter and single-selection mode. It retains and displays only the selected file name, does not open or import the file, does not log a path, and labels import as disabled in the preview. Stream access, schema validation, authorization, and atomic import remain a later workflow and are not implied by this picker validation.
- Avalonia single-instance coordination uses a per-user hashed mutex name and a `PipeOptions.CurrentUserOnly` named pipe. The protocol accepts only the fixed two-byte activation version/kind pair, rejects unsupported values, carries no paths or secrets, and can only request main-window activation; it never unlocks the vault. A secondary instance exits after a successful redirect and fails closed with a non-zero exit code when the primary cannot be reached. Windows unit tests and real Ubuntu/macOS CI transport tests cover mutual exclusion and activation round trips.
- Camera capture is isolated in `TOTP.Camera.OpenCv`; neither WPF nor Avalonia references OpenCV native types. Expected permission, unavailable-device, device-loss, stalled-frame, and native-runtime failures use typed results instead of exception-message parsing. Avalonia requests capture only after an explicit scan action; the toolbar action opens an owned scanner dialog and starts capture directly. The dialog clears and cancels capture on cancel, close, session lock, or disposal, zeroes each encoded preview buffer after decoding, disposes replaced previews, and validates decoded data without logging or displaying the seed. Successful imports return only the non-secret account ID, status, and localized result to the account page so the refreshed row can be selected and revealed; no seed or QR payload crosses that presentation event. Nested conflict prompts remain owned by the active scanner window. The OpenCvSharp managed and native packages are version-aligned at 4.13.0.20260627. Deterministic tests cover failure mapping, cancellation, device loss, stalls, capture disposal, direct dialog routing, nested ownership, UI lifecycle cleanup, safe metadata projection, imported-row reveal, and oversized/invalid payload rejection. Physical permission prompts, hot-plug behavior, and repeated real-device capture remain target validation gates.
- The Avalonia M3 update probe verifies an embedded, non-production appcast with Ed25519 before parsing it, enforces a 256 KiB document limit, prohibits DTD resolution, caps item count, requires HTTPS artifact URIs, and selects only newer OS/architecture-compatible entries. Its synthetic public key cannot authorize production updates, and the probe never downloads or installs the example artifact. Regression coverage verifies the fixture with both the portable verifier and NetSparkle's strict `Ed25519Checker`, and covers tampering, malformed signatures, version filtering, and oversized input.
- The Windows Avalonia installer adapter repeats Ed25519 verification while holding the downloaded ZIP without write/delete sharing, rejects non-ZIP packages and reparse points in its bundled helper, uses structured process arguments, and keeps the application running unless the visible updater process signals readiness. The production Avalonia feed remains disabled pending target-qualified release artifacts and physical install/relaunch acceptance.
- The macOS quick-unlock adapter stores the 32-byte vault key only in a data-protection Keychain item guarded by `userPresence`; LocalAuthentication availability uses the device-owner policy. The envelope contains only an opaque item reference and fixed-size SHA-256 binding. Provider/version/policy/algorithm metadata is closed, temporary buffers are cleared, enrollment still requires verified password recovery, and missing/cancelled/reset Keychain state falls back to the master password. Native prompt behavior remains a signed-bundle physical gate.
- Linux Secret Service integration sends base64 secret bytes only through `secret-tool` standard input, bounds and decodes lookup output without immutable secret strings, and clears temporary buffers. It requires a live session D-Bus and never enables Linux quick unlock; master-password-only authorization remains the approved fallback. GNOME/KDE-family lock detection accepts only recognized screen-saver signals and emits metadata-free lock state.
- macOS AVFoundation and Linux V4L2 preflight distinguish permission denial from no device before OpenCV capture. Platform support diagnostics expose only closed capability states and no paths or account data. CI assembles an unsigned structural DMG plus tar/DEB artifacts; production macOS distribution requires Developer ID, hardened runtime, secure timestamps, notarization, stapling, and Gatekeeper acceptance.
- M7 release validation rejects known vulnerable NuGet graphs, foreign or unresolved native dependencies, mutated artifact manifests, wrong-platform or wrong-channel signed offers, managed-package self-update, and incomplete downloads. `docs/architecture/M7_RELEASE_INTEGRITY.md` records the threat, data-flow, compatibility, recovery, and test evidence.
- Credentialed M7 tag jobs require Authenticode on Windows and Developer ID signing, notarization, stapling, and Gatekeeper acceptance on macOS. The final job signs direct payloads, the aggregate manifest, and the separate Avalonia appcast with a pinned NetSparkle tool and verifies both metadata signatures before release upload. Key contents are no longer passed in appcast-generator process arguments.
- The Windows updater now replaces files transactionally. Existing files are backed up before overwrite; failure or cancellation restores them in reverse order and removes newly copied files. Regression tests inject a later destination failure and verify that both overwritten and newly added files return to the pre-update state. No vault or settings location participates in the application-file transaction.
- Avalonia Headless loads the real app resources and main-window XAML on every supported CI runner. Target packages carry the established app mark as a Windows executable icon, Linux hicolor/desktop-entry icon, and generated macOS ICNS bundle resource.
- The M8 automated hardening audit maps security, recovery, accessibility, and performance evidence without treating headless/hosted checks as physical acceptance. New regressions reject equal-version replay, signed DTD and item-flood appcasts, restore a cancelled Windows update transaction, verify 1,000 repeated DEK lock cycles clear released buffers, restore a published vault backup generation, and resolve the dedicated high-contrast palette under the real XAML application. See `docs/architecture/M8_HARDENING_AUDIT.md`.
- The Avalonia M4 shell initializes the existing redacting logger before single-instance and UI startup, bridges DI logging to that provider, and flushes it on exit. Global UI/domain/task boundaries record exception types but never exception messages or exception objects. Fatal UI and application-domain faults make a best-effort authorization lock; UI faults request orderly nonzero shutdown. Main-window close also locks authorization and clears account, generated-output, and camera presentation before owned services are disposed. Regression tests cover message omission, fail-closed locking, shutdown failure, task policy, and idempotent close preparation.
- Authorized Avalonia navigation exposes only Accounts, Tools, or Settings at a time and is unavailable while locked. Settings behaves as a modal surface: toolbar interaction and account keyboard-command wrappers are disabled until the explicit close action returns to the prior page, and leaving Settings clears authorization inputs. Leaving Accounts clears generated OTP/QR output without reloading secret-free rows; leaving Tools cancels and clears camera capture. Locking hides the complete authorized shell and clears loaded presentation state.
- These presentation slices do not change the vault, envelope, preferences, import/export, or backup formats. WPF remains the release/default client during the migration.

## Exception Handling
When a finding cannot be fixed immediately:
1. Create a tracked issue with:
   - impact
   - exploitability
   - compensating controls
   - target remediation date
2. Link issue to release notes/security review
3. Reassess each release cycle

## Operational Runbooks
- Signing key rotation and compromise response: `docs/security/SIGNING_KEY_ROTATION.md`
