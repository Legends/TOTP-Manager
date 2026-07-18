# Platform file security

## Scope

M1.2 places filesystem permission enforcement behind `IPlatformFileSecurity`. Windows uses protected current-user ACLs. macOS and Linux share a descriptor-based POSIX implementation that enforces the rules below.

## Windows behavior

- Sensitive application directories receive a protected ACL owned by the current user.
- The current user receives full control with container and object inheritance on directories.
- Sensitive files receive a protected ACL owned by the current user with full control granted only to that user.
- Missing paths, an unavailable user SID, and ACL application failures are errors. The adapter does not silently skip protection.

Vault, settings, encrypted export, and backup writes use a uniquely named, exclusively created staging file. The staging file is hardened before it replaces the live destination. If hardening fails, the operation returns a failure and preserves the previous live file. Reparse-point destinations are rejected.

## macOS and Linux behavior

The Unix adapter:

- Opens paths through platform-specific libc bindings with `O_NOFOLLOW`, `O_NONBLOCK`, and `O_CLOEXEC`; Linux and macOS ABI details remain isolated behind the internal filesystem seam.
- Creates application data, configuration, backup, state, and log directories with mode `0700`.
- Creates vault, encrypted settings, backups, and updater state with mode `0600`.
- Uses mode `0600` for application-managed export staging files. The selected export directory itself must not be re-permissioned.
- Verifies that sensitive files and directories are owned by the effective user.
- Rejects symbolic links and unexpected non-regular files at sensitive storage destinations.
- Applies and then verifies permissions; an unsuccessful `chmod`, ownership check, or type check is a hardening failure.
- Fails closed without falling back to a shared directory or plaintext persistence.

Log files must be user-only by default even though redaction remains mandatory.

## Threat impact

- Removes Windows ACL code from the persistence layer and makes OS policy replaceable without weakening DAL behavior.
- Closes the previous behavior where an ACL exception was logged but the write still reported success.
- Hardening before atomic replacement avoids replacing a protected live file with an unverified file.
- Permission enforcement reduces exposure to other local accounts. It does not protect against malware running as the same user or an already-compromised administrator account.
- Native helper binaries are not shipped. Using the runtime architecture's libc avoids loading an incompatible helper and keeps permission changes bound to the verified open descriptor.

## Data flow and compatibility

- Encryption, serialization, vault format, settings format, and storage locations are unchanged.
- Existing files are re-hardened before they are read. If the OS denies that operation, the existing access-denied result is returned and the file is not modified.
- Existing configured storage paths remain supported.
- Backup names and rotation depth remain unchanged.
- Linux x64 and macOS x64/ARM64 are supported by the current native adapters. Other Unix architectures fail closed until their ABI mapping and integration tests are added.

## Verification evidence

- Windows ACL tests verify protected current-user directory and file rules.
- The dedicated portable Unix test project verifies `0700`/`0600` modes, symbolic-link rejection, regular-file enforcement, and missing-path failures in an Ubuntu 24.04 and macOS ARM64 CI matrix.
- DAL regression tests inject hardening failures during reads and staged writes.
- Recovery tests verify that failed vault, settings, export, and backup hardening does not replace or rotate live data.
