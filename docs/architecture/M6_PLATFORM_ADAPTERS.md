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
