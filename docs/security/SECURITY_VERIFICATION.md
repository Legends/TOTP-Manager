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
