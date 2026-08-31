# Single instance and activation

`SingleInstanceCoordinator` keeps startup policy independent of operating-system transport:

- `IInstanceLock` reports `Acquired`, `Recovered`, or `AlreadyRunning`.
- `IActivationDispatcher` sends a versioned `ApplicationActivationRequest`.

The desktop host uses `NamedMutexInstanceLock` with `NamedPipeActivationDispatcher` and `NamedPipeActivationListener` on Windows, macOS, and Linux. Names contain a stable current-user hash, and the pipe uses `PipeOptions.CurrentUserOnly`.

The activation payload is exactly two bytes: schema version and activation kind. It contains no command-line text, paths, file contents, OTP seeds, passwords, or other arbitrary data. Unsupported values are ignored. Future fields must remain bounded, validated, and treated as untrusted local input.

A secondary process exits only after the primary accepts the request; failed redirection produces a non-zero exit code. The primary posts accepted activation to Avalonia's UI dispatcher, restores a minimized main window, and activates it. Activation never changes authorization state, so a locked window remains locked.

The listener is cancelled during application shutdown. Mutex ownership stays with the main startup thread until the desktop lifetime returns. Tests cover primary, recovered, redirected, failed, repeated, invalid-payload, disposal, mutual-exclusion, and real transport paths across the supported CI hosts.
