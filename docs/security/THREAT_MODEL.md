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
  - Local settings/state
  - Local encrypted export files (`.totp`)
- Security services:
  - Authorization and password validation
  - Key management / crypto wrappers

## 3. Assets and Trust Boundaries
- Assets:
  - OTP secrets
  - Master password
  - Data Encryption Key (DEK)
  - Exported encrypted backup files
  - Signing/release credentials in CI
- Trust boundaries:
  - User input boundary (UI and file dialog)
  - Process memory boundary
  - Filesystem boundary
  - CI/CD boundary (GitHub Actions, secrets)

## 4. STRIDE Threat Analysis
| Threat | Example | Current Mitigation | Gap / Action |
|---|---|---|---|
| Spoofing | Fake user input into import/export workflows | Password prompts and explicit confirmation paths | Add stronger re-auth policy for sensitive actions |
| Tampering | Modified encrypted export payload | Crypto validation during import | Add integrity verification tests per file format/version |
| Repudiation | No trace of security-sensitive actions | App logging exists | Add security event taxonomy and redaction policy |
| Information Disclosure | Secrets/passwords retained in memory | Sensitive-data clearing and copied-key patterns | Add periodic memory review and secure-string strategy decision |
| Denial of Service | Malformed import payload crashes workflow | Error mapping and guarded workflows | Add fuzz tests for import parsers |
| Elevation of Privilege | Weak CI/release controls | GitHub secrets and signed builds | Enforce branch protection + required security workflow gates |

## 5. Attack Surfaces
- Import file parsing (`.totp`, `.json`, `.txt`, `.csv`)
- Export path handling
- Password prompt and validation flows
- CI/CD pipeline and release artifacts
- Dependency supply chain

## 6. Security Controls Baseline
- Centralized password validation service
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
- Third-party dependency risk remains ongoing and must be continuously monitored

## 8. Review Cadence
- Update this model:
  - On every security-significant feature
  - On new external dependency introduction
  - Before each production release tag
