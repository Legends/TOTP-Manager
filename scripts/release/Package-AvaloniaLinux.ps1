param(
    [Parameter(Mandatory = $true)]
    [string]$PublishDirectory,

    [Parameter(Mandatory = $true)]
    [string]$OutputDirectory,

    [Parameter(Mandatory = $true)]
    [string]$ReleaseVersion,

    [switch]$FrameworkDependent
)

$ErrorActionPreference = "Stop"
if (-not $IsLinux) { throw "The Linux packages must be assembled on Linux." }
if ($ReleaseVersion -notmatch '^(?<base>\d+\.\d+\.\d+)(?:-rc(?<rc>\d+))?$') {
    throw "ReleaseVersion must match <major>.<minor>.<patch>[-rc<nr>]."
}
$baseVersion = $Matches.base
$releaseCandidateNumber = $Matches.rc
$releaseChannel = if ($releaseCandidateNumber) { "rc" } else { "stable" }

$resolvedPublish = (Resolve-Path -LiteralPath $PublishDirectory).Path
$resolvedOutput = [IO.Path]::GetFullPath($OutputDirectory)
if (Test-Path -LiteralPath $resolvedOutput) {
    if (@(Get-ChildItem -LiteralPath $resolvedOutput -Force).Count -ne 0) {
        throw "OutputDirectory must be absent or empty."
    }
}
else {
    New-Item -ItemType Directory -Path $resolvedOutput | Out-Null
}

$portableRoot = Join-Path $resolvedOutput "TOTP-Manager-linux-x64-$ReleaseVersion"
New-Item -ItemType Directory -Path $portableRoot | Out-Null
Copy-Item -Path (Join-Path $resolvedPublish "*") -Destination $portableRoot -Recurse
& (Join-Path $PSScriptRoot "Set-PackageUpdatePolicy.ps1") `
    -PackageDirectory $portableRoot `
    -DistributionMode direct `
    -Channel $releaseChannel
$portableExecutable = Join-Path $portableRoot "TOTP.UI.Avalonia.Desktop"
& chmod "+x" $portableExecutable
if ($LASTEXITCODE -ne 0) { throw "Could not mark the Linux host executable." }

$tarPath = Join-Path $resolvedOutput "TOTP-Manager-linux-x64-$ReleaseVersion.tar.gz"
& tar -C $portableRoot -czf $tarPath .
if ($LASTEXITCODE -ne 0) { throw "Could not create the portable tarball." }

$debRoot = Join-Path $resolvedOutput "deb-root"
$debControl = Join-Path $debRoot "DEBIAN"
$debApp = Join-Path $debRoot "opt/totp-manager"
$debBin = Join-Path $debRoot "usr/bin"
$debDesktop = Join-Path $debRoot "usr/share/applications"
New-Item -ItemType Directory -Path $debControl, $debApp, $debBin, $debDesktop -Force | Out-Null
Copy-Item -Path (Join-Path $resolvedPublish "*") -Destination $debApp -Recurse
& (Join-Path $PSScriptRoot "Set-PackageUpdatePolicy.ps1") `
    -PackageDirectory $debApp `
    -DistributionMode package-manager `
    -Channel $releaseChannel
& chmod "+x" (Join-Path $debApp "TOTP.UI.Avalonia.Desktop")
if ($LASTEXITCODE -ne 0) { throw "Could not mark the packaged Linux host executable." }

$debianVersion = if ($releaseCandidateNumber) {
    "$baseVersion~rc$releaseCandidateNumber"
}
else {
    $baseVersion
}
$dependencies = @("libsecret-tools", "libgl1", "libglib2.0-0")
if ($FrameworkDependent) { $dependencies = @("dotnet-runtime-9.0") + $dependencies }
$control = @"
Package: totp-manager
Version: $debianVersion
Section: utils
Priority: optional
Architecture: amd64
Maintainer: TOTP Manager maintainers
Depends: $($dependencies -join ', ')
Description: Local-first desktop TOTP authenticator
 A local-first Avalonia desktop authenticator with encrypted local storage.
"@
[IO.File]::WriteAllText((Join-Path $debControl "control"), $control, [Text.UTF8Encoding]::new($false))

$launcher = @'
#!/bin/sh
exec /opt/totp-manager/TOTP.UI.Avalonia.Desktop "$@"
'@
$launcherPath = Join-Path $debBin "totp-manager"
[IO.File]::WriteAllText($launcherPath, $launcher, [Text.UTF8Encoding]::new($false))
& chmod "+x" $launcherPath
if ($LASTEXITCODE -ne 0) { throw "Could not mark the Linux launcher executable." }

$desktopEntry = @"
[Desktop Entry]
Type=Application
Name=TOTP Manager
Comment=Local-first desktop TOTP authenticator
Exec=totp-manager
Terminal=false
Categories=Utility;Security;
StartupWMClass=TOTP Manager
"@
[IO.File]::WriteAllText(
    (Join-Path $debDesktop "io.github.legends.totpmanager.desktop"),
    $desktopEntry,
    [Text.UTF8Encoding]::new($false))

$debPath = Join-Path $resolvedOutput "totp-manager_${debianVersion}_amd64.deb"
& dpkg-deb --root-owner-group --build $debRoot $debPath
if ($LASTEXITCODE -ne 0) { throw "Could not create the DEB package." }
& dpkg-deb --info $debPath
if ($LASTEXITCODE -ne 0) { throw "The generated DEB package is invalid." }

Write-Output $tarPath
Write-Output $debPath
