# M8 automated hardening audit

## Scope and acceptance boundary

This audit closes the repository-verifiable portion of M8. It does not convert hosted-runner or headless evidence into physical acceptance. Interactive assistive-technology behavior, DPI/layout inspection, real camera timing, native permission prompts, clean-machine installation, extended wall-clock soak, credentialed release publication, and an independent penetration test remain explicit final gates.

## Security review

| Area | Automated evidence | Residual gate |
| --- | --- | --- |
| Threat model | `docs/security/THREAT_MODEL.md` covers both clients, native stores/session services, signed target-qualified feeds, artifact manifests, transactional update replacement, and package ownership | Revisit after external review and every security-significant release change |
| Quick unlock | Contract, enrollment, password-recovery, Windows TPM/Hello, macOS Keychain/LocalAuthentication, missing/reset key, cancellation, invalid metadata, cleanup, and key-zeroing tests | Physical Windows Hello and signed macOS user-presence prompts |
| Sensitive buffers | `SecurityContext`, `SensitiveBuffer`, password unwrap, QR/camera buffers, platform-store buffers, and updater copy buffers have owned clearing paths; a 1,000-cycle regression retains and verifies zeroed released DEK arrays | Framework string bindings for passwords/account secrets remain a documented managed-runtime limitation; inspect process memory during external testing |
| Clipboard | Ownership-aware Windows/macOS clearing, fail-closed Wayland policy, replacement preservation, retry, and disabled-policy tests | Physical X11/Wayland/macOS clipboard-manager and history behavior |
| Logging | Structured and rendered-text redaction tests cover key/value, JSON, URI query, bearer, exception, startup, and UI-boundary paths; security boundaries log types/codes rather than secret-bearing messages | Review sanitized logs from exact release artifacts |
| Storage and backup | Vault/envelope/preferences corruption, authentication failure, size bounds, atomic replacement, hardening failure, backup rotation rollback, and encrypted export round trips are covered. A published backup generation is copied back and read as the prior vault state | Physical recovery run using synthetic data and documented operator steps |
| Secure-store reset | Windows missing TPM key and verification failures, macOS missing/tampered Keychain binding, Linux missing session bus/item, and password recovery paths fail closed | Reset native stores on real Windows/macOS accounts |
| Update downgrade/replay | Only versions strictly greater than the running version are candidates. Equal-version replay, older-version downgrade, unknown channels, wrong targets, and stable/RC crossover policy are covered | Physical signed update and rollback exercise |
| Malicious update input | Invalid/tampered signatures, substituted payloads, HTTP URLs, DTDs, item floods, oversized feeds/files, interrupted downloads, reparse points, wrong formats, and cancelled/failed file transactions are rejected or rolled back | External assessment of signed release binaries and helper boundary |

No cryptographic, vault, authorization-envelope, preferences, import/export, or backup format changed in M8. The updater change affects application binary replacement only.

## Reliability evidence

- Startup: configured, first-run, quick-unlock fallback, retryable store failure, exception sanitization, early logging, and composition tests.
- Crash: UI/domain/task exception policies, fail-closed locking, non-zero shutdown, and abandoned single-instance ownership recovery.
- Camera: unavailable, permission denied, invalid native runtime, device loss, stall, cancellation, close/lock disposal, malformed payload, and secret-free error projection.
- Secure store: unavailable, denied, missing/reset, malformed reference, invalid recovered key, and recovery-password fallback.
- Update: disabled/managed modes, network failure, invalid feed/package, incomplete download cleanup, launch refusal, transactional copy failure, cancellation rollback, and successful replacement.
- Backup: encrypted export/import compatibility, rotating vault backups, failed-rotation preservation, and restoration of a published generation.
- Deterministic endurance: 10,000-account projection/filtering and 1,000 repeated unlock/lock key-buffer cycles. Extended wall-clock timer/idle-lock and camera/device soak remains physical acceptance work.

## Accessibility and UX evidence

- The real application resource tree and main-window XAML load under Avalonia Headless on Windows, Ubuntu, and macOS runners.
- The custom high-contrast dictionary uses the same typed theme variant as the runtime theme service; its previous string-key construction failure has a regression gate.
- Shared secret input, validation, notification, QR, symbol, and account-row controls have automation/name/live-region tests. Main XAML supplies heading levels and names for navigation, account search/list, generated code/timing, camera preview, and settings controls.
- English and German catalogs are complete for every declared Avalonia key and update dynamic resources in place.
- The account editor uses one short, non-repeating 180 ms right-edge entrance animation; it does not animate secret content independently or delay interaction. Reduced-motion behavior remains part of real-target accessibility acceptance because Avalonia's current cross-platform settings contract does not expose a system reduced-motion preference.
- Initial locales are left-to-right. RTL localization is explicitly deferred until an RTL locale enters product scope; no code claims that mirrored layout has been physically accepted.

Keyboard traversal/focus order, screen-reader announcements, high-contrast appearance, 100/150/200% DPI, and localization overflow require the real-target record template and remain open.

## Performance evidence

The checked-in Windows x64, Ubuntu x64, and macOS ARM64 package-probe reports enforce process-start, working-set, native-footprint, and 500/1,000/5,000-account filtering ceilings. A separate 10,000-account view-model test includes projection and search. These are regression budgets, not interactive UX measurements.

Cold/warm launch to a rendered password gate, unlocked steady-state memory, visible search latency, camera startup, and lock/unlock latency require exact release artifacts on physical targets. Record raw samples and p50/p95 values in `docs/architecture/evidence/M3_REAL_TARGET_RECORD_TEMPLATE.md`.
