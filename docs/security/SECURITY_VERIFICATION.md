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
- These presentation slices do not change the vault, envelope, preferences, import/export, or backup formats. WPF remains the release/default client during M3.

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
