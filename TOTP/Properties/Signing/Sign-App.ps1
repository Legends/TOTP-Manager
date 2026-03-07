param(
    [Parameter(Mandatory = $true)]
    [string]$ExePath,

    [string]$PfxPath = "",
    [string]$PfxPassword = ""
)

$ErrorActionPreference = "Stop"

function Write-Info {
    param([string]$Message)
    Write-Host "[sign] $Message"
}

function Resolve-SignTool {
    $tool = Get-ChildItem -Path "${env:ProgramFiles(x86)}\Windows Kits\10\bin" -Recurse -Include signtool.exe -ErrorAction SilentlyContinue |
        Where-Object { $_.FullName -like "*x64*" } |
        Sort-Object LastWriteTime -Descending |
        Select-Object -First 1

    if (-not $tool) {
        throw "signtool.exe not found."
    }

    return $tool.FullName
}

function Resolve-CertificateInput {
    param(
        [string]$InputPfxPath,
        [string]$InputPassword
    )

    $cleanupPath = $null
    $resolvedPfxPath = $InputPfxPath
    $resolvedPassword = $InputPassword

    if ([string]::IsNullOrWhiteSpace($resolvedPassword)) {
        $resolvedPassword = $env:SIGNING_CERT_PASSWORD
    }

    if ([string]::IsNullOrWhiteSpace($resolvedPfxPath)) {
        $resolvedPfxPath = $env:SIGNING_CERT_PATH
    }

    if ([string]::IsNullOrWhiteSpace($resolvedPfxPath) -and -not [string]::IsNullOrWhiteSpace($env:SIGNING_CERT_BASE64)) {
        $cleanupPath = Join-Path $env:TEMP ("totp-signing-" + [guid]::NewGuid().ToString("N") + ".pfx")
        [IO.File]::WriteAllBytes($cleanupPath, [Convert]::FromBase64String($env:SIGNING_CERT_BASE64))
        $resolvedPfxPath = $cleanupPath
        Write-Info "Loaded signing certificate from SIGNING_CERT_BASE64 into temp file."
    }

    if ([string]::IsNullOrWhiteSpace($resolvedPfxPath)) {
        throw "No signing certificate provided. Use -PfxPath, SIGNING_CERT_PATH, or SIGNING_CERT_BASE64."
    }

    if (-not (Test-Path $resolvedPfxPath)) {
        throw "Signing certificate path does not exist: $resolvedPfxPath"
    }

    if ([string]::IsNullOrWhiteSpace($resolvedPassword)) {
        throw "Signing certificate password missing. Provide -PfxPassword or SIGNING_CERT_PASSWORD."
    }

    return @{
        Path = $resolvedPfxPath
        Password = $resolvedPassword
        CleanupPath = $cleanupPath
    }
}

if (-not (Test-Path $ExePath)) {
    throw "Executable not found: $ExePath"
}

$signtool = Resolve-SignTool
Write-Info "Using signtool: $signtool"

$cert = Resolve-CertificateInput -InputPfxPath $PfxPath -InputPassword $PfxPassword
try {
    Write-Info "Signing executable: $ExePath"
    $output = & "$signtool" sign `
        /f "$($cert.Path)" `
        /p "$($cert.Password)" `
        /tr "http://timestamp.digicert.com" `
        /td sha256 `
        /fd sha256 `
        "$ExePath" 2>&1

    if ($LASTEXITCODE -ne 0) {
        throw "signtool failed with exit code $LASTEXITCODE. Output: $output"
    }

    $verifyOutput = & "$signtool" verify /v /pa "$ExePath" 2>&1
    if ($LASTEXITCODE -ne 0) {
        throw "Signature verification failed. Output: $verifyOutput"
    }

    Write-Info "Signature verified."
}
finally {
    if ($cert.CleanupPath -and (Test-Path $cert.CleanupPath)) {
        Remove-Item -Force $cert.CleanupPath
    }
}
