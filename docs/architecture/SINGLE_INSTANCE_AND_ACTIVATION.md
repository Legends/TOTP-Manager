# Single instance and activation

M1.7 separates the decision to run or redirect from Windows mutex and foreground-window APIs.

## Portable policy

`SingleInstanceCoordinator` depends on two contracts from `TOTP.Core.Platform`:

- `IInstanceLock` reports `Acquired`, `Recovered`, or `AlreadyRunning`.
- `IActivationDispatcher` receives a versioned `ApplicationActivationRequest`.

The coordinator converts those platform results into a primary, recovered-primary, redirected, or redirect-failed startup outcome. It rejects repeated startup attempts and unsupported activation versions.

The current activation payload intentionally contains only a schema version and `ActivateMainWindow` intent. It does not forward command-line text, file contents, OTP seeds, passwords, or other arbitrary strings. Future payload fields must be bounded, validated, and treated as untrusted local input.

## Windows implementation

`WindowsNamedMutexInstanceLock` preserves the existing global named-mutex behavior. It attempts a non-blocking acquisition and distinguishes:

- a newly created mutex: primary instance;
- an existing but unlocked or abandoned mutex: recovered primary;
- a mutex owned by a live process: secondary instance.

Windows releases kernel handles when a process exits, so a normally crashed process may appear as a new acquisition. An abandoned owner thread and an unlocked surviving named object are both explicitly recovered. The implementation never waits indefinitely for an existing owner.

`WindowsExistingInstanceActivator` preserves the existing behavior for a secondary launch: locate visible windows owned by the existing process, prefer an unowned top-level window, restore it, and perform the foreground activation sequence. Windows-specific process enumeration and `user32.dll` calls remain in the WPF platform layer.

## Future platform implementations

- macOS should use an OS-managed application-instance mechanism or an atomic advisory lock, then deliver the versioned activation request through a same-user local channel before asking the application to activate.
- Linux should prefer the desktop application's D-Bus name as both ownership and activation transport. A same-user Unix domain socket plus an advisory lock is an acceptable fallback.
- File-based fallbacks must acquire an OS lock atomically. They must not delete a lock merely because its timestamp is old; owner liveness and lock ownership must be established first.
- Local activation transports must restrict access to the current user, validate the payload version and size, and reject unknown activation kinds.

## Security and compatibility review

- Threat impact: a secondary process cannot become primary while a live owner holds the lock. Stale or abandoned ownership cannot permanently deny startup.
- Data flow: the secondary process sends only a versioned activation intent to the platform dispatcher. No vault or authorization data is included.
- Compatibility: the mutex remains global and uses the existing `TOTP.UI.WPF` identity. Existing foreground restoration behavior is retained.
- Test evidence: coordination tests cover primary, recovered, redirected, failed, repeated, invalid-payload, and disposal paths. Windows tests cover fresh, live-owner, unlocked-stale, and abandoned-owner mutex states plus the native foreground sequence.
