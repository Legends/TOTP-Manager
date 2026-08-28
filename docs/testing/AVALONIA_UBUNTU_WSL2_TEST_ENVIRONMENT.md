# Testing TOTP Manager on Ubuntu with WSL2/WSLg

This guide creates a fast Ubuntu test loop for the Avalonia desktop application while the source remains in Visual Studio on Windows 11.

It is specific to this repository. Run commands from the repository root unless a section says otherwise.

## What this environment proves

WSL2/WSLg is appropriate for:

- compiling the Linux target with the Linux .NET SDK;
- launching the real `TOTP.UI.Avalonia.Desktop` process through WSLg;
- checking Avalonia layout, focus, keyboard behavior, dialogs, scaling, themes, and localization;
- running the Linux/Unix and Avalonia headless test projects;
- exercising password setup, unlock, account CRUD, QR generation, import/export, and single-instance behavior with synthetic data;
- probing the Linux OpenCV native runtime;
- producing a development-only Linux publish directory.

WSLg is not a complete Ubuntu desktop session. It is not authoritative evidence for:

- GNOME login, logout, session lock, suspend, resume, or autostart;
- a normal desktop Secret Service/keyring session;
- native Wayland behavior (Avalonia uses X11/XWayland by default);
- Linux clipboard ownership semantics on a normal X11 or Wayland desktop;
- camera hardware and permission behavior;
- `.deb` installation, desktop entries, package upgrades, or uninstall behavior;
- release acceptance, accessibility with a real desktop screen reader, or distribution support.

Use the WSL loop for daily development. Use the retained release candidate in a full Ubuntu 24.04 VM, live system, or physical installation for acceptance. See [M6 physical acceptance](../architecture/M6_PHYSICAL_ACCEPTANCE.md) and the [Ubuntu live-system guide](UBUNTU_24_04_LIVE_USB_TEST_GUIDE.md).

## Repository facts

- Windows checkout: `E:\Repos\TOTP-Manager`
- WSL view of that checkout: `/mnt/e/Repos/TOTP-Manager`
- Linux startup project: `TOTP.UI.Avalonia.Desktop/TOTP.UI.Avalonia.Desktop.csproj`
- Target framework: .NET 9
- Linux platform adapters: `TOTP.Platform.Linux` and `TOTP.Platform.Unix`
- Linux-specific tests: `TOTP.Tests.Unix`
- Cross-platform UI tests: `TOTP.Tests.Avalonia.Headless`
- NuGet configuration: `NuGet.config`

There is no `global.json`. The application targets .NET 9, but the build SDK must also be new enough for Avalonia 12.1's Roslyn-based source generator. Ubuntu 26.04 development should use the full .NET 10 SDK; CI and the Ubuntu 24.04 release baseline use the latest .NET 9 SDK feature band supplied by `actions/setup-dotnet`.

Do not use `dotnet build TOTP.sln` as the primary Linux command. The solution intentionally contains the Windows-only updater, Windows adapters, and Windows-targeted test project. Build the Avalonia project and Linux-capable tests explicitly.

## 1. Install or verify Ubuntu WSL2

Run the following in an elevated Windows PowerShell terminal:

```powershell
wsl.exe --install -d Ubuntu
wsl.exe --update
wsl.exe --shutdown
```

Restart Windows if requested. Do not use an elevated terminal for ordinary Ubuntu development after installation.

Verify the distribution:

```powershell
wsl.exe --list --verbose
```

The `Ubuntu` row must report version `2`. If it does not:

```powershell
wsl.exe --set-version Ubuntu 2
```

Always specify `-d Ubuntu` in Windows launch commands. On development machines with Docker Desktop, `docker-desktop` may be the default WSL distribution and is not the environment described here.

The distribution name on the current development machine is `Ubuntu`. If `wsl.exe --list --verbose` shows `Ubuntu-24.04` instead, substitute that exact name anywhere this guide uses `Ubuntu`.

## 2. Prepare Ubuntu

Ubuntu 26.04 is suitable for the daily WSL build and UI loop in this guide. It provides useful forward-compatibility coverage, but it does not replace the repository's Ubuntu 24.04 release baseline. CI, packaging, retained native evidence, and physical acceptance currently target Ubuntu 24.04.

### Open Ubuntu from Windows 11

Use any one of these methods. The PowerShell method is the least ambiguous because it explicitly selects Ubuntu instead of the current default `docker-desktop` distribution.

#### Method A: Windows PowerShell or Command Prompt (recommended)

1. Open a normal, non-administrator PowerShell or Command Prompt window.
2. Run:

   ```powershell
   wsl.exe -d Ubuntu
   ```

3. Wait until a Linux prompt appears. It normally resembles:

   ```text
   your-linux-user@your-computer:/mnt/c/Users/YourWindowsUser$
   ```

You are now typing commands inside Ubuntu. Do not include the displayed `$` prompt when copying commands from this guide.

#### Method B: Windows Start menu

1. Press the Windows key.
2. Type `Ubuntu`.
3. Select the installed **Ubuntu** application.

If Windows shows more than one Ubuntu version, choose the distribution whose name appeared in `wsl.exe --list --verbose`.

#### Method C: Windows Terminal

1. Open **Windows Terminal**.
2. Select the arrow next to the new-tab button.
3. Select **Ubuntu**.

Do not select the Docker Desktop profile.

### Complete the first launch

The first Ubuntu launch can take several minutes while WSL extracts the distribution. It then asks for a Linux account:

```text
Enter new UNIX username:  
New password:  
Retype new password:  
```

- Choose a Linux username; it does not need to match the Windows username.
- The Linux password is independent of the Windows password.
- Nothing appears while typing the password—not even `*` characters. This is expected.
- This password is required by `sudo` when installing packages.

When setup finishes, Ubuntu displays its shell prompt.

If launch instead reports `Wsl/Service/E_UNEXPECTED` or **Catastrophic failure**, Ubuntu did not start. Do not enter the Bash commands below into PowerShell. Follow the matching subsection under **Troubleshooting**, restart Windows if necessary, and return here after `wsl.exe -d Ubuntu` opens successfully.

### Confirm the Ubuntu release and architecture

Run the following commands at the Ubuntu shell prompt:

```bash
source /etc/os-release
printf '%s %s\n' "$NAME" "$VERSION_ID"
uname -m
whoami
pwd
```

For the currently installed WSL distribution, expected essentials are:

```text
Ubuntu 26.04
x86_64
admin77
```

Ubuntu 24.04 is also valid and remains the supported release target. `uname -m` must report `x86_64` for the current `linux-x64` package policy.

Your initial directory is inherited from the Windows process that launched WSL. Starting in `/mnt/c/Program Files/Microsoft Visual Studio/2022/Enterprise` is therefore normal. Move to the repository before running restore, build, test, or launch commands:

```bash
cd /mnt/e/Repos/TOTP-Manager
pwd
git status --short --branch
```

The expected `pwd` output is:

```text
/mnt/e/Repos/TOTP-Manager
```

Continue with Ubuntu 26.04 for daily development. Record it as forward-compatibility testing rather than Ubuntu 24.04 acceptance.

### Optional: install Ubuntu 24.04 alongside Ubuntu 26.04

Use a separate Ubuntu 24.04 distribution when you need local parity with the supported release target. Leave Ubuntu with `exit`, return to PowerShell, and inspect the available distributions:

```powershell
wsl.exe --list --online
```

Install the explicit Ubuntu 24.04 distribution when it is listed:

```powershell
wsl.exe --install -d Ubuntu-24.04
```

After installation, launch it with `wsl.exe -d Ubuntu-24.04` and substitute `Ubuntu-24.04` for `Ubuntu` in later Windows commands. Ubuntu 26.04 and Ubuntu 24.04 can remain installed side by side. Do not replace or unregister an existing distribution until any data in it has been reviewed and backed up.

### Install the required Ubuntu packages

The following commands are Bash commands. Run them inside the Ubuntu window, one block at a time:

Install development and native runtime prerequisites:

```bash
sudo apt update
sudo apt upgrade -y
sudo apt install -y \
  build-essential \
  ca-certificates \
  curl \
  git \
  libfontconfig1 \
  libglib2.0-0 \
  libgl1 \
  libice6 \
  libsecret-tools \
  libsm6 \
  libx11-6 \
  software-properties-common \
  unzip \
  x11-apps
```

These packages cover WSLg/Avalonia's X11 prerequisites and the native libraries declared by the repository's Linux package policy. `libsecret-tools` makes capability probing possible; its presence does not create a normal desktop keyring session.

## 3. Install a compatible .NET SDK

The application targets `net9.0`, but that does not require the command-line SDK itself to be version 9. Ubuntu 26.04 development should use the complete .NET 10 SDK from Ubuntu's feed. This matches the newer compiler used successfully by the Windows development environment and can target .NET 9. Because the application is framework-dependent, the .NET 9 runtime must also be installed to execute the resulting binary; the .NET 10 SDK does not include that older runtime.

If Ubuntu responds with `Command 'dotnet' not found` and suggests `sudo apt install dotnet-host-10.0`, do not use that exact suggestion. `dotnet-host-10.0` is only a host package; install `dotnet-sdk-10.0` instead.

Ubuntu 26.04 supplies .NET 9 through Canonical's .NET backports PPA:

```bash
sudo apt update
sudo apt install -y software-properties-common
sudo add-apt-repository -y ppa:dotnet/backports
sudo apt update
sudo apt install -y \
  dotnet-sdk-10.0 \
  dotnet-runtime-9.0
```

Verify:

```bash
dotnet --version
dotnet --info
dotnet --list-sdks
dotnet --list-runtimes
```

On Ubuntu 26.04, `dotnet --version` should now begin with `10.`, `dotnet --list-sdks` should include a `10.0.x` entry, and `dotnet --list-runtimes` must include `Microsoft.NETCore.App 9.0.x`. An installed `9.0.1xx` SDK may remain side by side, but it must not be the SDK selected for this Avalonia 12.1 build.

The first SDK invocation may print the standard ASP.NET Core HTTPS development-certificate message. TOTP Manager is a desktop application and does not use that certificate; no trust command is required.

The Ubuntu 24.04 release/CI baseline remains .NET 9. CI installs the latest `9.0.x` feature band rather than Canonical's older `9.0.1xx` SDK.

## 4. Verify WSLg

Inside Ubuntu:

```bash
printf 'DISPLAY=%s\nWAYLAND_DISPLAY=%s\nXDG_RUNTIME_DIR=%s\n' \
  "$DISPLAY" "$WAYLAND_DISPLAY" "$XDG_RUNTIME_DIR"
```

Values such as `DISPLAY=:0`, `WAYLAND_DISPLAY=wayland-0`, and `XDG_RUNTIME_DIR=/run/user/1000` indicate that WSLg supplied its display environment.

Check whether the optional test application is installed:

```bash
command -v xclock
```

If that command prints nothing, install the package inside Ubuntu:

```bash
sudo apt update
sudo apt install -y x11-apps
```

Then launch it:

```bash
xclock
```

Close `xclock` after its window appears. If it is installed but no window appears, run these commands in elevated Windows PowerShell and retry:

```powershell
wsl.exe --update
wsl.exe --shutdown
```

Also update the Windows GPU driver. WSLg GUI support requires WSL2.

## 5. Choose the working tree

### Daily Visual Studio workflow: shared Windows checkout

Use the existing checkout:

```bash
cd /mnt/e/Repos/TOTP-Manager
git status --short --branch
```

This lets Visual Studio and WSL use one working tree. Builds and file watching under `/mnt/e` can be slower, and NTFS does not reproduce normal Linux permission and case behavior exactly.

Set polling only for the current terminal when using `dotnet watch`:

```bash
export DOTNET_USE_POLLING_FILE_WATCHER=1
```

Do not add this globally unless every .NET watcher in the distribution should poll.

### Filesystem-fidelity workflow: WSL-native clone

For case-sensitivity, executable-bit, permission, and package testing, use a separate clone under the Linux filesystem:

```bash
mkdir -p ~/source
cd ~/source
git clone https://github.com/Legends/TOTP-Manager.git
cd TOTP-Manager
```

Do not copy a live vault or secret-bearing artifacts between the Windows and WSL checkouts. Commit code in one checkout and fetch it in the other.

## 6. Restore and build the Linux application

From `/mnt/e/Repos/TOTP-Manager` (or the WSL-native clone):

```bash
dotnet restore \
  TOTP.UI.Avalonia.Desktop/TOTP.UI.Avalonia.Desktop.csproj \
  --configfile NuGet.config

dotnet build \
  TOTP.UI.Avalonia.Desktop/TOTP.UI.Avalonia.Desktop.csproj \
  --configuration Debug \
  --no-restore
```

This project selects `TOTP_PLATFORM_LINUX`, the Linux/Unix adapters, and the Linux x64 OpenCvSharp runtime when MSBuild runs on Linux.

Do not pass a Windows runtime identifier and do not reuse a Windows publish directory.

## 7. Run native probes before opening the UI

Verify that the packaged OpenCV native runtime can load:

```bash
dotnet run \
  --project TOTP.UI.Avalonia.Desktop/TOTP.UI.Avalonia.Desktop.csproj \
  --configuration Debug \
  --no-build \
  -- --m3-native-probe
```

The command should exit with code `0` and no native probe failure. Optional measurement output:

```bash
dotnet run \
  --project TOTP.UI.Avalonia.Desktop/TOTP.UI.Avalonia.Desktop.csproj \
  --configuration Debug \
  --no-build \
  -- --m3-measurement-probe
```

These probes do not open a camera, vault, or GUI.

## 8. Use isolated synthetic test data

Do not test against a real authenticator vault. Give the WSL session isolated XDG roots before launching:

```bash
export TOTP_WSL_TEST_ROOT="$HOME/.local/share/totp-manager-wsl-test"
export XDG_CONFIG_HOME="$TOTP_WSL_TEST_ROOT/config"
export XDG_DATA_HOME="$TOTP_WSL_TEST_ROOT/data"
export XDG_STATE_HOME="$TOTP_WSL_TEST_ROOT/state"
export XDG_CACHE_HOME="$TOTP_WSL_TEST_ROOT/cache"
mkdir -p \
  "$XDG_CONFIG_HOME" \
  "$XDG_DATA_HOME" \
  "$XDG_STATE_HOME" \
  "$XDG_CACHE_HOME"
```

The application adds its own `totp-manager` subdirectory below these roots. Use only synthetic issuers, account names, seeds, passwords, QR codes, backups, and imports.

Before deleting test data, print and verify the exact root:

```bash
printf 'Synthetic test root: %s\n' "$TOTP_WSL_TEST_ROOT"
find "$TOTP_WSL_TEST_ROOT" -maxdepth 3 -print
```

Never point these variables at a real home directory or existing application-data root for cleanup.

## 9. Run the application

For the normal WSLg test loop, use the repository launcher. It applies a
project-local Avalonia scale of `2` so a WSLg scaling renegotiation does not
make the application miniature:

```bash
bash scripts/testing/run-avalonia-wsl.sh
```

The launcher does not change application code or global WSL configuration.
Override its scale for a different display when needed:

```bash
AVALONIA_GLOBAL_SCALE_FACTOR=1.5 \
  bash scripts/testing/run-avalonia-wsl.sh
```

To pass additional `dotnet run` options, append them to the command, for
example `--no-build`.

The original unscaled direct command, without the launcher, is:

```bash
dotnet run \
  --project TOTP.UI.Avalonia.Desktop/TOTP.UI.Avalonia.Desktop.csproj \
  --configuration Debug \
  --no-build
```

The TOTP Manager window should appear through WSLg with the stable product title, `TOTP Manager`.

For the rapid edit/test loop from the Windows-hosted checkout:

```bash
export DOTNET_USE_POLLING_FILE_WATCHER=1

dotnet watch \
  --project TOTP.UI.Avalonia.Desktop/TOTP.UI.Avalonia.Desktop.csproj \
  run \
  --configuration Debug
```

Stop with `Ctrl+C`. Some AXAML changes require a restart even when Hot Reload is available.

### Launch directly from Windows PowerShell

This command opens an Ubuntu shell in the correct repository:

```powershell
wsl.exe -d Ubuntu --cd /mnt/e/Repos/TOTP-Manager bash
```

To run the application directly:

```powershell
wsl.exe -d Ubuntu --cd /mnt/e/Repos/TOTP-Manager `
  bash scripts/testing/run-avalonia-wsl.sh
```

Use single quotes around the Bash command so PowerShell does not expand Bash variables.

## 10. Run Linux-capable tests

Run the Unix platform tests:

```bash
dotnet test \
  TOTP.Tests.Unix/TOTP.Tests.Unix.csproj \
  --configuration Debug
```

Run the real Avalonia XAML under the headless backend:

```bash
dotnet test \
  TOTP.Tests.Avalonia.Headless/TOTP.Tests.Avalonia.Headless.csproj \
  --configuration Debug
```

The main `TOTP.Tests` project targets Windows and is not the Linux test entry point. Continue running the normal Windows suite from Windows as documented in `AGENTS.md`.

## 11. WSLg interaction checklist

Use a synthetic vault and verify:

- [ ] First-run password setup opens.
- [ ] Enter from either password field activates **Create protected vault**.
- [ ] Password unlock works after restart.
- [ ] Add one account and verify the overview shows only the issuer.
- [ ] Verify selected and newly-added row colors match the Windows product palette, not the Ubuntu system accent.
- [ ] Edit the single account and verify the complete editor is visible without a scrollbar when screen space permits.
- [ ] Verify keyboard traversal reaches issuer, account name, secret, Cancel, and Save.
- [ ] Open delete confirmation and verify it has no minimize/close toolbar and is visually distinct from the main window.
- [ ] Verify Cancel/Escape does not delete and Delete removes only the selected synthetic entry.
- [ ] Switch English/German and check wrapping, dialog sizing, and clipping.
- [ ] Check 100%, 150%, and 200% Windows display scaling where WSLg exposes it usefully.
- [ ] Check dark, light, and high-contrast resource construction.
- [ ] Generate and copy a synthetic TOTP, then verify the UI's clipboard status.
- [ ] Generate a QR preview and close it with Escape.
- [ ] Launch a second WSL instance and verify the existing process is activated.
- [ ] Review sanitized support diagnostics and logs.

Do not treat WSLg clipboard, keyring, camera, accessibility, or session-lock observations as final Ubuntu acceptance.

## 12. Expected platform capability behavior under WSL

WSL commonly lacks a normal GNOME user session and unlocked Secret Service collection. The application should fail closed: report quick-unlock/secret-store capability as unavailable or misconfigured and keep the master password as the recovery path. Do not install or start ad hoc keyring services merely to turn that status green; that would test a custom environment rather than the supported Ubuntu desktop flow.

Likewise, lack of GNOME session-lock signals under WSL is expected. Test lock-on-session-lock on a full Ubuntu desktop using [M6 physical acceptance](../architecture/M6_PHYSICAL_ACCEPTANCE.md).

## 13. Publish a development Linux build

Do not rely only on `dotnet run`. Publish a self-contained Linux x64 development payload:

```bash
dotnet publish \
  TOTP.UI.Avalonia.Desktop/TOTP.UI.Avalonia.Desktop.csproj \
  --configuration Release \
  --runtime linux-x64 \
  --self-contained true \
  --output artifacts/wsl-publish/linux-x64
```

Run it:

```bash
./artifacts/wsl-publish/linux-x64/TOTP.UI.Avalonia.Desktop
```

Check the native probe from the published bytes:

```bash
./artifacts/wsl-publish/linux-x64/TOTP.UI.Avalonia.Desktop \
  --m3-native-probe
```

The `artifacts` directory is ignored by Git. A WSL publish is development evidence, not an accepted or signed release artifact.

For real tar/DEB assembly, follow [Avalonia desktop distribution](../architecture/AVALONIA_DESKTOP_DISTRIBUTION.md) and use `scripts/release/Package-AvaloniaLinux.ps1` on Ubuntu with PowerShell and Debian packaging tools installed. Never label an unsigned WSL build as a production release.

## 14. Troubleshooting

### `Wsl/Service/E_UNEXPECTED` or “Catastrophic failure”

This occurs before the Ubuntu process starts and is not an application build failure. In elevated Windows PowerShell, capture the non-secret status first:

```powershell
wsl.exe --status
wsl.exe --version
wsl.exe --list --verbose
```

Then try:

```powershell
wsl.exe --shutdown
wsl.exe --update
```

If Ubuntu still cannot start, close Docker Desktop and other WSL consumers and restart Windows. Check available memory and disk space before retrying. Follow [Microsoft's WSL troubleshooting guide](https://learn.microsoft.com/windows/wsl/troubleshooting) if the failure persists.

Do not run `wsl --unregister Ubuntu`: unregistering deletes the distribution and its data.

### The wrong WSL distribution opens

The current machine may default to `docker-desktop`. Always use:

```powershell
wsl.exe -d Ubuntu
```

Optionally make Ubuntu the default:

```powershell
wsl.exe --set-default Ubuntu
```

### The compatible SDK cannot be found

On Ubuntu 26.04, inspect the full SDK package rather than the host package:

```bash
apt-cache policy dotnet-sdk-10.0
```

Then repeat the SDK installation section. Do not substitute `dotnet-host-10.0`.

### `InitializeComponent` is missing and CS9057 mentions compiler 4.12 versus 4.14

This means the `9.0.1xx` SDK selected Roslyn 4.12 and Avalonia 12.1's source generator could not load because it requires Roslyn 4.14. Confirm and correct the selected SDK:

```bash
dotnet --version
dotnet --list-sdks
sudo apt update
sudo apt install -y dotnet-sdk-10.0
hash -r
dotnet --version
```

The final version must begin with `10.` on Ubuntu 26.04. Clean only the affected project, then restore and rebuild:

```bash
cd /mnt/e/Repos/TOTP-Manager
dotnet clean \
  TOTP.UI.Avalonia.Desktop/TOTP.UI.Avalonia.Desktop.csproj \
  --configuration Debug
dotnet restore \
  TOTP.UI.Avalonia.Desktop/TOTP.UI.Avalonia.Desktop.csproj \
  --configfile NuGet.config
dotnet build \
  TOTP.UI.Avalonia.Desktop/TOTP.UI.Avalonia.Desktop.csproj \
  --configuration Debug \
  --no-restore
```

### The Avalonia window does not appear

```bash
printf 'DISPLAY=%s WAYLAND_DISPLAY=%s\n' "$DISPLAY" "$WAYLAND_DISPLAY"
xclock
```

If `xclock` also fails, update and restart WSL from elevated PowerShell.

### A native library is missing

Run the M3 native probe and inspect only the library name/error type. Do not attach secret-bearing logs. Reinstall the prerequisite packages from section 2 and verify that restore selected the Linux runtime asset.

### Windows edits are not detected

```bash
export DOTNET_USE_POLLING_FILE_WATCHER=1
pwd
git status --short --branch
```

Restart `dotnet watch` and confirm both Visual Studio and WSL point to the same checkout.

### Builds under `/mnt/e` are slow

Use the WSL-native clone described above. Do not copy `bin`, `obj`, `artifacts`, vaults, logs, or backups between checkouts.

### Windows and Linux output appears stale

Clean only the affected Linux-capable projects:

```bash
dotnet clean TOTP.UI.Avalonia.Desktop/TOTP.UI.Avalonia.Desktop.csproj
dotnet clean TOTP.Tests.Unix/TOTP.Tests.Unix.csproj
dotnet clean TOTP.Tests.Avalonia.Headless/TOTP.Tests.Avalonia.Headless.csproj
```

Avoid broad recursive deletion commands against the repository. If a deeper cleanup is necessary, inspect each exact `bin`/`obj` target before removing it.

## 15. Daily command summary

Inside Ubuntu:

```bash
cd /mnt/e/Repos/TOTP-Manager
export DOTNET_USE_POLLING_FILE_WATCHER=1
export TOTP_WSL_TEST_ROOT="$HOME/.local/share/totp-manager-wsl-test"
export XDG_CONFIG_HOME="$TOTP_WSL_TEST_ROOT/config"
export XDG_DATA_HOME="$TOTP_WSL_TEST_ROOT/data"
export XDG_STATE_HOME="$TOTP_WSL_TEST_ROOT/state"
export XDG_CACHE_HOME="$TOTP_WSL_TEST_ROOT/cache"

dotnet watch \
  --project TOTP.UI.Avalonia.Desktop/TOTP.UI.Avalonia.Desktop.csproj \
  run \
  --configuration Debug
```

Before a handoff:

```bash
dotnet test TOTP.Tests.Unix/TOTP.Tests.Unix.csproj --configuration Debug
dotnet test TOTP.Tests.Avalonia.Headless/TOTP.Tests.Avalonia.Headless.csproj --configuration Debug
dotnet publish \
  TOTP.UI.Avalonia.Desktop/TOTP.UI.Avalonia.Desktop.csproj \
  --configuration Release \
  --runtime linux-x64 \
  --self-contained true \
  --output artifacts/wsl-publish/linux-x64
./artifacts/wsl-publish/linux-x64/TOTP.UI.Avalonia.Desktop --m3-native-probe
```

## Related repository documentation

- [Avalonia desktop distribution](../architecture/AVALONIA_DESKTOP_DISTRIBUTION.md)
- [M3 target validation](../architecture/M3_TARGET_VALIDATION.md)
- [M6 physical acceptance](../architecture/M6_PHYSICAL_ACCEPTANCE.md)
- [Platform application paths](../architecture/PLATFORM_APPLICATION_PATHS.md)
- [Security verification](../security/SECURITY_VERIFICATION.md)
- [Threat model](../security/THREAT_MODEL.md)

## Authoritative external references

- [Microsoft: Install WSL](https://learn.microsoft.com/windows/wsl/install)
- [Microsoft: Run Linux GUI apps with WSL](https://learn.microsoft.com/windows/wsl/tutorials/gui-apps)
- [Microsoft: WSL filesystem performance](https://learn.microsoft.com/windows/wsl/filesystems)
- [Microsoft: WSL file permissions](https://learn.microsoft.com/windows/wsl/file-permissions)
- [Microsoft: Choose a .NET installation source for Ubuntu](https://learn.microsoft.com/dotnet/core/install/linux-ubuntu-decision)
- [Avalonia: Desktop Linux and WSL2](https://docs.avaloniaui.net/docs/platform-specific-guides/linux)
