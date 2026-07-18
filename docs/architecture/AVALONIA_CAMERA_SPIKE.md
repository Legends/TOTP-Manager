# Avalonia camera QR spike gate

## Status

Camera QR scanning remains the last uncompleted interactive M3 slice. The existing runner is testable but lives in the WPF project with Windows-only native OpenCV packaging. Moving only its view would preserve a hidden Windows dependency and is not acceptable.

## Required module boundary

Create a UI-neutral `TOTP.Camera.OpenCv` project that owns:

- `QrScannerRunner`
- the video-capture adapter and factory
- frame preview encoding
- QR decoding
- all `OpenCvSharp` types and disposal rules

`TOTP.Core.IQrScannerRunner` remains the UI-facing contract. Avalonia may receive encoded preview bytes and decoded text, but must not reference `Mat`, `VideoCapture`, `QRCodeDetector`, or native runtime APIs. Camera selection, permission errors, cancellation, and device loss must remain typed boundary outcomes rather than UI exception parsing.

## Runtime packaging decision

Keep the managed `OpenCvSharp4` package and every native runtime on exactly the same version. The repository currently uses 4.11; adopting newer runtime packages requires one coordinated dependency upgrade with WPF regression testing.

The supported package families are:

- Windows x64: `OpenCvSharp4.runtime.win`
- Linux x64: `OpenCvSharp4.official.runtime.linux-x64` (portable manylinux build; do not adopt the deprecated Ubuntu-version-specific packages)
- macOS ARM64: `OpenCvSharp4.runtime.osx.arm64`
- macOS x64, if retained by product policy: `OpenCvSharp4.runtime.osx.x64`

References: [OpenCvSharp NuGet installation/runtime matrix](https://www.nuget.org/packages/OpenCvSharp4), [Windows runtime package and runtime-family notes](https://www.nuget.org/packages/OpenCvSharp4.runtime.win/).

Runtime packages belong in the desktop host's OS/architecture-conditional packaging graph, not Core, Infrastructure, Shared UI, or a portable camera logic assembly. CI restore/build is necessary but not sufficient because camera devices are unavailable on hosted runners.

## Security and lifecycle requirements

- Request camera permission only when the user starts scanning.
- Add the required macOS camera usage description before a signed test package is exercised.
- Treat decoded QR text as untrusted input and pass it through the existing strict `otpauth` parser and account-validation workflow.
- Never log decoded payloads, preview frames, account seeds, or device-specific identifiers.
- Bound preview resolution and encoded-frame size.
- Dispose detector, frames, and capture handles on success, cancellation, device loss, window close, and application shutdown.
- Stop capture while the vault is locked or the scanner window is hidden.

## Completion evidence

The M3 camera checkbox may be marked complete only when:

- the extracted runner's deterministic failure/cancellation/disposal tests pass;
- an Avalonia scanner view has no OpenCV types in its view model;
- Windows x64, macOS ARM64, and Linux x64 packages load their native runtime;
- a real-device smoke test records camera start, first preview, successful decode, cancellation, device loss, and repeated open/close without leaked capture handles;
- decoded data reaches the existing validated import workflow without being logged.
