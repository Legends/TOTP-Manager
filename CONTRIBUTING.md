# Contributing to TOTP Manager

This document onboards senior engineers to the project's architecture, security model, and development standards.

## Engineering Principles

- Secure-by-design is mandatory.
- MVVM boundaries are strict.
- DI + interfaces are required for testability and replaceability.
- Prefer handling failures over throwing exceptions.
- Result-pattern/return-based control flow is preferred for expected failures.
- No plaintext secret persistence in logs, files, or long-lived memory structures.

---

## 1. Development Environment Setup

### Required Tooling

- Windows 10/11
- .NET SDK 8.0+ (project currently targets .NET 9 where configured)
- Visual Studio 2022 (17.8+ recommended) with:
  - .NET Desktop Development workload
  - Test tools
  - GitHub integration (optional)
- Git

### Clone and Restore

```powershell
git clone https://github.com/Legends/TOTP-Manager.git
cd TOTP-Manager
dotnet restore TOTP.sln --configfile NuGet.config
dotnet build TOTP.sln -c Debug
dotnet test TOTP.Tests\TOTP.Tests.csproj
```

### Visual Studio Recommendations

- Enable nullable reference type warnings as errors where feasible.
- Enable analyzers and treat critical warnings as build blockers.
- Use x64 debug profile for local testing.
- Avoid "Just My Code" when debugging crypto/security flows.

---

## 2. Architecture Deep-Dive

### Current Product Feature Set

The application currently implements these end-user capabilities:

- Local encrypted TOTP vault with on-device storage only
- TOTP code generation with standard defaults (`SHA1`, `6` digits, `30s` period)
- Account CRUD workflows
- Inline account search/filtering in the main grid
- QR import via camera scanning
- QR generation and enlarged QR preview for transfer to another device
- Inline edit flow plus full edit/add flyouts
- Encrypted export/import workflows for backup and migration
- Conflict-handling support during import
- Master password setup and unlock flow
- Optional Windows Hello setup and unlock support
- Session locking on logout/manual lock
- Lock on minimize
- Lock on Windows session lock
- Idle-timeout driven auto-lock
- Clipboard copy of generated TOTP codes
- Optional delayed clipboard clearing
- Localization support (currently English/German)
- Settings flyout for general, security, transfer, and logging options
- NetSparkle-based automatic update checks using Ed25519-signed appcasts
- Startup splash screen with dedicated splash UI thread
- Scanner backend warmup to reduce first QR scan latency
- Structured file logging with configurable minimum log level
- Single-instance enforcement with activation of the existing window

The release pipeline currently produces:

- `fast` release artifact: framework-dependent, startup-optimized
- `portable` release artifact: self-contained single-file
- GitHub release assets for NetSparkle auto-update (`TOTP.UI.WPF.exe`, `appcast.xml`, `appcast.xml.signature`)

### Solution Structure

- `TOTP.Core`
  - Domain models, contracts, service interfaces, security abstractions
- `TOTP.Infrastructure`
  - Concrete implementations (security, crypto orchestration, persistence-facing services)
- `TOTP.DAL`
  - Data access implementation (`OtpDAL`) and persistence concerns
- `TOTP` (`TOTP.UI.WPF`)
  - WPF UI, ViewModels, MVVM commands, composition root/bootstrap
- `TOTP.Tests`
  - Unit tests and regression tests

### Core vs UI Responsibilities

- `Core`: policy, contracts, domain-safe behavior
- `UI.WPF`: presentation, bindings, user interaction orchestration only
- Never place crypto/business decisions directly in code-behind.

### Interfaces + DI

- Every service dependency must be represented by an interface.
- Constructor injection only (no service locator anti-pattern).
- Composition and registration are centralized in startup/bootstrap (`BootLoader`/service registration).

### DAL (`OtpDAL`) Expectations

- DAL is responsible for persistence mechanics, not business policy.
- Input validation and security decisions happen above DAL in services/workflows.
- DAL methods should return deterministic outcomes and map low-level errors to app-level results where possible.

---

## 3. Security Protocols

### Argon2id

- Used for password-based key derivation.
- Parameters must remain security-reviewed (time cost, memory cost, parallelism).
- Any parameter changes require migration strategy and compatibility review.

### DPAPI

- Used to protect local credential/key material with Windows account context.
- Do not bypass DPAPI for "convenience" persistence.
- Recovery/portability implications must be documented for users.

### Secure Code Only Mandate

- No plaintext secrets in logs.
- Minimize secret lifetime in memory.
- Prefer ephemeral byte buffers over immutable strings for sensitive transformations when feasible.
- Clear temporary buffers after use (`Array.Clear` etc.).
- Never commit real secrets, tokens, or private keys.

---

## 4. Coding Standards

### General

- Follow SOLID, DRY, KISS, and clear OOP boundaries.
- Keep methods cohesive and side effects explicit.
- Favor small, testable units with interface-driven seams.

### Error Handling

- Use try/catch at boundary layers (I/O, crypto, external service calls, workflow orchestration).
- For expected business failures, return `Result`/typed outcomes instead of throwing.
- Exceptions are for truly exceptional/unrecoverable conditions.
- User-facing flows must fail gracefully with meaningful messages.

### File Update Policy

- PRs and AI-assisted contributions must provide full-file updates for modified files (no partial/snippet-only patch artifacts in review docs).

### Testing Expectations

- Unit tests are mandatory for new behavior and regressions.
- Preferred stack:
  - xUnit
  - Moq
  - AutoFixture + AutoMoq where useful
  - FluentAssertions
- Cover:
  - MVVM command behavior
  - Property change notifications
  - Security-sensitive workflow branching
  - Failure and recovery paths
- Add regression tests for every security bug fix.

---

## 5. DevOps & CI/CD

### GitHub Actions

Current pipeline includes:
- `build-test`
- `sast-codeql`
- `sca-dotnet`
- `secrets-scan`
- Optional `dast-zap` (manual dispatch with target URL)

Security docs are under:
- `docs/security/THREAT_MODEL.md`
- `docs/security/SECURITY_VERIFICATION.md`
- `docs/security/PENTEST_PLAN.md`
- `docs/security/BRANCH_PROTECTION.md`

### Branching and Merge Policy

- Work from feature branches.
- Open PR to `master`.
- Required checks must pass before merge.
- Direct pushes to protected branches are disallowed by policy.

### Docker / Swarm / Kubernetes

The desktop app itself is not containerized for runtime use.  
Container/orchestration support is intended for potential backend sync services and security tooling environments:

- Docker for local reproducible tooling/security scans
- Swarm/Kubernetes for future sync backend deployment topologies
- Keep infra manifests versioned and environment-specific

---

## 6. Security Review Requirements for PRs

Every PR affecting security-sensitive code should include:

- Threat impact statement
- Data-flow impact (what secret/key material changes)
- Error-handling behavior
- Test evidence (unit/regression)
- Any migration impact (especially for password/KDF/storage changes)

---

## 7. Commit and PR Quality Bar

- Small, focused commits with clear intent.
- PR description must include:
  - What changed
  - Why it changed
  - Security impact
  - Test results
- No TODO-based security debt merged without tracked follow-up issue and owner.

---

## 8. Quick Start Checklist for New Senior Dev

1. Build and run tests locally.
2. Read `docs/security/*`.
3. Review DI registrations and service boundaries.
4. Review recent security-related regressions in tests.
5. Start with a small, non-invasive change to validate local workflow.
