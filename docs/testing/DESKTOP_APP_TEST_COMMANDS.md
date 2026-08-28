# Desktop application test commands

Use the repository root as the working directory unless stated otherwise.

## Windows host — Avalonia desktop application

Run in **Windows PowerShell** from `E:\Repos\TOTP-Manager`:

```powershell
.\scripts\dev\Publish-And-Run-AvaloniaWindows.ps1 -Configuration Release -StopRunningInstance
```

This publishes and starts the Windows version of `TOTP.UI.Avalonia.Desktop`.

## Ubuntu Hyper-V VM — full Linux desktop test

Run in **Windows PowerShell** from `E:\Repos\TOTP-Manager` while the Ubuntu VM is running and its desktop user is signed in:

```powershell
.\scripts\testing\Sync-Restore-Run-AvaloniaLinuxVm.ps1
```

This packages the current Windows working tree, transfers it over SSH to the
VM-local repository, restores the Avalonia project, and starts it in the Ubuntu
desktop session. Build output, IDE state, Git metadata, and `artifacts` are
excluded from the transfer. The temporary archive is removed after
synchronization.

The default VM-local repository is `~/source/TOTP-Manager`. A pre-existing
shared mount can still be used explicitly when needed:

```powershell
.\scripts\testing\Sync-Restore-Run-AvaloniaLinuxVm.ps1 `
    -MountedRepository /mnt/totp-manager
```

The VM requires `openssh-server`, `rsync`, `tar`, and the .NET 9 SDK. Using an
SSH key avoids the two password prompts needed for upload and execution.

## Ubuntu WSL2/WSLg — fast Linux development test

Run in the **Ubuntu WSL shell**:

```bash
cd /mnt/e/Repos/TOTP-Manager
./scripts/testing/run-avalonia-wsl.sh
```

Alternatively, run the same WSL test directly from **Windows PowerShell** in the repository root:

```powershell
wsl.exe -d Ubuntu --cd /mnt/e/Repos/TOTP-Manager `
    bash ./scripts/testing/run-avalonia-wsl.sh
```

Use WSL/WSLg for quick development checks. Use the full Hyper-V VM for Linux desktop and end-user acceptance testing.
