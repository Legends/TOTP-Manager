# Platform application paths

## Status

M1.1 establishes `IPlatformApplicationPaths` as the source of application filesystem locations. Windows, macOS, and Linux adapters implement the policy below without creating directories or weakening filesystem permissions. The Avalonia composition root selects its concrete adapter at build time for the host operating system and registers it as the single application-wide implementation.

## Path policy

| Asset | Windows | macOS | Linux |
|---|---|---|---|
| Packaged configuration | Executable directory, `appsettings.json` | Application bundle/resource directory | Executable/resource directory |
| Vault | `%AppData%\TOTP-Manager\master.totp` | `~/Library/Application Support/TOTP-Manager/master.totp` | `$XDG_DATA_HOME/totp-manager/master.totp` |
| Authorization envelope | `%AppData%\TOTP-Manager\authorization-envelope.bin` | `~/Library/Application Support/TOTP-Manager/authorization-envelope.bin` | `$XDG_DATA_HOME/totp-manager/authorization-envelope.bin` |
| Preferences | `%AppData%\TOTP-Manager\preferences.json` | `~/Library/Application Support/TOTP-Manager/preferences.json` | `$XDG_CONFIG_HOME/totp-manager/preferences.json` |
| Backups | Beside the vault for compatibility | `~/Library/Application Support/TOTP-Manager/Backups` | `$XDG_DATA_HOME/totp-manager/backups` |
| Logs | Executable directory, `Logs` | `~/Library/Logs/TOTP-Manager` | `$XDG_STATE_HOME/totp-manager/logs` |
| Update state | `%AppData%\TOTP-Manager\autoupdate-state.json` | `~/Library/Application Support/TOTP-Manager/autoupdate-state.json` | `$XDG_STATE_HOME/totp-manager/autoupdate-state.json` |

Linux adapters must use these fallbacks when the corresponding absolute XDG variable is absent:

- `XDG_CONFIG_HOME`: `~/.config`
- `XDG_DATA_HOME`: `~/.local/share`
- `XDG_STATE_HOME`: `~/.local/state`

Relative XDG locations are invalid and must not be accepted as application storage roots.

## Compatibility and migration

- The Windows adapter deliberately preserves every existing production location. M1.1 does not move or rewrite user data.
- Existing configuration overrides for the vault path remain supported and take precedence over adapter defaults.
- Windows backups remain adjacent to the configured vault. A later backup-policy extraction must retain discovery of those files.
- A future Windows-to-portable migration must discover the existing roaming-data files before creating a new layout. It must use atomic copy/verification and leave rollback data intact.
- Path comparisons and migration fixtures must not contain real secrets.

## Security impact

- This change alters path selection ownership, not encryption or storage formats.
- Platform adapters must fail closed when a required trusted user directory cannot be resolved. They must never fall back to the current working directory or plaintext storage.
- Directory creation and permission hardening remain filesystem boundary operations. M1.2 places OS-specific ACL policy behind `IPlatformFileSecurity`; Unix adapters must follow the requirements in `docs/security/PLATFORM_FILE_SECURITY.md`.
- Logs remain outside secret-bearing data files and must continue through the existing redaction pipeline.
