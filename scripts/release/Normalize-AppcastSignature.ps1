param(
    [Parameter(Mandatory = $true)]
    [string]$SignaturePath
)

$ErrorActionPreference = "Stop"

if (-not (Test-Path $SignaturePath)) {
    throw "Signature file not found: $SignaturePath"
}

$rawValue = (Get-Content -Path $SignaturePath -Raw).Trim()
if ([string]::IsNullOrWhiteSpace($rawValue)) {
    throw "Signature file is empty: $SignaturePath"
}

$decimalBytePattern = '^(?:\d{1,3}\s+)*\d{1,3}$'
$normalizedValue = $rawValue

if ($rawValue -match $decimalBytePattern) {
    $bytes = $rawValue -split '\s+' | ForEach-Object { [byte]$_ }
    $normalizedValue = [System.Text.Encoding]::ASCII.GetString($bytes).Trim()
    Write-Host "[update] Converted decimal byte signature to base64 text."
}

$base64Pattern = '^[A-Za-z0-9+/=]+$'
if ($normalizedValue -notmatch $base64Pattern) {
    throw "Signature file does not contain a valid base64 signature: $SignaturePath"
}

try {
    [void][Convert]::FromBase64String($normalizedValue)
}
catch {
    throw "Signature file is not valid base64: $SignaturePath"
}

Set-Content -Path $SignaturePath -Value $normalizedValue -NoNewline -Encoding utf8
Write-Host "[update] Normalized appcast signature file: $SignaturePath"
