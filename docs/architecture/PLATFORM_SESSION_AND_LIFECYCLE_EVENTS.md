# Platform session events

Operating-system event delivery is separated from the product decision to lock the vault.

## Portable contract

`TOTP.Core.Platform` defines `IPlatformSessionEventSource`, which publishes `Active`, `Locked`, `Disconnected`, or `Unknown` session state.

Consumers call `Start` and `Stop` explicitly. `IsSupported` lets a host report that reliable delivery is unavailable. Event sources report platform facts only; they do not read settings or modify authorization state. A previous unused suspend/resume abstraction was removed rather than retained without an active lock policy.

## Windows adapter

`WindowsSessionEventSource` maps `SystemEvents.SessionSwitch` into the portable session states. Subscription is idempotent.

Only `SessionLock` maps to `Locked`. Disconnect is observable but does not implicitly lock the application.

## Product lock policy

`SessionLockPolicyBackgroundService` subscribes to the portable session source. It locks the authorization service only when:

1. the platform reports `Locked`; and
2. `LockOnSessionLock` is enabled.

Expected event-source failures remain startup failures so the host does not silently claim session monitoring is active. Authorization failures are caught and logged without including secret material.

## Equivalent platform semantics

Current adapters use these mappings where the platform can report them reliably:

| Portable state | macOS | Linux |
| --- | --- | --- |
| `Locked` | polled screen-lock state | recognized ScreenSaver `ActiveChanged(true)` signal |
| `Active` | polled screen-unlock state | recognized ScreenSaver `ActiveChanged(false)` signal |
| `Disconnected` | not emitted by the current adapter | not emitted by the current adapter |

An adapter must report `IsSupported == false` when its desktop/session environment cannot provide reliable events. It must not infer a lock event solely from application focus loss.

## Security and compatibility review

- Threat impact: session-lock enforcement remains enabled by default and is no longer coupled to Windows event argument types.
- Data flow: OS event -> platform adapter -> portable state -> lock policy -> authorization lock. No seed, password, or derived-key data crosses the event boundary.
- Compatibility: no settings or storage formats change. Windows `SessionLock` behavior and the `LockOnSessionLock` preference retain their existing meaning.
- Test evidence: policy tests cover enabled, disabled, irrelevant-state, failure, and subscription-lifetime branches. Platform tests cover supported lock/unlock and ignored or unavailable states.
