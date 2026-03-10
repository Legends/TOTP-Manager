param(
    [Parameter(Mandatory = $true)]
    [string]$ReleaseFolder,

    [Parameter(Mandatory = $true)]
    [string]$BaseDownloadUrl,

    [Parameter(Mandatory = $true)]
    [string]$PrivateKeyPath,

    [Parameter(Mandatory = $true)]
    [string]$PublicKeyPath,

    [Parameter(Mandatory = $false)]
    [string]$FileVersion
)

$ErrorActionPreference = "Stop"

if (-not (Test-Path $ReleaseFolder)) {
    throw "Release folder not found: $ReleaseFolder"
}

if (-not (Test-Path $PrivateKeyPath)) {
    throw "Private key file not found: $PrivateKeyPath"
}

if (-not (Test-Path $PublicKeyPath)) {
    throw "Public key file not found: $PublicKeyPath"
}

Write-Host "[update] Generating appcast for $ReleaseFolder"
Write-Host "[update] Base download URL: $BaseDownloadUrl"
if (-not [string]::IsNullOrWhiteSpace($FileVersion)) {
    Write-Host "[update] Override appcast version: $FileVersion"
}

# Requires: dotnet tool install --global NetSparkleUpdater.Tools.AppCastGenerator
$arguments = @(
    '--binaries', "$ReleaseFolder",
    '--ext', 'exe',
    '--base-url', "$BaseDownloadUrl",
    '--appcast-output-directory', "$ReleaseFolder",
    '--output-file-name', 'appcast',
    '--key-path', ([IO.Path]::GetDirectoryName($PrivateKeyPath)),
    '--private-key-override', (Get-Content $PrivateKeyPath -Raw).Trim(),
    '--public-key-override', (Get-Content $PublicKeyPath -Raw).Trim(),
    '--human-readable'
)

if (-not [string]::IsNullOrWhiteSpace($FileVersion)) {
    $arguments += @('--file-version', $FileVersion)
}

netsparkle-generate-appcast @arguments

Write-Host "[update] Appcast generation complete."
