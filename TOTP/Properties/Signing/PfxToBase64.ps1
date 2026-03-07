param(
    [Parameter(Mandatory = $true)]
    [string]$PfxPath
)

$ErrorActionPreference = "Stop"
if (-not (Test-Path $PfxPath)) {
    throw "PFX file not found: $PfxPath"
}

[Convert]::ToBase64String([IO.File]::ReadAllBytes($PfxPath))
