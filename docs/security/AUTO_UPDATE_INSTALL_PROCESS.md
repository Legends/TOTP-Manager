# Auto-Update Install Process

The Avalonia desktop client owns update discovery, download progress, release notes, and explicit installation consent. `IPortableUpdateService` accepts only a signed `appcast-v2.xml`, selects an entry matching the current OS, architecture, channel, and distribution ownership, and verifies the downloaded package before it becomes installable.

An update check never downloads a package, and a completed download never starts installation without a separate user action.

## Windows installation handoff

The Windows host uses `WindowsUpdateInstallerLauncher` to hand a verified ZIP to the dedicated `TOTP.Updater` helper:

1. Accept only a regular ZIP package within the portable 128 MiB limit.
2. Hold the package without write/delete sharing and repeat Ed25519 verification.
3. Reject reparse points in the bundled updater runtime.
4. Copy the trusted helper runtime into a fresh current-user `%TEMP%` directory.
5. Start the helper with arguments supplied through `ProcessStartInfo.ArgumentList`.
6. Wait for the helper's ready signal before requesting graceful Avalonia shutdown.

The helper then:

1. Shows installation progress and signals readiness.
2. Waits for the parent TOTP process to exit.
3. Expands the ZIP into an isolated staging directory.
4. Copies existing destination files into a rollback directory before replacement.
5. Applies staged files with retry handling for transient file locks.
6. Restores prior files in reverse order if replacement fails or is cancelled.
7. Starts the updated Avalonia executable from the target directory after success.
8. Logs non-secret diagnostics to `%TEMP%\totp-update-helper.log` and cleans temporary staging.

Incomplete rollback is surfaced as a distinct failure and is never reported as a successful update.

Relevant code:

- `TOTP.Platform.Windows/WindowsUpdateInstallerLauncher.cs`
- `TOTP.Updater/UpdateInstallerService.cs`
- `TOTP.Updater/UpdateInstallerViewModel.cs`

## Other platforms and package ownership

The client consumes only `appcast-v2.xml`; target and stable/RC channel fields must match. Linux package-manager and future store packages disable application-owned updates. Direct Linux and macOS packages may verify and download matching artifacts but retain a manual platform handoff until a dedicated installer adapter is approved.

Unsigned RC packages have automatic updates disabled and do not receive an appcast.
