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
- When `SIGNING_CERT_BASE64` and `SIGNING_CERT_PASSWORD` are configured, release binary is Authenticode-signed in CI (ephemeral cert file only)

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
- Manual lock delegates to `IAuthorizationService.Lock`, clears generated codes, selection, search text, and projected account rows, then returns the shell to the password gate. Regression coverage verifies both authorization-state locking and presentation-state cleanup.
- The first Avalonia settings page edits only the already-reviewed idle-timeout and lock-on-minimize preferences through `ISettingsService`. It does not bind or serialize authorization metadata. Failed or exceptional saves restore the prior active values and expose only generic UI text; tests cover successful persistence and rollback.
- Avalonia clipboard writes use a separate asynchronous contract and a receipt tied to the exact in-process `DataTransfer` instance. Timed clearing occurs only while that object remains the current clipboard owner; a user or another application changing the clipboard prevents clearing. The adapter releases its ownership reference after clear, ownership loss, replacement, or disposal and never logs code text.
- Conditional ownership is enabled only on Windows, macOS, and X11, matching Avalonia's documented in-process ownership support. Wayland and unknown Linux sessions advertise write-only capability, so the security-sensitive copy action fails before writing rather than copying a code it cannot safely clear. Adapter and scheduler tests cover exact-receipt clearing, changed-clipboard preservation, unsupported-capability fail-closed behavior, and UI duration forwarding.
- QR generation follows an account-ID boundary parallel to TOTP generation. Infrastructure alone resolves the account seed and constructs the `otpauth` URI, returns PNG bytes in an owned `SensitiveBuffer`, and zeroes its temporary PNG array. Avalonia zeroes its decoding copy and disposes the secret-bearing bitmap on selection change, lock, or view-model disposal. Neither seed, URI, PNG, nor exception text is logged. Tests cover seed confinement, temporary-buffer clearing, generic failures, bitmap projection, and lock cleanup; no storage or export format changes are involved.
- The M3 native file-picker probe uses Avalonia's platform storage provider with an allowlisted `*.totp`/`*.json` filter and single-selection mode. It retains and displays only the selected file name, does not open or import the file, does not log a path, and labels import as disabled in the preview. Stream access, schema validation, authorization, and atomic import remain a later workflow and are not implied by this picker validation.
- Avalonia single-instance coordination uses a per-user hashed mutex name and a `PipeOptions.CurrentUserOnly` named pipe. The protocol accepts only the fixed two-byte activation version/kind pair, rejects unsupported values, carries no paths or secrets, and can only request main-window activation; it never unlocks the vault. A secondary instance exits after a successful redirect and fails closed with a non-zero exit code when the primary cannot be reached. Windows unit tests and real Ubuntu/macOS CI transport tests cover mutual exclusion and activation round trips.
- Camera capture is isolated in `TOTP.Camera.OpenCv`; neither WPF nor Avalonia references OpenCV native types. Expected permission, unavailable-device, device-loss, stalled-frame, and native-runtime failures use typed results instead of exception-message parsing. Avalonia requests capture only after an explicit scan action, zeroes each encoded preview buffer after decoding, disposes replaced previews, cancels capture on lock/disposal, and validates decoded data without logging or displaying the seed. The OpenCvSharp managed and native packages are version-aligned at 4.13.0.20260627. Deterministic tests cover failure mapping, cancellation, device loss, stalls, capture disposal, UI lifecycle cleanup, safe metadata projection, and oversized/invalid payload rejection. Physical permission prompts, hot-plug behavior, and repeated real-device capture remain target validation gates.
- The Avalonia M3 update probe verifies an embedded, non-production appcast with Ed25519 before parsing it, enforces a 256 KiB document limit, prohibits DTD resolution, caps item count, requires HTTPS artifact URIs, and selects only newer OS/architecture-compatible entries. Its synthetic public key cannot authorize production updates, and the probe never downloads or installs the example artifact. Regression coverage verifies the fixture with both the portable verifier and NetSparkle's strict `Ed25519Checker`, and covers tampering, malformed signatures, version filtering, and oversized input.
- The Avalonia M4 shell initializes the existing redacting logger before single-instance and UI startup, bridges DI logging to that provider, and flushes it on exit. Global UI/domain/task boundaries record exception types but never exception messages or exception objects. Fatal UI and application-domain faults make a best-effort authorization lock; UI faults request orderly nonzero shutdown. Main-window close also locks authorization and clears account, generated-output, and camera presentation before owned services are disposed. Regression tests cover message omission, fail-closed locking, shutdown failure, task policy, and idempotent close preparation.
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
