# Platform application paths

## Status

`IPlatformApplicationPaths` is the source of application filesystem locations. Windows, macOS, and Linux adapters implement the policy below without creating directories or weakening filesystem permissions. The Avalonia composition root selects the host implementation and registers it as the single application-wide instance.

## Path policy

| Asset | Windows | macOS | Linux |
|---|---|---|---|
| Packaged configuration | Executable directory, `appsettings.json` | Application bundle/resource directory | Executable/resource directory |
| Vault | `%AppData%\TOTP-Manager\master.totp` | `~/Library/Application Support/TOTP-Manager/master.totp` | `$XDG_DATA_HOME/totp-manager/master.totp` |
| Authorization envelope | `%AppData%\TOTP-Manager\authorization-envelope.bin` | `~/Library/Application Support/TOTP-Manager/authorization-envelope.bin` | `$XDG_DATA_HOME/totp-manager/authorization-envelope.bin` |
| Preferences | `%AppData%\TOTP-Manager\preferences.json` | `~/Library/Application Support/TOTP-Manager/preferences.json` | `$XDG_CONFIG_HOME/totp-manager/preferences.json` |
| Rotating vault backups | Beside the vault (`master.totp.bak1`…`.bak5`) | Beside the vault (`master.totp.bak1`…`.bak5`) | Beside the vault (`master.totp.bak1`…`.bak5`) |
| Logs | Executable directory, `Logs` | `~/Library/Logs/TOTP-Manager` | `$XDG_STATE_HOME/totp-manager/logs` |
| Update state | `%AppData%\TOTP-Manager\autoupdate-state.json` | `~/Library/Application Support/TOTP-Manager/autoupdate-state.json` | `$XDG_STATE_HOME/totp-manager/autoupdate-state.json` |

Linux adapters must use these fallbacks when the corresponding absolute XDG variable is absent:

- `XDG_CONFIG_HOME`: `~/.config`
- `XDG_DATA_HOME`: `~/.local/share`
- `XDG_STATE_HOME`: `~/.local/state`

Relative XDG locations are invalid and must not be accepted as application storage roots.

## Compatibility and migration

- The Windows adapter deliberately preserves the established production locations and does not move or rewrite user data.
- Existing configuration overrides for the vault path remain supported and take precedence over adapter defaults.
- Rotating vault backups remain adjacent to the configured vault on every platform. The platform `BackupDirectory` property is reserved for future backup-policy extraction and must not be presented as the current rotating-backup location.
- A future Windows-to-portable migration must discover the existing roaming-data files before creating a new layout. It must use atomic copy/verification and leave rollback data intact.
- Path comparisons and migration fixtures must not contain real secrets.

## Security impact

- This change alters path selection ownership, not encryption or storage formats.
- Platform adapters must fail closed when a required trusted user directory cannot be resolved. They must never fall back to the current working directory or plaintext storage.
- Directory creation and permission hardening remain filesystem boundary operations. OS-specific ACL policy sits behind `IPlatformFileSecurity`; Unix adapters must follow [the platform file-security policy](../security/PLATFORM_FILE_SECURITY.md).
- Logs remain outside secret-bearing data files and must continue through the existing redaction pipeline.
