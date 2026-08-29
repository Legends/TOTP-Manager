# Third-Party Notices

This project uses third-party software and assets.

Important:
- This file is an inventory/notice aid and not legal advice.
- Runtime packages are accepted only when their source and redistribution terms are compatible with the project's MIT distribution and the SignPath Foundation OSS conditions.
- Dependency-review and vulnerability checks run in CI. License and asset provenance still require maintainer review when a dependency or asset changes.

## NuGet Dependencies (Runtime)

From the current project files:
- `TOTP.UI.Avalonia.Desktop/TOTP.UI.Avalonia.Desktop.csproj`
- `TOTP.Core/TOTP.Core.csproj`
- `TOTP.Infrastructure/TOTP.Infrastructure.csproj`
- `TOTP.DAL/TOTP.DAL.csproj`
- `TOTP.UI.Avalonia.Shared/TOTP.UI.Avalonia.Shared.csproj`
- `TOTP.UI.Avalonia.Desktop/TOTP.UI.Avalonia.Desktop.csproj`
- `TOTP.Platform.Windows/TOTP.Platform.Windows.csproj`
- `TOTP.Platform.MacOS/TOTP.Platform.MacOS.csproj`
- `TOTP.Platform.Linux/TOTP.Platform.Linux.csproj`
- `TOTP.Platform.Unix/TOTP.Platform.Unix.csproj`

Direct runtime components include:
- `Avalonia`
- `Avalonia.Desktop`
- `Avalonia.Themes.Fluent`
- `FluentResults`
- `Microsoft.Extensions.*`
- `Microsoft.Xaml.Behaviors.Wpf`
- `Notification.Wpf`
- `Newtonsoft.Json`
- `OpenCvSharp4` and the Windows, Linux x64, and macOS ARM64 OpenCvSharp runtime packages (Apache-2.0)
- `Otp.NET`
- `QRCoder`
- `Serilog.*`
- `SharpVectors`
- `System.Drawing.Common`
- `ZXing.Net`
- `NSec.Cryptography` (MIT)

The Avalonia, FluentResults, Microsoft.Extensions, Microsoft.Win32.SystemEvents, NSec.Cryptography, Otp.NET, and QRCoder dependencies are MIT-licensed. OpenCvSharp and Serilog packages are Apache-2.0-licensed. Exact resolved transitive dependencies are recorded by restore/build artifacts and reviewed by GitHub dependency review.

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

These are normally non-redistributed (dev/test scope), but still require repository license compatibility. `FluentAssertions` is pinned to the Apache-2.0-licensed 7.x line; version 8 or later must not be introduced because its community/commercial license is not OSI-approved. Moq is BSD-3-Clause; the remaining current test packages use MIT or Apache-2.0 terms.

## Asset provenance

All embedded raster assets were created specifically for this repository or rendered locally from public-domain flag geometry. Their creation method, licensing declaration, and reviewed hashes are recorded in [`docs/assets/ASSET_PROVENANCE.md`](docs/assets/ASSET_PROVENANCE.md). Unknown earlier icon and flag files were replaced and are not distributed by the current tree.

## Removed Binary Artifacts

The following decompiled/re-signed third-party artifacts were removed from version control:
- `Moq.AutoMock.signed.dll`
- `Moq.AutoMock.il`
- `Moq.AutoMock.res`
- `Moq.AutoMock.Resources.Strings.resources`

These files should not be committed again.
