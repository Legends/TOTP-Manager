# M6 desktop platform adapters

## Baseline audit

M6 begins from a stronger baseline than the original checklist assumed. Earlier vertical slices already delivered and tested Windows v2 authorization storage, development-vault conflict handling, Hello-backed quick unlock with password recovery, ACL hardening, named-mutex/named-pipe activation, conditional clipboard handling, native dialogs, and shell integration. macOS application paths and Unix permission enforcement are implemented, as are Linux XDG paths and the fail-closed master-password fallback.

Those items are checked in the roadmap because their implementation and automated contract evidence are complete. Physical macOS/Linux behavior remains part of the postponed M3 target evidence and the M6 exit suite; checking an implementation item does not claim that target-host acceptance has run.

## Windows lifecycle lock slice

The Avalonia process now registers a Windows `IPlatformSessionEventSource` backed by `Microsoft.Win32.SystemEvents.SessionSwitch`. Lock, unlock/logon/connect, disconnect/logoff, and unknown reasons are mapped to the closed Core session-state enum. Subscription is idempotent and is removed during application shutdown.

`SessionLockPolicyBackgroundService` is now part of the Avalonia composition root and is explicitly started/stopped with the desktop lifetime. A configured Windows session lock first clears authorization state, then raises a metadata-free application-locked event. `MainWindowViewModel` marshals that event through `IUiScheduler` and clears account, OTP, QR, camera, tools, and authorized-shell state. This closes the prior gap where the key could be locked while secret-bearing presentation state remained visible.

Window minimization now delegates to the same view-model lock transition when the persisted `LockOnMinimize` policy is enabled. Unsaved settings edits do not alter the policy because the decision reads `ISettingsService.Current`, not the settings editor projection.

- Threat impact: Windows session lock and configured minimization both remove authorization and secret-bearing presentation output. A platform event cannot leave the UI visibly authorized after the vault key is cleared.
- Data-flow impact: only the closed session-state enum and a metadata-free lock event cross platform/presentation boundaries. No session identifier, username, desktop name, or exception message is retained or logged.
- Compatibility impact: WPF lifecycle behavior and storage formats are unchanged. Non-Windows builds receive an unavailable no-op session source until target-specific detection is implemented.
- Recovery impact: unlocking always returns through the existing password/optional quick-unlock recovery gate; session events never modify the authorization envelope.
- Test evidence: policy tests cover enabled/disabled behavior, non-lock states, source lifetime, UI notification, and sanitized lock failure logging. Main-window tests cover configured minimize lock and secret-surface teardown. The complete solution continues to compile without warnings.

Physical Windows session-switch acceptance remains required before the M6 exit criteria can be signed off.

## Windows signed update execution slice

The Windows Avalonia host now replaces the portable fail-closed installer placeholder with a Windows adapter. The adapter accepts only a regular ZIP package produced by the portable update service, opens it without write/delete sharing, enforces the same 128 MiB ceiling, and repeats Ed25519 package verification immediately before process handoff. A failed or malformed signature never reaches process execution.

The existing `TOTP.Updater` helper remains isolated from Avalonia and is bundled under the Windows desktop output. Before staging, the adapter requires a regular helper directory and executable and rejects reparse points anywhere in the copied helper bundle. It resolves the running executable only when it is directly inside the selected installation directory, constructs each helper argument through `ProcessStartInfo.ArgumentList`, and does not request application shutdown until the helper signals that its window is ready.

The production Avalonia feed remains disabled until M7 publishes and configures target-qualified Windows artifacts. Completing this adapter does not authorize the existing WPF appcast or an unqualified release artifact for Avalonia.

- Threat impact: a package modified after download verification, an unsigned package, a non-ZIP payload, a redirected helper bundle, or an executable outside the active installation directory fails closed before application shutdown.
- Data-flow impact: package bytes are read once more at the platform boundary and cleared after verification. Only local package/install paths and the parent process ID are passed to the bundled helper; none are exposed by the Avalonia view model or written to the application log.
- Compatibility impact: the WPF NetSparkle workflow, authorization envelope, vault, export, and appcast formats are unchanged. Windows Avalonia artifacts now include the same updater runtime in a dedicated subdirectory.
- Recovery impact: failure before helper readiness leaves the application running and reports the existing sanitized installer error. After readiness, the helper owns visible progress, in-place replacement, and relaunch.
- Test evidence: adapter tests cover signature revalidation, structured helper handoff followed by shutdown, rejection of non-ZIP payloads, and rejection of an executable outside the selected installation directory. Build output inspection confirms the helper executable is bundled.

Physical install/relaunch acceptance against a target-qualified signed Avalonia ZIP remains required before the M6 exit criteria can be signed off.
