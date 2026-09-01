# M6 physical platform acceptance

Automated implementation is not physical acceptance. Record the artifact hash, OS version, desktop/session type, hardware, result, and any screenshots or sanitized diagnostics for every run. Never attach a real vault, OTP seed, password, Keychain item value, or Secret Service value.

## Windows 10/11 x64 checklist

Use the exact retained Authenticode-signed self-contained ZIP candidate. The framework-dependent fast ZIP may be checked separately on a machine with the .NET 9 desktop runtime, but it is not the appcast update payload.

1. Verify the SHA-256 against the signed release manifest and verify Authenticode on both the application and bundled updater.
2. Extract to a clean user-writable directory and launch. Confirm the taskbar/window icon, startup, single-instance activation, and password setup with synthetic data.
3. Install a newer signed local candidate through the update UI. Confirm visible updater handoff, application relaunch, version change, and preservation of the synthetic vault.
4. Repeat with an intentionally obstructed disposable application copy. Confirm the updater reports failure and the prior application files remain launchable; do not induce failure in a real vault-bearing install.
5. Confirm lock-on-session-lock, clipboard ownership clearing, camera permission/no-device behavior, and sanitized logs.

## Ubuntu 24.04 live-stick checklist

Use the retained self-contained `linux-x64` tarball or DEB candidate.

1. Verify the SHA-256 hash against the retained build record.
2. Launch from the desktop entry and from a terminal; confirm no native-library error.
3. Create a synthetic vault, restart, and verify password unlock and account CRUD.
4. Open support diagnostics. Record Secret Service, session lock, clipboard, camera, and installer capability states plus `echo $XDG_SESSION_TYPE` and `$XDG_CURRENT_DESKTOP` separately.
5. Copy a synthetic OTP. On X11, verify timed clearing only clears the app-owned value and preserves a replacement value. On Wayland, verify the UI explicitly reports safe conditional clearing unavailable.
6. Start camera scanning. Verify no-device, denied-permission, and successful synthetic QR behavior as the hardware/session permits; close/lock during capture and confirm the preview stops.
7. Enable lock-on-session-lock, lock Ubuntu with the desktop action, unlock the OS session, and confirm the app requires its master password and shows no previous OTP/QR/camera output.
8. Launch a second instance and confirm the existing window activates while remaining locked or unlocked exactly as it was.
9. For the DEB, install, launch, close, upgrade/reinstall the same synthetic candidate, and uninstall. Confirm the user vault under XDG data paths is neither packaged nor removed by package uninstall.
10. Review logs for synthetic identifiers only. Confirm no password, OTP seed, OTP code, clipboard value, D-Bus payload metadata, or filesystem secret path appears.

## macOS 14+ ARM64 checklist

Use the exact Developer ID signed, notarized, and stapled DMG candidate.

1. Verify the SHA-256 hash, `codesign --verify --deep --strict`, `spctl --assess`, and `xcrun stapler validate`.
2. Mount the DMG, copy the app to Applications, launch through Finder, and confirm Gatekeeper accepts it without bypass instructions.
3. Create a synthetic vault and enable quick unlock using the recovery password. Confirm the system LocalAuthentication prompt appears and cancellation leaves password recovery available.
4. Restart and verify quick unlock through Touch ID, Apple Watch, or the macOS password fallback available on that Mac. Delete/reset the Keychain item and confirm safe password fallback.
5. Deny and then grant camera access in System Settings. Confirm the app distinguishes permission denied from no camera and can scan a synthetic QR after authorization.
6. Verify timed clipboard clearing preserves a value copied by another application after OTP Harbor's copy.
7. Enable lock-on-session-lock, lock/unlock macOS, and confirm authorization plus every secret-bearing presentation surface is cleared.
8. Launch a second instance and confirm activation of the existing app without unlocking it.
9. Move the app between Applications and a user folder and repeat Keychain/relaunch checks to expose signing identity or designated-requirement mistakes.
10. Review sanitized diagnostics/logs and record capability states without including paths or secret material.
