# Third-Party Notices

This project uses third-party software and assets.

Important:
- This file is an inventory/notice aid and not legal advice.
- You are responsible for validating licenses and distribution obligations before release.
- If any listed component license conflicts with your distribution model, remove/replace it.

## NuGet Dependencies (Runtime)

From the current project files:
- `TOTP/TOTP.UI.WPF.csproj`
- `TOTP.Core/TOTP.Core.csproj`
- `TOTP.Infrastructure/TOTP.Infrastructure.csproj`
- `TOTP.DAL/TOTP.DAL.csproj`
- `TOTP.UI.Avalonia.Shared/TOTP.UI.Avalonia.Shared.csproj`
- `TOTP.UI.Avalonia.Desktop/TOTP.UI.Avalonia.Desktop.csproj`
- `TOTP.Platform.Windows/TOTP.Platform.Windows.csproj`
- `TOTP.Platform.MacOS/TOTP.Platform.MacOS.csproj`
- `TOTP.Platform.Linux/TOTP.Platform.Linux.csproj`
- `TOTP.Platform.Unix/TOTP.Platform.Unix.csproj`

Components include (non-exhaustive):
- `Avalonia`
- `Avalonia.Desktop`
- `Avalonia.Themes.Fluent`
- `FluentResults`
- `Microsoft.Extensions.*`
- `Microsoft.Xaml.Behaviors.Wpf`
- `Notification.Wpf`
- `Newtonsoft.Json`
- `OpenCvSharp4`, `OpenCvSharp4.Extensions`, `OpenCvSharp4.runtime.win`
- `Otp.NET`
- `QRCoder`
- `Serilog.*`
- `SharpVectors`
- `Syncfusion.*` (commercial licensing applies)
- `System.Drawing.Common`
- `ZXing.Net`
- `NSec.Cryptography`

Action required:
- Confirm each package license from upstream and keep a record in release artifacts.

## NuGet Dependencies (Test-Only)

From `TOTP.Tests/TOTP.Tests.csproj`:
- `AutoFixture.AutoMoq`
- `coverlet.collector`
- `FluentAssertions`
- `Microsoft.NET.Test.Sdk`
- `Moq`
- `Moq.AutoMock`
- `xunit.*`
- `Xunit.StaFact`

These are normally non-redistributed (dev/test scope), but still require internal compliance tracking.

## Commercial Components

- `Syncfusion.*` packages are used by the WPF UI.
- Ensure your team holds valid Syncfusion licensing for build and distribution use cases.
- Keep proof of entitlement and follow Syncfusion redistribution terms.

## Asset Provenance

Asset folders requiring provenance verification:
- `TOTP/Assets/Icons`
- `TOTP/Assets/backgrounds`

Action required:
- For each non-original asset, record source URL, author, and license.
- Remove or replace any asset without clear redistribution rights.

## Removed Binary Artifacts

The following decompiled/re-signed third-party artifacts were removed from version control:
- `Moq.AutoMock.signed.dll`
- `Moq.AutoMock.il`
- `Moq.AutoMock.res`
- `Moq.AutoMock.Resources.Strings.resources`

These files should not be committed again.
