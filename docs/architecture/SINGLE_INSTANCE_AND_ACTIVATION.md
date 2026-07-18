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

## Avalonia desktop implementation

The Avalonia host uses `NamedMutexInstanceLock` plus `NamedPipeActivationDispatcher`/`NamedPipeActivationListener` on Windows, macOS, and Linux. Both names include a stable hash of the current user so independent user sessions do not contend. The pipe is created with `PipeOptions.CurrentUserOnly`.

The transport reads exactly two bytes: activation schema version and kind. Unsupported values are ignored. The secondary process exits only after the primary accepts the dispatch; a failed redirect produces a non-zero exit code. The primary posts the accepted request to Avalonia's UI dispatcher, restores a minimized main window, and activates it. This action never changes authorization state, so an activated locked window remains locked.

The listener is owned by the Avalonia service provider and cancelled on application exit. Mutex ownership remains on the main startup thread until the classic desktop lifetime returns. Real transport/mutual-exclusion tests run in Windows tests and in the Ubuntu/macOS portable CI job.

## Security and compatibility review

- Threat impact: a secondary process cannot become primary while a live owner holds the lock. Stale or abandoned ownership cannot permanently deny startup.
- Data flow: the secondary process sends only a versioned activation intent to the platform dispatcher. No vault or authorization data is included.
- Compatibility: WPF retains its existing global mutex and native foreground activation. Avalonia uses a separate v2 identity during migration, so preview launches cannot redirect or suppress the release client.
- Test evidence: coordination tests cover primary, recovered, redirected, failed, repeated, invalid-payload, and disposal paths. Windows tests cover the native WPF foreground sequence and the Avalonia pipe/mutex transport; Ubuntu and macOS CI execute the same Avalonia transport against their real host implementations.
