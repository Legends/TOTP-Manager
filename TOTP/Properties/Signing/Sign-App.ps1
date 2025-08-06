# Sign-App.ps1


function Debug-Output {
    param([string]$message)
    if ($Host.Name -like "*Visual Studio Code*") {
        Write-Output $message
    }
    else {
        Write-Host $message
    }
}

# Signs the main application executable using Authenticode and a PFX certificate

$ErrorActionPreference = 'Stop'

# Determine if we're running in GitHub Actions
$isCI = $env:GITHUB_ACTIONS -eq 'true'

# Get root path of the project
$scriptDir = Split-Path -Parent -Path $MyInvocation.MyCommand.Path
$projectRoot = Resolve-Path "$scriptDir\..\.."

# Resolve signtool.exe dynamically (latest x64 version)
$signtoolPath = Get-ChildItem -Path "${env:ProgramFiles(x86)}\Windows Kits\10\bin" -Recurse `
    -Include signtool.exe -ErrorAction SilentlyContinue |
Where-Object { $_.FullName -like '*x64*' } |
Sort-Object LastWriteTime -Descending |
Select-Object -First 1

if (-not $signtoolPath) {
    Write-Error "❌ Could not find signtool.exe"
    exit 1
}

Debug-Output "✅ Using SignTool path: $($signtoolPath.FullName)"

# Locate the EXE
$exe = Get-ChildItem -Path "$projectRoot\bin\Release" -Recurse -Filter "TOTP.Manager.exe" | Select-Object -First 1
if (-not $exe) {
    Write-Error "❌ Could not find the executable to sign."
    exit 1
}

Debug-Output "✅ Found executable: $($exe.FullName)"

# Check if already signed and valid
try {
    $verifyOutput = & "$($signtoolPath.FullName)" verify /pa "$($exe.FullName)" 2>&1
    if ($verifyOutput -match "Successfully verified") {
        Debug-Output "🔏 EXE is already signed and verified. Skipping signing step."
        exit 0
    }
    else {
        Debug-Output "ℹ️ EXE is not signed yet. Proceeding with signing."
    }
}
catch {
    Debug-Output "⚠️ Signature verification threw an error (expected if unsigned). Proceeding with signing."
}


# Resolve PFX file
$pfxPath = Join-Path -Path $projectRoot -ChildPath "Properties\Signing\totp-signing-cert.pfx"
if (-not (Test-Path $pfxPath)) {
    Write-Error "❌ Signing certificate (.pfx) not found at $pfxPath"
    exit 1
}

# Load password from GitHub Actions or user-secrets
if ($isCI) {
    $pfxPassword = $env:SIGNING_CERT_PASSWORD
    if (-not $pfxPassword) {
        Write-Error "❌ SIGNING_CERT_PASSWORD secret not set in GitHub Actions."
        exit 1
    }
}
else {
    Debug-Output "`$projectRoot = $projectRoot"
    $pfxPassword = dotnet user-secrets list --project "$projectRoot" |
    Where-Object { $_ -match '^pfxPassword\s*=\s*(.+)$' } |
    ForEach-Object { $matches[1].Trim() }

    if (-not $pfxPassword) {
        Write-Error "❌ No pfxPassword found in user secrets."
        exit 1
    }

    Debug-Output "`$pfxPassword = $pfxPassword"  # DEBUG: Remove in production
}

# Sign the EXE
Debug-Output "🔐 Signing the executable..."
& "$($signtoolPath.FullName)" sign `
    /f "$pfxPath" `
    /p "$pfxPassword" `
    /tr "http://timestamp.digicert.com" `
    /td sha256 `
    /fd sha256 `
    "$($exe.FullName)"

if ($LASTEXITCODE -ne 0) {
    Write-Error "❌ SignTool failed with exit code $LASTEXITCODE"
    exit $LASTEXITCODE
}

Debug-Output "🎉 Successfully signed the executable."

# Verify signature
Debug-Output "🔍 Verifying signature..."
try {
    $verifyResult = & "$($signtoolPath.FullName)" verify /v /pa "$($exe.FullName)" 2>&1
    Debug-Output $verifyResult

    if ($verifyResult -match "Successfully verified") {
        Debug-Output "✅ Signature verified successfully."
    }
    elseif ($verifyResult -match "certificate.*not trusted by the trust provider") {
        Debug-Output "⚠️ Signature verified, but the certificate is self-signed and not trusted on this machine."
        # Don't treat this as an error in CI context
    }
    else {
        Write-Error "❌ Signature verification failed unexpectedly."
        exit 1
    }
}
catch {
    Write-Warning "⚠️ Signature verification threw an error: $_. Exception will be ignored in CI."
}



