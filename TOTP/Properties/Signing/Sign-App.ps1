# Sign-App.ps1
# signs totp.ui.wpf.exe during Release mode!

$ErrorActionPreference = 'Stop'

# Logging setup
$scriptDir = Split-Path -Parent -Path $MyInvocation.MyCommand.Path
$projectRoot = Resolve-Path "$scriptDir\..\.."
$logPath = "$projectRoot\signing-log.txt"

function Log {
    param([string]$msg)
    $timestamp = Get-Date -Format "yyyy-MM-dd HH:mm:ss"
    Add-Content -Path $logPath -Value "$timestamp - $msg"
    Write-Host $msg
}

Log "############################ SIGNING SCRIPT START ############################"

# Check CI context
$isCI = $env:GITHUB_ACTIONS -eq 'true'

# Locate signtool.exe
$signtoolPath = Get-ChildItem -Path "${env:ProgramFiles(x86)}\Windows Kits\10\bin" -Recurse `
    -Include signtool.exe -ErrorAction SilentlyContinue |
Where-Object { $_.FullName -like '*x64*' } |
Sort-Object LastWriteTime -Descending |
Select-Object -First 1

Log $signtoolPath

if (-not $signtoolPath) {
    Log "❌ Could not find signtool.exe"
    exit 101
}
Log "✅ Using SignTool path: $($signtoolPath.FullName)"

# Locate EXE
$exe = Get-ChildItem -Path "$projectRoot\bin\Release" -Recurse -File |
Where-Object { $_.Name -eq "TOTP.UI.WPF.exe" } |
Select-Object -First 1

Log $projectroot 
Log $exe

if (-not $exe) {
    Log "❌ Could not find the executable to sign."
    exit 102
}
Log "✅ Found executable: $($exe.FullName)"

# Check if already signed
try {
    $verifyOutput = & "$($signtoolPath.FullName)" verify /pa "$($exe.FullName)" 2>&1
    $verifyExitCode = $LASTEXITCODE
    Log "🔍 Signature check output:"
    # Log $verifyOutput

    if ($verifyExitCode -eq 0 -and $verifyOutput -match "Successfully verified") {
        Log "🔏 EXE is already signed and verified. Skipping signing step."
        exit 0
    }
    else {
        Log "ℹ️ EXE is not signed yet. Proceeding with signing."
    }
}
catch {
    $cleanMessage = $_.ToString() -replace "Error", "Info"
    Log "ℹ️ EXE is unsigned (expected): $cleanMessage"
}

# Locate PFX
$pfxPath = Join-Path -Path $projectRoot -ChildPath "Properties\Signing\totp-signing-cert.pfx"
if (-not (Test-Path $pfxPath)) {
    Log "❌ Signing certificate (.pfx) not found at $pfxPath"
    exit 103
}
Log "✅ Found PFX certificate"

# Load password
$pfxPassword = $null
if ($isCI) {
    $pfxPassword = $env:SIGNING_CERT_PASSWORD
    if (-not $pfxPassword) {
        Log "❌ SIGNING_CERT_PASSWORD not set in GitHub Actions"
        exit 104
    }
    Log "✅ Loaded password from CI environment"
}
else {
    Log "🔍 Attempting to load password from user-secrets..."
    $secrets = dotnet user-secrets list --project "$projectRoot" 2>&1
    foreach ($line in $secrets) {
        if ($line -match '^pfxPassword\s*=\s*(.+)$') {
            $pfxPassword = $matches[1].Trim()
            break
        }
    }

    if ([string]::IsNullOrWhiteSpace($pfxPassword)) {
        Log "❌ No pfxPassword found in user-secrets"
        exit 104
    }
    Log "✅ Loaded password from user-secrets"
}

# Sign the EXE
Log "🔐 Signing the executable..."
$signOutput = & "$($signtoolPath.FullName)" sign `
    /f "$pfxPath" `
    /p "$pfxPassword" `
    /tr "http://timestamp.digicert.com" `
    /td sha256 `
    /fd sha256 `
    "$($exe.FullName)"

Log "🔧 SignTool output:"
Log $signOutput

if ($LASTEXITCODE -ne 0) {
    Log "❌ SignTool failed with exit code $LASTEXITCODE"
    exit 105
}

# Verify signature (non-CI only)
if (-not $isCI) {
    Log "🔍 Verifying signature post-signing..."
    try {
        $verifyResult = & "$($signtoolPath.FullName)" verify /v /pa "$($exe.FullName)" 2>&1
        Log $verifyResult

        if ($verifyResult -match "Successfully verified") {
            Log "✅ Signature verified successfully."
        }
        elseif ($verifyResult -match "certificate.*not trusted by the trust provider") {
            Log "⚠️ Signature verified, but certificate is not trusted locally."
        }
        else {
            Log "❌ Signature verification failed unexpectedly."
            exit 106
        }
    }
    catch {
        Log "⚠️ Verification threw an error: $_"
    }
}
else {
    Log "⚠️ Skipping verification in CI context"
}

Log "🎉 Successfully signed the executable."
Log "############################ SIGNING SCRIPT END ############################"
