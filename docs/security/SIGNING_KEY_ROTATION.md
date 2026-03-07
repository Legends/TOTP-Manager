# Signing Key Rotation Runbook

## Scope
- Applies to release Authenticode signing inputs used by CI:
  - `SIGNING_CERT_BASE64`
  - `SIGNING_CERT_PASSWORD`

## Rotation Cadence
- Rotate at least every 12 months.
- Rotate immediately on any suspected exposure.
- Rotate on maintainer offboarding with signing access.

## Standard Rotation Procedure
1. Acquire/import replacement code-signing certificate (`.pfx`) from approved CA process.
2. Convert certificate to base64 (local secure workstation):
   - `pwsh ./TOTP/Properties/Signing/PfxToBase64.ps1 -PfxPath "C:\secure\new-cert.pfx"`
3. Update GitHub repository secrets:
   - replace `SIGNING_CERT_BASE64`
   - replace `SIGNING_CERT_PASSWORD`
4. Create a temporary release-candidate tag and verify:
   - publish workflow completes
   - signing step runs
   - signature verifies on produced executable
5. Revoke/archive old certificate according to CA and organizational policy.
6. Record rotation date, operator, and cert thumbprint in internal release notes.

## Incident Response (Compromise Suspected)
1. Immediately disable release publishing (or force signing step skip).
2. Revoke compromised certificate with CA.
3. Rotate both secrets in GitHub (`SIGNING_CERT_BASE64`, `SIGNING_CERT_PASSWORD`).
4. Re-sign latest trusted release artifacts with replacement certificate.
5. Publish security notice if public artifacts may have been impacted.

## Verification Checklist
- Workflow `build-and-test` publish job shows signing step executed.
- Signed binary verifies with `signtool verify /v /pa`.
- No `.pfx` or certificate password committed to repository.
