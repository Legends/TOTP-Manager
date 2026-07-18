# M3 target validation

## Automated package evidence

The `M3 package probe` matrix publishes the Avalonia desktop host for each technical target and executes `--m3-native-probe` from the publish directory. The probe loads the managed/native OpenCvSharp pair, constructs both QR-decoder and video-capture objects, and exits without opening a camera or touching vault state.

Targets:

- Windows x64 (`win-x64`)
- Ubuntu 24.04 x64 (`linux-x64`)
- macOS 14+ ARM64 (`osx-arm64`)
- macOS x64 (`osx-x64`) while Intel support remains under evaluation

Every matrix run records package byte size in the GitHub job summary. A manual `workflow_dispatch` additionally retains each framework-dependent validation package for seven days. These are unsigned technical artifacts, not releases.

This evidence proves restore, publish, native-asset placement, process startup, and native OpenCV loading on the target OS/architecture. It does not prove camera permission UX, camera-device behavior, accessibility, signing, notarization, installation, or update execution.

## Required real-target record

Complete one record per supported target using a packaged commit that passed the matrix. Record the commit, package RID, OS version, hardware, display scaling, desktop/session type, and tester.

- [ ] Launch to the password gate without secret-bearing diagnostics.
- [ ] Unlock a synthetic test vault and render 500 accounts.
- [ ] Filter by issuer and account name.
- [ ] Generate TOTP and verify timed clipboard behavior for the current display server.
- [ ] Generate and dispose a QR image.
- [ ] Start camera capture only after the explicit scan action.
- [ ] Record time to first preview and successful decode of a synthetic TOTP QR.
- [ ] Cancel capture and verify the device is released.
- [ ] Remove/disable the camera and verify typed recovery behavior.
- [ ] Repeat open/close 100 times and record handle/resource behavior.
- [ ] Verify native file picker and single-instance activation.
- [ ] Verify keyboard-only navigation and screen-reader announcements.
- [ ] Verify 100%, 150%, and 200% scaling where the platform supports them.

macOS packages used for real camera testing must include `NSCameraUsageDescription`. Signed/notarized artifacts and production update behavior remain M7 gates.

## Evidence policy

Do not mark a target complete from compilation alone. Attach a measurement report and the completed real-target record. Hardware- or permission-dependent rows remain open when only hosted CI evidence exists.
