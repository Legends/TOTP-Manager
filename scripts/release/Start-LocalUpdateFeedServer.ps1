param(
    [Parameter(Mandatory = $false)]
    [string]$FeedFolder = "D:\Repos\TOTP-Manager\updatertest\iis-feed-debug",

    [Parameter(Mandatory = $false)]
    [int]$Port = 5005,

    [Parameter(Mandatory = $false)]
    [string]$BindAddress = "127.0.0.1"
)

$ErrorActionPreference = "Stop"

if (-not (Test-Path $FeedFolder)) {
    throw "Feed folder not found: $FeedFolder"
}

$python = Get-Command python -ErrorAction SilentlyContinue
if ($null -eq $python) {
    throw "python.exe was not found on PATH."
}

Write-Host "[update] Starting local update feed server"
Write-Host "[update] Feed folder: $FeedFolder"
Write-Host "[update] URL: http://$BindAddress`:$Port/appcast.xml"
Write-Host "[update] Press Ctrl+C to stop"

Push-Location $FeedFolder
try {
    & $python.Source -m http.server $Port --bind $BindAddress
}
finally {
    Pop-Location
}
