param (
    [string]$CertPath,
    [System.Security.SecureString]$CertPassword
)

$ErrorActionPreference = "Stop"

function Log {
    param([string]$message)
    Write-Host "[INFO] $message"
}

function LogError {
    param([string]$message)
    Write-Host "[ERROR] $message" -ForegroundColor Red
}

try {
    $SolutionDir = Split-Path -Parent $MyInvocation.MyCommand.Definition
    $ProjectPath = Join-Path $SolutionDir "TOTP\TOTP.Manager.csproj"
    $OutputDir = Join-Path $SolutionDir "publish"

    Log "Starting publish process..."

    if (Test-Path $OutputDir) {
        Log "Cleaning existing publish directory..."
        Remove-Item -Recurse -Force $OutputDir
    }

    Log "Running dotnet publish..."
    dotnet publish "$ProjectPath" `
        --configuration Release `
        --runtime win-x64 `
        --self-contained true `
        -p:PublishSingleFile=true `
        -p:IncludeAllContentForSelfExtract=true `
        -p:PublishTrimmed=false `
        --output "$OutputDir"

    Log "Publish completed."

    if ($CertPath -and $CertPassword) {
        Log "Signing enabled. Preparing..."
        $exePath = Get-ChildItem $OutputDir -Filter "*.exe" | Select-Object -First 1

        if ($exePath) {
            $tempPwd = [System.Runtime.InteropServices.Marshal]::PtrToStringAuto(
                [System.Runtime.InteropServices.Marshal]::SecureStringToBSTR($CertPassword)
            )

            $signtoolPath = "${env:ProgramFiles(x86)}\Windows Kits\10\bin\x64\signtool.exe"
            if (-not (Test-Path $signtoolPath)) {
                $signtoolPath = Get-Command signtool.exe -ErrorAction SilentlyContinue | Select-Object -First 1 | ForEach-Object { $_.Source }
            }

            if (-not $signtoolPath) {
                LogError "Could not find signtool.exe."
                exit 1
            }

            Log "Signing the executable..."
            & "$signtoolPath" sign `
                /f "$CertPath" `
                /p "$tempPwd" `
                /fd SHA256 `
                /tr http://timestamp.digicert.com `
                /td SHA256 `
                /v "$($exePath.FullName)"

            Log "Signing completed."
        }
        else {
            LogError "❌ No .exe file found in publish directory."
        }
    }

    Log "Opening publish folder..."
    Start-Process "explorer.exe" -ArgumentList "$OutputDir"
}
catch {
    LogError "❌ An error occurred: $_"
    exit 1
}
