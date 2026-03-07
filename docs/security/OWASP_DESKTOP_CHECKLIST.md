# OWASP Desktop Security Checklist (Concrete, Module-Based)

Status values:
- `Implemented`: control exists and is enforced in code/config.
- `Partial`: control exists but coverage/strictness is incomplete.
- `Missing`: control not present or not enforceable yet.

Modules:
- `UI`: `TOTP` (WPF)
- `Core`: `TOTP.Core`
- `Infra`: `TOTP.Infrastructure`
- `DAL`: `TOTP.DAL`
- `DevSecOps`: `.github`, `scripts`, `docs/security`

## Checklist
| ID | OWASP-aligned control | UI | Core | Infra | DAL | DevSecOps | Evidence |
|---|---|---|---|---|---|---|---|
| DS-01 | Sensitive local data encrypted at rest | Partial | Implemented | Implemented | Implemented | Partial | `VaultService`, `AppSettingsDAL` (DPAPI), encrypted `.totp` export |
| DS-02 | Sensitive keys isolated in memory and cleared | Implemented | Partial | Implemented | Partial | Missing | `SecurityContext`, `MasterPasswordService`, `SeedStorageService`, password prompt/unlock consume-and-clear, and MainViewModel/OtpViewModel now clear transient flyout secret cache on close/success |
| DS-03 | Modern crypto + authenticated encryption (AEAD) | Partial | Implemented | Implemented | Implemented | Partial | AES-256-GCM + Argon2id in security services |
| DS-04 | KDF parameters validated against abuse bounds | Missing | Missing | Implemented | Partial | Missing | `MasterPasswordService` now bounds-checks iterations/memory/salt/nonce |
| DS-05 | Import parsers resistant to malformed/oversized files | Partial | Partial | Implemented | Partial | Partial | `ExportService` now enforces max import file size (5 MiB) |
| DS-06 | Least-privilege filesystem ACL for secret/settings files | Missing | Missing | Missing | Implemented | Partial | `AccountDAL` + `AppSettingsDAL` now harden ACL to current user on Windows |
| DS-07 | Secure write pattern (atomic writes, temp file) | Partial | Implemented | Partial | Implemented | Partial | `AccountDAL` temp-write + replace |
| DS-08 | Authorization required for sensitive operations | Partial | Implemented | Implemented | Partial | Partial | `AuthorizationService`, password/hello gate, session lock services |
| DS-09 | Security logging without secret leakage | Partial | Partial | Implemented | Partial | Partial | Central Serilog redaction enricher masks sensitive property names (password/secret/token/credential/dek) before sink write |
| DS-10 | Build/release security gates (SAST/SCA/secrets) | Missing | Missing | Missing | Missing | Implemented | `.github/workflows/security-audit.yml`, `SECURITY_VERIFICATION.md` |
| DS-11 | Signed release artifacts and key custody | Missing | Missing | Missing | Missing | Implemented | CI publish now signs `TOTP.UI.WPF.exe` from `SIGNING_CERT_BASE64` + `SIGNING_CERT_PASSWORD`; repo-local `.pfx` removed |
| DS-12 | Security tests for critical controls | Partial | Partial | Partial | Partial | Partial | Added tests for import limits + KDF parameter validation |

## Highest-Risk Gaps Prioritized First
1. `DS-06` file ACL hardening for secrets/settings (implemented now).
2. `DS-05` import size hard limits to reduce DoS parsing risk (implemented now).
3. `DS-04` strict Argon/KDF parameter bounds to prevent resource exhaustion (implemented now).

## Remaining High-Risk Next
1. Expand redaction policy to cover literal message text and exception message sanitization rules.
2. Reduce long-lived OTP secret plaintext in editing/view-model flows where feasible.
3. Add key-rotation runbook for signing cert and periodic secret rotation checks in CI governance.
