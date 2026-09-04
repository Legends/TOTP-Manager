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
- Microsoft Store is the primary Windows release path. CI builds an unsigned Partner Center-only MSIX with Store-managed updates and no standalone updater; the package becomes distributable only after Microsoft certification/signing and physical acceptance. The previous SignPath Foundation application was not approved, so no Foundation certificate or active direct stable-download channel exists.
- Projects, CI, and framework-dependent packaging target .NET 10. The runtime migration changes no vault, envelope, preference, import/export, or update-metadata format; self-contained public packages continue to carry their reviewed runtime.
- Until platform signing credentials are funded, RC tags may publish only explicitly labeled unsigned Windows/Linux development previews. Their package configuration disables automatic updates, their manifest permits only manual preview downloads, and the workflow excludes appcasts and macOS assets.
- Stable Avalonia packaging keeps assembly identity at the valid `2.0.0.0` form while reserving file/appcast revision `65535` for ordering after every bounded RC. The updater reads the injected file-version provider, and regression coverage prevents a stable build from comparing as older than its RCs.

## Portable Authorization Cutover Evidence

- Runtime composition resolves `PortableSettingsService`, `PortableAuthorizationService`, the v2 password lifecycle, and the Windows quick-unlock adapter as one graph.
- The development-era DPAPI settings DAL is absent from the runtime service graph; it remains only in historical-format tests and documentation.
- Strict codec, store, activation, password lifecycle, session, quick-unlock enrollment, facade, and authorization-prompt regressions cover malformed inputs, wrong credentials, missing platform state, atomic replacement, rollback, vault verification, cancellation, and temporary-key clearing.
- A supported-hardware Windows Hello/TPM registration and unlock smoke test remains a manual pre-release gate.

## Avalonia Presentation Boundary Evidence

- Password unlock delegates to the portable authorization facade, removes the bound password before awaiting verification, and never echoes credential or exception text.
- The account-list projection retains only account ID, issuer, and account name. OTP seeds remain outside the Avalonia row model and are never required for list rendering.
- Synthetic 500-account coverage verifies the list capacity and the absence of a `Secret` member on the presentation type. Search coverage verifies that filtering is limited to issuer and account name. Failure tests verify that account loading is recoverable and does not expose underlying exception text.
- `IAccountTotpService` resolves the selected account and invokes the existing zeroing `ITotpGenerator` inside Infrastructure. Avalonia supplies only the non-secret account ID and never receives the seed. Generated codes are never logged and are removed from presentation state when their current time step expires, selection changes, or the view model is disposed. Managed code strings cannot be deterministically zeroed, so their lifetime is minimized rather than claimed to be zeroized.
- TOTP tests verify account-ID selection, seed confinement to the infrastructure service, invalid-seed redaction, generic UI failures, and expiring-code projection.
- Manual lock delegates to `IAuthorizationService.Lock`, clears generated codes, selection, search text, and projected account rows, then returns to the configured quick-unlock or password gate. Quick-unlock cancellation remains locked, while the explicit master-password fallback changes only the current presentation and does not silently rewrite the persisted unlock preference. Regression coverage verifies password and quick-unlock gate projection, successful reauthorization, cancellation, preference preservation, authorization-state locking, and presentation-state cleanup.
- Avalonia settings are grouped into Security, Import/Export, Miscellaneous, and About tabs. Vault locking and conditional clipboard-clear preferences appear with the authorization controls under Security; this presentation-only move introduces no new authorization metadata, secret binding, storage format, or migration impact. Valid general-preference edits use a 200 ms coalesced auto-save and persist through `ISettingsService`; authorization changes continue through `IAuthorizationService` and recovery-password confirmation dialogs. Loading does not trigger persistence, and failed or exceptional preference saves restore the prior active values and expose only generic UI text; tests cover tab placement, coalescing, successful persistence, and rollback.
- The saved minimum log level is applied to the redacting shared log switch during startup, after early logging is initialized and before normal application orchestration. A command-line level remains authoritative, malformed persisted enum values fall back to Information, and preference-load failures retain the active safe default. This changes filtering only: sinks, redaction, log content, preference format, and migration behavior are unchanged. Regression tests cover the saved-level path, override precedence, malformed-value fallback, and load failure.
- Avalonia clipboard writes use a separate asynchronous contract and a receipt tied to the exact in-process `DataTransfer` instance. Timed clearing occurs only while that object remains the current clipboard owner; a user or another application changing the clipboard prevents clearing. The adapter releases its ownership reference after clear, ownership loss, replacement, or disposal and never logs code text.
- Conditional ownership is enabled only on Windows, macOS, and an available X11 display, matching Avalonia's documented in-process ownership support and its current Linux desktop backend. WSLg is detected through `DISPLAY` because it does not reliably populate `XDG_SESSION_TYPE`; unknown Linux sessions without X11 advertise write-only capability. Clearing occurs only while the exact in-process `DataTransfer` receipt remains current, and another clipboard owner prevents clearing. When conditional clearing is disabled or unavailable, copying remains available and the unsupported case produces a localized warning. Adapter, policy, and scheduler tests cover WSLg/X11 detection, exact-receipt clearing, changed-clipboard preservation, write-only fallback, and UI duration forwarding. No storage, secret, or compatibility format changes are involved.
- Selecting an account in Avalonia generates and immediately copies that account's current code. Selection changes during generation cannot copy the superseded account's code, and periodic time-step refreshes update the display without repeatedly replacing the user's clipboard. Automatic-clear opt-out performs a normal copy; unsupported conditional ownership also copies and warns rather than silently claiming cleanup. Regression coverage verifies selected-code copying, bounded clear lifetime, localized fallback status, and rejection of stale-account copies.
- QR generation follows an account-ID boundary parallel to TOTP generation. Infrastructure alone resolves the account seed and constructs the `otpauth` URI, returns PNG bytes in an owned `SensitiveBuffer`, and zeroes its temporary PNG array. Avalonia zeroes its decoding copy, displays the bitmap directly in a temporary owned preview dialog, and disposes it as soon as that dialog closes. The dialog creates no additional secret-bearing copy and closes on Escape, selection change, lock, or view-model disposal. Neither seed, URI, PNG, nor exception text is logged. Tests cover seed confinement, temporary-buffer clearing, generic failures, direct preview routing and cleanup, Escape closure, and lock cleanup; no storage or export format changes are involved.
- Native import uses Avalonia's platform storage provider with allowlisted `*.totp`, `*.json`, `*.txt`, and `*.csv` filters and single-selection mode. The selected stream passes through bounded format validation and the account-import workflow; encrypted backups require their export password. Import creates a recovery backup before mutation, applies the selected conflict policy, and exposes only presentation-safe outcomes.
- Avalonia single-instance coordination uses a per-user hashed mutex name and a `PipeOptions.CurrentUserOnly` named pipe. The protocol accepts only the fixed two-byte activation version/kind pair, rejects unsupported values, carries no paths or secrets, and can only request main-window activation; it never unlocks the vault. A secondary instance exits after a successful redirect and fails closed with a non-zero exit code when the primary cannot be reached. Windows unit tests and real Ubuntu/macOS CI transport tests cover mutual exclusion and activation round trips.
- Camera capture is isolated in `TOTP.Camera.OpenCv`; Avalonia does not reference OpenCV native types. Expected permission, unavailable-device, device-loss, stalled-frame, and native-runtime failures use typed results instead of exception-message parsing. Avalonia requests capture only after an explicit scan action; the toolbar action opens an owned scanner dialog and starts capture directly. The dialog clears and cancels capture on cancel, close, session lock, or disposal, zeroes each encoded preview buffer after decoding, disposes replaced previews, and validates decoded data without logging or displaying the seed. Successful imports return only the non-secret account ID, status, and localized result to the account page so the refreshed row can be selected and revealed; no seed or QR payload crosses that presentation event. Nested conflict prompts remain owned by the active scanner window. The OpenCvSharp managed and native packages are version-aligned at 4.13.0.20260627. Deterministic tests cover failure mapping, cancellation, device loss, stalls, capture disposal, direct dialog routing, nested ownership, UI lifecycle cleanup, safe metadata projection, imported-row reveal, and oversized/invalid payload rejection. Physical permission prompts, hot-plug behavior, and repeated real-device capture remain target validation gates.
- Update parsing verifies Ed25519 metadata before use, enforces a 256 KiB document limit, prohibits DTD resolution, caps item count, requires HTTPS artifact URIs, and selects only newer OS/architecture-compatible entries. Test fixtures use a synthetic key that cannot authorize production updates. Regression coverage covers tampering, malformed signatures, version filtering, and oversized input.
- Portable update-check failures now carry typed presentation-safe categories. Missing or unreachable feeds are reported as unavailable rather than as signature failures, while invalid appcast formats or Ed25519 signatures still fail closed and retain the explicit verification-failure warning. No feed, key, signature, artifact-selection, download, or installation rule changed; no data-format or migration impact exists. Regression coverage distinguishes HTTP unavailability from cryptographic rejection without exposing transport or verifier details in the UI.
- The Windows Avalonia installer adapter repeats Ed25519 verification while holding the downloaded ZIP without write/delete sharing, rejects non-ZIP packages and reparse points in its bundled helper, uses structured process arguments, and keeps the application running unless the visible updater process signals readiness. The production Avalonia feed remains disabled pending target-qualified release artifacts and physical install/relaunch acceptance.
- The macOS quick-unlock adapter stores the 32-byte vault key only in a data-protection Keychain item guarded by `userPresence`; LocalAuthentication availability uses the device-owner policy. The envelope contains only an opaque item reference and fixed-size SHA-256 binding. Provider/version/policy/algorithm metadata is closed, temporary buffers are cleared, enrollment still requires verified password recovery, and missing/cancelled/reset Keychain state falls back to the master password. Native prompt behavior remains a signed-bundle physical gate.
- Linux Secret Service integration sends base64 secret bytes only through `secret-tool` standard input, bounds and decodes lookup output without immutable secret strings, and clears temporary buffers. It requires a live session D-Bus and never enables Linux quick unlock; master-password-only authorization remains the approved fallback. GNOME/KDE-family lock detection accepts only recognized screen-saver signals and emits metadata-free lock state.
- macOS AVFoundation and Linux V4L2 preflight distinguish permission denial from no device before OpenCV capture. Platform support diagnostics expose only closed capability states and no paths or account data. CI assembles an unsigned structural DMG plus tar/DEB artifacts; production macOS distribution requires Developer ID, hardened runtime, secure timestamps, notarization, stapling, and Gatekeeper acceptance.
- M7 release validation rejects known vulnerable NuGet graphs, foreign or unresolved native dependencies, mutated artifact manifests, wrong-platform or wrong-channel signed offers, managed-package self-update, and incomplete downloads. `docs/architecture/M7_RELEASE_INTEGRITY.md` records the threat, data-flow, compatibility, recovery, and test evidence.
- The dormant credentialed M7 direct-download path still fails closed unless Windows Authenticode and macOS Developer ID/notarization requirements are satisfied. It is not the active Windows Store path. Any future activation must verify platform signatures, sign direct payload metadata with the pinned NetSparkle tool, and keep key contents out of process arguments.
- The Windows updater now replaces files transactionally. Existing files are backed up before overwrite; failure or cancellation restores them in reverse order and removes newly copied files. Regression tests inject a later destination failure and verify that both overwritten and newly added files return to the pre-update state. No vault or settings location participates in the application-file transaction.
- Updater startup and installation failures display localized recovery guidance and log exception types only. Raw exception objects, messages, package paths, and target paths are not projected into the updater UI or failure log; regression coverage uses a synthetic sensitive path marker.
- Avalonia Headless loads the real app resources and main-window XAML on every supported CI runner. Target packages carry the established app mark as a Windows executable icon, Linux hicolor/desktop-entry icon, and generated macOS ICNS bundle resource.
- The M8 automated hardening audit maps security, recovery, accessibility, and performance evidence without treating headless/hosted checks as physical acceptance. New regressions reject equal-version replay, signed DTD and item-flood appcasts, restore a cancelled Windows update transaction, verify 1,000 repeated DEK lock cycles clear released buffers, restore a published vault backup generation, and resolve the dedicated high-contrast palette under the real XAML application. See `docs/architecture/M8_HARDENING_AUDIT.md`.
- The Avalonia shell initializes the redacting logger before single-instance and UI startup, bridges DI logging to that provider, and flushes it on exit. Global UI/domain/task boundaries record exception types but never exception messages or exception objects. Fatal UI and application-domain faults make a best-effort authorization lock; UI faults request orderly nonzero shutdown. Main-window close also locks authorization and clears account, generated-output, and camera presentation before owned services are disposed. Regression tests cover message omission, fail-closed locking, shutdown failure, task policy, and idempotent close preparation.
- Authorized Avalonia navigation exposes only Accounts, Tools, or Settings at a time and is unavailable while locked. Settings behaves as a modal surface: toolbar interaction and account keyboard-command wrappers are disabled until the explicit close action returns to the prior page, and leaving Settings clears authorization inputs. Leaving Accounts clears generated OTP/QR output without reloading secret-free rows; leaving Tools cancels and clears camera capture. Locking hides the complete authorized shell and clears loaded presentation state.
- Retiring the legacy WPF client does not change the vault, envelope, preferences, import/export, or backup formats. Avalonia is the only release/default client.
- Idle locking uses monotonic elapsed time and an application-wide Avalonia input heartbeat. OS session-lock subscriptions are established synchronously during hosted-service startup, and either automatic lock path clears the same secret-bearing presentation state as manual lock.

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
