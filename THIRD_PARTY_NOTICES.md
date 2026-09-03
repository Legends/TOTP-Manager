# Third-party notices

OTP Harbor uses third-party packages under licenses compatible with this repository's MIT distribution. Exact versions are declared in the project files and resolved by `dotnet restore`; dependency and vulnerability review runs in CI.

## Runtime dependencies

- Avalonia and Avalonia Themes Fluent — MIT
- FluentResults — MIT
- Microsoft.Extensions libraries — MIT
- Microsoft.Win32.SystemEvents — MIT
- NSec.Cryptography — MIT
- Otp.NET — MIT
- QRCoder — MIT
- OpenCvSharp and selected native runtimes — Apache-2.0
- Serilog and its configured enrichers/sinks — Apache-2.0
- ZXing.Net — Apache-2.0

## Test dependencies

- xUnit and runner packages — Apache-2.0
- Microsoft.NET.Test.Sdk — MIT
- Moq — BSD-3-Clause
- Avalonia.Headless.XUnit — MIT

## Assets

The application icon and locale flags are project-owned or locally rendered from public-domain flag geometry. Provenance and reviewed hashes are recorded in [docs/assets/ASSET_PROVENANCE.md](docs/assets/ASSET_PROVENANCE.md).

This file is an inventory aid, not legal advice. Review upstream license texts when adding or upgrading a dependency.
