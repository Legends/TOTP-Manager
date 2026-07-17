# TOTP Manager Threat Model

## 1. Scope
- Product: TOTP Manager (WPF desktop app)
- In scope:
  - Local secret handling (OTP seeds, export/import files, passwords, keys in memory)
  - Settings and security workflows
  - Build and release pipeline
- Out of scope:
  - Third-party services not controlled by this repository

## 2. Architecture Summary
- Client: WPF desktop application
- Data stores:
  - Plaintext allowlisted non-secret preferences (`preferences.json`)
  - Password-wrapped authorization envelope (`authorization-envelope.bin`)
  - AES-GCM encrypted account vault
  - Local encrypted export files (`.totp`)
- Security services:
  - Recovery-password authorization and platform quick unlock
  - Versioned key wrapping, envelope activation, and vault-key verification

## 3. Assets and Trust Boundaries
- Assets:
  - OTP secrets
  - Master password
  - Data Encryption Key (DEK)
  - Password-wrapper and platform quick-unlock metadata
  - Exported encrypted backup files
  - Signing/release credentials in CI
- Trust boundaries:
  - User input boundary (UI and file dialog)
  - Process memory boundary
  - Filesystem boundary
  - Windows Hello/TPM provider boundary
  - CI/CD boundary (GitHub Actions, secrets)

## 4. STRIDE Threat Analysis
| Threat | Example | Current Mitigation | Gap / Action |
|---|---|---|---|
| Spoofing | Fake user input into import/export workflows | Password prompts and explicit confirmation paths | Add stronger re-auth policy for sensitive actions |
| Tampering | Modified encrypted export payload | Crypto validation during import | Add integrity verification tests per file format/version |
| Tampering | Replaced, corrupted, or rolled-back authorization envelope | Strict bounded codec, authenticated password wrapper, candidate-key vault verification, atomic replacement, one bounded backup | Add explicit rollback-version policy before multi-device synchronization |
| Repudiation | No trace of security-sensitive actions | App logging exists | Add security event taxonomy and redaction policy |
| Information Disclosure | Secrets/passwords retained in memory | Sensitive-data clearing and copied-key patterns | Add periodic memory review and secure-string strategy decision |
| Information Disclosure | Authorization material accidentally enters plaintext preferences | Allowlisted `AppPreferencesV1` mapper and strict unknown-field rejection; authorization has a separate encrypted envelope | Require data-classification review for every new preference field |
| Denial of Service | Malformed import payload crashes workflow | Error mapping and guarded workflows | Add fuzz tests for import parsers |
| Elevation of Privilege | Weak CI/release controls | GitHub secrets and signed builds | Enforce branch protection + required security workflow gates |

## 5. Attack Surfaces
- Import file parsing (`.totp`, `.json`, `.txt`, `.csv`)
- Export path handling
- Password prompt and validation flows
- Authorization-envelope and preferences file replacement
- Windows Hello/TPM registration, reset, and unlock
- CI/CD pipeline and release artifacts
- Dependency supply chain

## 6. Security Controls Baseline
- Centralized password validation service
- Mandatory recovery-password wrapper independent of platform quick unlock
- Candidate DEK verification against the vault before envelope activation
- Bounded parsing, atomic envelope replacement, and fail-closed authorization results
- Sensitive-data cleanup for prompt workflows
- Exception handling/logging in workflow boundaries
- Automated CI checks for:
  - SAST (CodeQL)
  - SCA (NuGet vulnerability/deprecation scan)
  - Secret scanning (Gitleaks)
  - Optional DAST (ZAP baseline) for externally reachable endpoints

## 7. Residual Risks
- Manual penetration testing still required for release confidence
- Desktop runtime hardening (host OS, malware resistance) is environment-dependent
- A local attacker able to run as the user can replace or roll back application files; authenticated unwrap and vault verification prevent silent key substitution but do not provide a monotonic anti-rollback counter
- Loss or reset of Windows Hello/TPM state disables quick unlock; recovery remains dependent on the master password
- Third-party dependency risk remains ongoing and must be continuously monitored

## 8. Review Cadence
- Update this model:
  - On every security-significant feature
  - On new external dependency introduction
  - Before each production release tag
