param(
    [Parameter(Mandatory = $false)]
    [string]$ProjectPath = "D:\Repos\TOTP-Manager\updatertest\LocalUpdateStub\LocalUpdateStub.csproj",

    [Parameter(Mandatory = $false)]
    [string]$PublishFolder = "D:\Repos\TOTP-Manager\updatertest\LocalUpdateStub\publish",

    [Parameter(Mandatory = $false)]
    [string]$FeedFolder = "D:\Repos\TOTP-Manager\updatertest\iis-feed-debug",

    [Parameter(Mandatory = $false)]
    [string]$BaseDownloadUrl = "http://127.0.0.1:5005/",

    [Parameter(Mandatory = $false)]
    [string]$PrivateKeyPath = "D:\Repos\TOTP-Manager\updatertest\netsparkle keys\NetSparkle_Ed25519.priv",

    [Parameter(Mandatory = $false)]
    [string]$PublicKeyPath = "D:\Repos\TOTP-Manager\updatertest\netsparkle keys\NetSparkle_Ed25519.pub",

    [Parameter(Mandatory = $false)]
    [string]$FileVersion = "1.0.1",

    [Parameter(Mandatory = $false)]
    [string]$DisplayVersion = "1.0.1-local-stub"
)

$ErrorActionPreference = "Stop"

$scriptRoot = Split-Path -Parent $MyInvocation.MyCommand.Path
$publishFeedScript = Join-Path $scriptRoot "Publish-LocalUpdateFeed.ps1"

if (-not (Test-Path $ProjectPath)) {
    throw "Stub project not found: $ProjectPath"
}

Write-Host "[update] Publishing local update stub"
dotnet publish $ProjectPath -c Release -o $PublishFolder

& $publishFeedScript `
    -ReleaseFolder $PublishFolder `
    -IisFeedFolder $FeedFolder `
    -BaseDownloadUrl $BaseDownloadUrl `
    -PrivateKeyPath $PrivateKeyPath `
    -PublicKeyPath $PublicKeyPath `
    -MainExecutableName "LocalUpdateStub.exe" `
    -FileVersion $FileVersion `
    -DisplayVersion $DisplayVersion

Write-Host "[update] Local stub feed prepared at: $FeedFolder"
