# AGENTS.md

This file is the working contract for humans and coding agents contributing to `TOTP-Manager`.

It is intentionally opinionated. The repo is security-sensitive, Windows-specific, and already has clear architectural direction. Contributions should reinforce that direction, not dilute it.

## Mission

Build a trustworthy Windows desktop TOTP manager that is:

- local-first
- secure by default
- polished enough for daily personal use
- maintainable under long-term iteration
- releaseable with high confidence

The product aspiration is not "just a code generator". It is a serious desktop authenticator with:

- encrypted local storage
- reliable unlock and authorization flows
- import/export and backup safety
- robust update delivery
- strong regression coverage around security-sensitive paths

If a proposed change improves convenience but weakens trust, auditability, or recovery confidence, reject it or redesign it.

## Product Direction

Use these as prioritization rules when tradeoffs are unclear.

### 1. Security first

Protect OTP seeds, passwords, derived keys, exported backups, and update/release credentials as first-class assets.

Expected posture:

- no plaintext secret persistence
- no accidental secret logging
- short-lived sensitive buffers where feasible
- explicit authorization for sensitive actions
- documented security impact for meaningful changes

### 2. Desktop quality matters

This is a Windows WPF app, not a web wrapper. UX should feel stable, native, responsive, and respectful of user attention.

Priorities:

- fast startup
- reliable single-instance behavior
- predictable lock/unlock behavior
- low-friction account CRUD
- smooth QR workflows
- safe update/install flow

### 3. Architecture remains intentional

The repository already points toward strict layering, MVVM, DI, and testable workflows. Preserve those boundaries.

### 4. Shipping discipline

Releases, auto-update metadata, signatures, and CI behavior are part of the product. Treat them as product code, not afterthoughts.

## Current Repo Shape

### Solution layout

- `TOTP.Core`
  - domain models, enums, common primitives, contracts, security abstractions
- `TOTP.Infrastructure`
  - concrete implementations for logging, crypto orchestration, security, settings, account management, export, QR generation
- `TOTP.DAL`
  - persistence and filesystem-facing data access
- `TOTP`
  - WPF UI, view models, commands, workflows, bootstrap, services, assets
- `TOTP.Tests`
  - unit, regression, security-adjacent, and integration tests
- `TOTP.Updater`
  - updater/install support UI and logic
- `scripts`
  - release, security, and local update/testing helpers
- `docs/security`
  - threat model, verification notes, branch protection, signing/update documentation

### Platform/runtime

- Windows only
- .NET 9
- WPF desktop UI
- CI runs on `windows-latest`

### Key libraries already in play

- `FluentResults`
- `Serilog`
- `Otp.NET`
- `QRCoder`
- `OpenCvSharp`
- `ZXing.Net`
- `NetSparkleUpdater`
- `Syncfusion`
- `xUnit v3`
- `Moq`
- `AutoFixture.AutoMoq`
- `FluentAssertions`

## Non-Negotiable Engineering Rules

### Security rules

- Never log OTP seeds, master passwords, DPAPI payloads, export secrets, or raw secret material.
- Never introduce plaintext-at-rest shortcuts for debugging or convenience.
- Never commit real secrets, private keys, certificates, or personal backup artifacts.
- Any change to crypto, KDF parameters, password handling, authorization flow, storage format, import/export format, or update verification requires explicit security review thinking.
- If sensitive data must briefly exist in memory, minimize lifetime and clear temporary buffers where practical.

### Architecture rules

- Keep MVVM boundaries strict.
- Keep business/security decisions out of WPF code-behind.
- Prefer interface-driven services and constructor injection.
- Keep composition rooted in startup/bootstrap, primarily [`TOTP/Startup/BootLoader.cs`](E:/Repos/TOTP-Manager/TOTP/Startup/BootLoader.cs).
- Do not add service-locator-style resolution inside feature code unless there is already a clear repo pattern and no cleaner seam.

### Error handling rules

- Use return-based/result-based control flow for expected failures.
- Catch exceptions at boundaries: file I/O, OS integration, crypto, update/install, camera/scanner interaction, startup orchestration.
- Make user-visible failure modes recoverable and explicit.
- Add tests for failure branches, not only happy paths.

### Testing rules

- New behavior should come with tests.
- Security fixes must come with regression tests.
- Prefer targeted, deterministic tests near the changed workflow.
- Avoid tests that only mirror implementation details with no behavioral value.

## Architectural Intent By Layer

### `TOTP.Core`

Owns:

- domain concepts
- contracts/interfaces
- security abstractions
- cross-layer primitives and error codes

Should not own:

- WPF UI concerns
- filesystem details
- concrete infrastructure wiring

### `TOTP.Infrastructure`

Owns:

- implementations behind core contracts
- security orchestration
- logging/redaction behavior
- settings/export/account services

Should not become:

- a second UI layer
- a dumping ground for unrelated helpers

### `TOTP.DAL`

Owns:

- persistence mechanics
- local file handling
- low-level storage mapping

Should not own:

- product policy
- authorization decisions
- UI messaging

### `TOTP`

Owns:

- WPF views
- view models
- commands
- app startup
- orchestration between services and user interaction

Should not own:

- raw crypto policy
- persistence internals
- secret-handling shortcuts for binding convenience

## What Good Changes Look Like

Good contributions usually have these properties:

- they improve one workflow clearly
- they keep dependencies explicit
- they reduce ambiguity around failures
- they preserve or improve testability
- they do not leak security details into presentation logic
- they leave logging safer, not noisier
- they fit the current release/update model instead of bypassing it

Examples:

- tightening authorization around export/import
- improving startup reliability without weakening diagnostics
- adding regression coverage for lock/unlock edge cases
- improving QR scanning robustness and test seams
- hardening logging redaction
- clarifying release automation and appcast generation

## What To Avoid

- "quick fixes" in code-behind that bypass view-model or service boundaries
- hidden static state when DI would be cleaner
- catch-and-ignore error handling
- adding dependencies without clear need
- generic abstractions with no current payoff
- premature cloud/backend assumptions in a local-first app
- weakening update verification, signing, or release metadata handling
- storing secret-bearing values in immutable strings longer than necessary when a safer pattern exists

## Repo-Specific Workflows

### Restore

```powershell
dotnet restore TOTP.sln --configfile NuGet.config
```

### Build

```powershell
dotnet build TOTP.sln -c Debug
```

### Run tests

Full:

```powershell
dotnet test TOTP.sln -c Debug
```

Fast PR-like subset:

```powershell
dotnet test TOTP.sln -c Release --no-build --filter "FullyQualifiedName!~Integration&FullyQualifiedName!~IdleMonitoringBackgroundServiceTests&FullyQualifiedName!~UserActivityServiceTests"
```

### Run locally

```powershell
dotnet run --project .\TOTP\TOTP.UI.WPF.csproj
```

### Debug auto-update locally

Use the scripts under `scripts/release` and the guidance in:

- [`docs/security/AUTO_UPDATE.md`](E:/Repos/TOTP-Manager/docs/security/AUTO_UPDATE.md)
- [`docs/security/AUTO_UPDATE_INSTALL_PROCESS.md`](E:/Repos/TOTP-Manager/docs/security/AUTO_UPDATE_INSTALL_PROCESS.md)

Relevant helpers include:

- `scripts/release/Generate-Appcast.ps1`
- `scripts/release/Setup-LocalAutoUpdateTest.ps1`
- `scripts/release/Start-LocalUpdateFeedServer.ps1`
- `scripts/release/Setup-GitHubAutoUpdate.ps1`

## CI / Release Reality

The repo already treats release engineering seriously. Match that standard.

### Current CI expectations

GitHub Actions currently builds/tests on push and PR, and publishes on version tags.

Observed workflow expectations:

- restore from `NuGet.config`
- build in `Release`
- PR test runs are filtered for speed
- push/tag runs execute the fuller test set
- tagged releases publish `fast` and `portable` artifacts
- appcast generation/signing is integrated when secrets are configured

### Release tag format

The workflow expects:

```text
v<major>.<minor>.<patch>
v<major>.<minor>.<patch>-rc<nr>
```

Do not change release versioning casually. It affects published assets and appcast metadata.

### Auto-update

Auto-update is a product feature, not an ops detail.

Guardrails:

- do not weaken signature validation
- do not hardcode private material into repo files
- preserve compatibility between published binaries and appcast metadata
- verify version fields when changing publish flow

## Startup, Logging, and Diagnostics

Startup is performance- and reliability-sensitive. The current app already records startup stages and uses early logging.

When touching startup code:

- preserve single-instance behavior
- preserve splash/startup sequencing unless intentionally redesigning it
- avoid blocking the UI thread unnecessarily
- preserve or improve startup diagnostics
- treat logging redaction as mandatory

Key files:

- [`TOTP/Program.cs`](E:/Repos/TOTP-Manager/TOTP/Program.cs)
- [`TOTP/Startup/BootLoader.cs`](E:/Repos/TOTP-Manager/TOTP/Startup/BootLoader.cs)
- [`TOTP/Infrastructure/Logging/LoggingConfigurator.cs`](E:/Repos/TOTP-Manager/TOTP/Infrastructure/Logging/LoggingConfigurator.cs)
- [`TOTP.Infrastructure/Logging/SensitiveTextRedactor.cs`](E:/Repos/TOTP-Manager/TOTP.Infrastructure/Logging/SensitiveTextRedactor.cs)

## Security Review Triggers

Treat the following as mandatory review triggers:

- password setup/unlock flow changes
- Argon2id parameter changes
- DPAPI handling changes
- vault/encryption/storage format changes
- import/export schema or cryptographic wrapper changes
- update signing/appcast validation changes
- logging/redaction changes
- Windows Hello integration changes
- backup/restore behavior changes

When making these changes, document:

- threat impact
- data-flow impact
- compatibility or migration impact
- test evidence

## Guidance For AI Agents

### Default posture

- read before writing
- prefer small, reversible changes
- keep edits aligned with current architecture
- add tests with behavior changes
- do not "simplify" by removing security boundaries

### Before editing

Review the relevant local context first:

- feature code
- associated interfaces/contracts
- DI registrations
- existing tests
- related security docs when applicable

### When editing

- preserve naming and file organization patterns already used nearby
- prefer extending an existing service/workflow over creating parallel logic
- keep code comments sparse and useful
- avoid speculative refactors unless they directly unblock the change

### After editing

- build the affected projects when feasible
- run the most relevant tests
- mention any unverified risk explicitly

## Suggested Near-Term Aspirations

These are consistent with the current repo direction and should guide future decisions:

- make the app a high-trust personal authenticator for Windows
- continue tightening secret-handling and redaction discipline
- deepen regression coverage around authorization, update, and import/export flows
- improve recovery and migration confidence without sacrificing local-first design
- keep release automation reproducible and auditable
- keep the UX polished while preserving explicit security boundaries

## Source Documents Worth Reading First

- [`readme.md`](E:/Repos/TOTP-Manager/readme.md)
- [`CONTRIBUTING.md`](E:/Repos/TOTP-Manager/CONTRIBUTING.md)
- [`docs/security/THREAT_MODEL.md`](E:/Repos/TOTP-Manager/docs/security/THREAT_MODEL.md)
- [`docs/security/SECURITY_VERIFICATION.md`](E:/Repos/TOTP-Manager/docs/security/SECURITY_VERIFICATION.md)
- [`docs/security/PENTEST_PLAN.md`](E:/Repos/TOTP-Manager/docs/security/PENTEST_PLAN.md)
- [`docs/security/AUTO_UPDATE.md`](E:/Repos/TOTP-Manager/docs/security/AUTO_UPDATE.md)
- [`docs/security/BRANCH_PROTECTION.md`](E:/Repos/TOTP-Manager/docs/security/BRANCH_PROTECTION.md)

## Bottom Line

Contribute as if this repository is trying to become a trustworthy desktop security product, because that is what the codebase already signals.

Optimize for:

- trust
- correctness
- maintainability
- safe iteration
- release confidence

Do not optimize for:

- shortcuts
- hidden magic
- superficial velocity
- convenience that weakens security posture
