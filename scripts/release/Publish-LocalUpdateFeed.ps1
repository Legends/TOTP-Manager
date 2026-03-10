param(
    [Parameter(Mandatory = $true)]
    [string]$ReleaseFolder,

    [Parameter(Mandatory = $false)]
    [string]$IisFeedFolder = "D:\Repos\TOTP-Manager\updatertest\iis-feed",

    [Parameter(Mandatory = $false)]
    [string]$BaseDownloadUrl = "http://localhost/",

    [Parameter(Mandatory = $false)]
    [string]$PrivateKeyPath = "D:\Repos\TOTP-Manager\updatertest\netsparkle keys\NetSparkle_Ed25519.priv",

    [Parameter(Mandatory = $false)]
    [string]$PublicKeyPath = "D:\Repos\TOTP-Manager\updatertest\netsparkle keys\NetSparkle_Ed25519.pub",

    [Parameter(Mandatory = $false)]
    [string]$MainExecutableName = "TOTP.UI.WPF.exe",

    [Parameter(Mandatory = $false)]
    [string]$FileVersion,

    [Parameter(Mandatory = $false)]
    [string]$DisplayVersion
)

$ErrorActionPreference = "Stop"

$scriptRoot = Split-Path -Parent $MyInvocation.MyCommand.Path
$appcastScript = Join-Path $scriptRoot "Generate-Appcast.ps1"

if (-not (Test-Path $ReleaseFolder)) {
    throw "Release folder not found: $ReleaseFolder"
}

if (-not (Test-Path $appcastScript)) {
    throw "Appcast generator script not found: $appcastScript"
}

New-Item -ItemType Directory -Path $IisFeedFolder -Force | Out-Null

Write-Host "[update] Preparing local IIS feed"
Write-Host "[update] Release folder: $ReleaseFolder"
Write-Host "[update] IIS feed folder: $IisFeedFolder"
Write-Host "[update] Base download URL: $BaseDownloadUrl"

& $appcastScript `
    -ReleaseFolder $ReleaseFolder `
    -BaseDownloadUrl $BaseDownloadUrl `
    -PrivateKeyPath $PrivateKeyPath `
    -PublicKeyPath $PublicKeyPath `
    -MainArtifactName $MainExecutableName `
    -FileVersion $FileVersion `
    -DisplayVersion $DisplayVersion

Copy-Item -Path (Join-Path $ReleaseFolder "*") -Destination $IisFeedFolder -Recurse -Force

Write-Host "[update] Local IIS feed prepared."
Write-Host "[update] Files available in: $IisFeedFolder"
Write-Host "[update] Host that folder in IIS and verify that $($BaseDownloadUrl.TrimEnd('/'))/appcast.xml is reachable."
