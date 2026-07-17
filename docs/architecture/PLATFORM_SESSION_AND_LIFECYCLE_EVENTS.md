# Platform session and lifecycle events

M1.6 separates operating-system event delivery from the product decision to lock the vault.

## Portable contract

`TOTP.Core.Platform` defines two independently managed event sources:

- `IPlatformSessionEventSource` publishes `Active`, `Locked`, `Disconnected`, or `Unknown` session state.
- `IPlatformLifecycleEventSource` publishes `Suspending` and `Resumed` lifecycle state.

Consumers call `Start` and `Stop` for the source they use. `IsSupported` lets a host explicitly report that reliable delivery is unavailable. Event sources report platform facts only; they do not read settings or modify authorization state.

## Windows adapter

`WindowsPlatformEventSource` is the Windows implementation. It maps `SystemEvents.SessionSwitch` into the portable session states and `SystemEvents.PowerModeChanged` into lifecycle states. Session and lifecycle subscriptions are independent and idempotent.

The existing Windows behavior is preserved exactly: only `SessionLock` maps to `Locked`. Disconnect and suspend are observable but do not implicitly lock the application.

## Product lock policy

`SessionLockPolicyBackgroundService` subscribes to the portable session source. It locks the authorization service only when:

1. the platform reports `Locked`; and
2. `LockOnSessionLock` is enabled.

Expected event-source failures remain startup failures so the host does not silently claim session monitoring is active. Authorization failures are caught and logged without including secret material.

## Equivalent platform semantics

Future adapters should use these mappings:

| Portable state | macOS | Linux |
| --- | --- | --- |
| `Locked` | screen/session locked notification | login1 `Lock` signal or desktop lock notification |
| `Active` | screen/session unlocked notification | login1 `Unlock` signal or desktop unlock notification |
| `Disconnected` | user session logout or fast-user-switch loss where reliably available | login1 session removal or inactive/disconnected session |
| `Suspending` | workspace will-sleep notification | login1 `PrepareForSleep(true)` |
| `Resumed` | workspace did-wake notification | login1 `PrepareForSleep(false)` |

An adapter must report `IsSupported == false` when its desktop/session environment cannot provide reliable events. It must not infer a lock event solely from application focus loss.

## Security and compatibility review

- Threat impact: session-lock enforcement remains enabled by default and is no longer coupled to Windows event argument types.
- Data flow: OS event -> platform adapter -> portable state -> lock policy -> authorization lock. No seed, password, or derived-key data crosses the event boundary.
- Compatibility: no settings or storage formats change. Windows `SessionLock` behavior and the `LockOnSessionLock` preference retain their existing meaning.
- Test evidence: policy tests cover enabled, disabled, irrelevant-state, failure, and subscription-lifetime branches. Windows mapping tests cover lock, unlock, connection, disconnection, suspend, resume, and ignored events.
