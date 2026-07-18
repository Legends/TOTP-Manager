# M3 target validation

## Automated package evidence

The `M3 package probe` matrix publishes the Avalonia desktop host, assembles a target-shaped technical package, and executes `--m3-native-probe` from that package. The probe loads the managed/native OpenCvSharp pair, constructs both QR-decoder and video-capture objects, and exits without opening a camera or touching vault state. macOS uses an `.app` bundle whose `Info.plist` declares `NSCameraUsageDescription`, minimum macOS 14, high-resolution rendering, bundle identity, and executable metadata; CI validates the plist before probing the bundled executable.

Targets:

- Windows x64 (`win-x64`)
- Ubuntu 24.04 x64 (`linux-x64`)
- macOS 14+ ARM64 (`osx-arm64`)

macOS x64 was evaluated and excluded from the initial product policy. Its aligned OpenCvSharp 4.13 package restored and published successfully but failed at the first native runtime call on GitHub's `macos-15-intel` runner; the ARM64 package passed. The failed probe is retained in workflow run `29645841617`. This is a support-scope decision, not a claim that Intel camera behavior works.

Every matrix run records package byte size in the GitHub job summary. A manual `workflow_dispatch` additionally retains each framework-dependent validation package for seven days. These are unsigned technical artifacts, not releases.

The first complete three-target package and budget matrix passed for commit `2846f09` in workflow run `29645981850`. Later runs remain authoritative for later commits; the commit recorded inside each JSON report prevents evidence from being attributed to a different build.

This evidence proves restore, publish, native-asset placement, process startup, and native OpenCV loading on the target OS/architecture. It does not prove camera permission UX, camera-device behavior, accessibility, signing, notarization, installation, or update execution.

Automated thresholds are defined in [`M3_MEASUREMENT_BUDGETS.md`](M3_MEASUREMENT_BUDGETS.md).

## Required real-target record

Complete one copy of [`evidence/M3_REAL_TARGET_RECORD_TEMPLATE.md`](evidence/M3_REAL_TARGET_RECORD_TEMPLATE.md) per supported target using a packaged commit that passed the matrix. The package SHA-256 binds interactive results to the tested bytes; the commit and workflow run bind them to automated evidence.

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

The preview window is vertically scrollable at its minimum size and declares automation names for otherwise ambiguous inputs, account content, camera/QR images, and generated-code output. Status and error text use polite or assertive live-region semantics. These establish a testable accessibility path; they do not replace the target screen-reader and scaling rows above.

macOS packages used for real camera testing must include `NSCameraUsageDescription`. Signed/notarized artifacts and production update behavior remain M7 gates.

## Evidence policy

Do not mark a target complete from compilation alone. Attach a measurement report and the completed real-target record. Hardware- or permission-dependent rows remain open when only hosted CI evidence exists.
