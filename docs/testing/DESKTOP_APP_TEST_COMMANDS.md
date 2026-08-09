# Desktop application test commands

Use the repository root as the working directory unless stated otherwise.

## Windows host — WPF reference application

Run in **Windows PowerShell** from `E:\Repos\TOTP-Manager`:

```powershell
.\scripts\dev\Publish-And-Run-Wpf.ps1 -Configuration Release -StopRunningInstance
```

This publishes and starts the Windows WPF application used as the UI and behavior reference.

## Ubuntu Hyper-V VM — full Linux desktop test

Run in **Windows PowerShell** from `E:\Repos\TOTP-Manager` while the Ubuntu VM is running and its desktop user is signed in:

```powershell
.\scripts\testing\Sync-Restore-Run-AvaloniaLinuxVm.ps1
```

This synchronizes the mounted Windows working tree to the VM-local repository, restores the Avalonia project, and starts it in the Ubuntu desktop session.

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
