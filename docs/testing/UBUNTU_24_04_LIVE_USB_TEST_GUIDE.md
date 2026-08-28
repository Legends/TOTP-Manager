# Ubuntu 24.04 Live-USB Test Guide

Use only synthetic accounts and passwords. Because this is an unsigned preview, do not open or import your real authenticator vault.

## 1. Start Ubuntu

Boot the USB stick and select **Try Ubuntu**. Connect to the internet and open Terminal.

Confirm the system:

```bash
uname -m
lsb_release -ds
echo "$XDG_SESSION_TYPE"
echo "$XDG_CURRENT_DESKTOP"
```

Expected:

- Architecture: `x86_64`
- Ubuntu: `24.04`
- Usually GNOME on Wayland; some hardware or boot configurations may use X11

A normal live session loses application data after reboot. A persistent live USB retains it.

## 2. Download RC7

```bash
mkdir -p ~/Downloads/totp-rc7
cd ~/Downloads/totp-rc7

BASE_URL="https://github.com/Legends/TOTP-Manager/releases/download/v2.0.0-rc7"

wget "$BASE_URL/SHA256SUMS"
wget "$BASE_URL/totp-manager_2.0.0-rc7_amd64.deb"
```

Release page:

[TOTP Manager v2.0.0-rc7](https://github.com/Legends/TOTP-Manager/releases/tag/v2.0.0-rc7)

## 3. Verify the Download

Verify only the downloaded DEB package:

```bash
grep 'totp-manager_2.0.0-rc7_amd64.deb' SHA256SUMS | sha256sum --check
```

Expected:

```text
totp-manager_2.0.0-rc7_amd64.deb: OK
```

The checksum detects download corruption, but this remains an unsigned preview. The checksum is hosted with the same unsigned release.

## 4. Install the DEB Package

```bash
sudo apt update
sudo apt install ./totp-manager_2.0.0-rc7_amd64.deb
```

APT should resolve and install the package's declared runtime dependencies, potentially including packages such as `libsecret-tools`, `libgl1`, and GLib runtime libraries.

Confirm the installed package:

```bash
dpkg-query -W -f='${Package} ${Version} ${Architecture}\n' totp-manager
command -v totp-manager
```

Launch it from the application menu by searching for **TOTP Manager**, or run:

```bash
totp-manager
echo "Exit code: $?"
```

Confirm:

- No native-library errors appear
- The application icon is visible
- The window opens and is centered
- Closing and reopening works
- A normal user-initiated close returns exit code `0`

## 5. Verify Automatic Updates Are Disabled

```bash
python3 -c 'import json; print(json.load(open("/opt/totp-manager/appsettings.json"))["AutoUpdate"]["Enabled"])'
```

Expected:

```text
False
```

The preview must not offer or install automatic updates.

## 6. Test Vault Creation

Create a disposable vault using a unique test password.

Verify:

- Password confirmation and validation work
- Locking and unlocking work
- Closing and reopening the application within the same live session preserves the synthetic vault
- An incorrect password is rejected without exposing technical details

Do not reuse a real password.

## 7. Test Account Management

Add a synthetic account manually, for example:

- Issuer: `Test Service`
- Account: `demo@example.invalid`
- Secret: `JBSWY3DPEHPK3PXP`

Test:

- Add, edit, and delete
- Newly added account highlight and automatic scrolling
- Search and clearing search with Escape
- Right-click context menu without changing selection
- Generated TOTP and progress bar
- QR preview, enlargement, and closing with Escape
- Add/settings flyouts and Escape handling

Optionally compare the generated code with `oathtool`:

```bash
sudo apt install oathtool
oathtool --totp -b JBSWY3DPEHPK3PXP
```

## 8. Test QR Scanning

Create a synthetic QR code:

```bash
sudo apt install qrencode

qrencode -o ~/totp-test.png \
  'otpauth://totp/Test%20Service:camera@example.invalid?secret=JBSWY3DPEHPK3PXP&issuer=Test%20Service'
```

Display `~/totp-test.png` on another screen or phone and scan it with the Ubuntu system's camera.

Verify:

- Clicking the scan icon opens the scanner directly
- Camera preview appears
- The synthetic account is imported
- The imported row is highlighted and scrolled into view
- Escape closes the scanner
- Closing or locking during capture stops the camera
- No-camera and permission-denied states show understandable messages

If camera access fails, record whether it appears to be:

- An application defect
- A camera permission or configuration issue
- A camera not exposed by the live environment
- An unsupported camera or driver

## 9. Test Clipboard Behavior

Select a synthetic account and copy its TOTP code.

### On X11

- Verify the app-owned clipboard value clears after the configured timeout
- Copy unrelated text before the timeout and confirm the app does not erase it

### On Wayland

- Verify the application reports that safe conditional clipboard clearing is unavailable if the desktop cannot guarantee clipboard ownership

Never copy a real OTP during this test.

## 10. Test Locking and Single-Instance Behavior

Enable lock-on-session-lock, then lock Ubuntu with `Super+L`.

After returning:

- TOTP Manager should require its master password again
- No previous TOTP, QR code, or camera preview should remain visible

While the application is running, execute:

```bash
totp-manager
```

The existing window should activate instead of opening a second independent vault instance.

Live Ubuntu may have limited session-lock behavior if no login password is configured. Record that limitation if encountered.

## 11. Review Diagnostics

Application locations:

```text
Vault:       ~/.local/share/totp-manager
Preferences: ~/.config/totp-manager
Logs:        ~/.local/state/totp-manager/logs/app.log
```

Review the log:

```bash
less ~/.local/state/totp-manager/logs/app.log
```

Confirm it does not contain:

- Master passwords
- OTP seeds
- Generated OTP codes
- Clipboard contents
- QR payloads
- Secret Service values

Do not upload an unreviewed log.

After closing the application, confirm no process remains:

```bash
pgrep -a -f 'totp-manager|TOTP.UI.Avalonia.Desktop' \
  || echo "No TOTP Manager process remains"
```

This is especially useful after QR capture testing because it can reveal camera-related shutdown hangs.

## 12. Test Uninstall Behavior

Close TOTP Manager, then run:

```bash
sudo apt remove totp-manager
```

To also remove system-wide package configuration files:

```bash
sudo apt purge totp-manager
```

Confirm the program is removed:

```bash
command -v totp-manager || echo "Application removed"

dpkg-query -W totp-manager 2>/dev/null \
  || echo "Package no longer installed"

test -d ~/.local/share/totp-manager \
  && echo "Synthetic user data retained"
```

`apt remove` and `apt purge` normally leave user data under the home directory untouched.

On a non-persistent live USB, rebooting normally removes all session data.

## Portable Tarball Alternative

To test without installing the DEB:

```bash
cd ~/Downloads/totp-rc7

wget "$BASE_URL/TOTP-Manager-linux-x64-2.0.0-rc7.tar.gz"

grep 'TOTP-Manager-linux-x64-2.0.0-rc7.tar.gz' SHA256SUMS \
  | sha256sum --check

mkdir -p ~/Applications/totp-manager-rc7

tar -xzf TOTP-Manager-linux-x64-2.0.0-rc7.tar.gz \
  -C ~/Applications/totp-manager-rc7

cd ~/Applications/totp-manager-rc7
./TOTP.UI.Avalonia.Desktop
```

The DEB method is recommended for the primary Ubuntu test because it validates dependencies, desktop integration, installation, and removal.
