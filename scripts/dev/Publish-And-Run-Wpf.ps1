[CmdletBinding()]
param(
    [ValidateSet("Debug", "Release")]
    [string]$Configuration = "Release",

    [string]$Runtime = "win-x64",

    [string]$OutputPath,

    [switch]$SelfContained,

    [switch]$StopRunningInstance
)

$ErrorActionPreference = "Stop"

$scriptDirectory = Split-Path -Parent $MyInvocation.MyCommand.Path
$repositoryRoot = [System.IO.Path]::GetFullPath((Join-Path $scriptDirectory "..\.."))
$projectPath = Join-Path $repositoryRoot "TOTP\TOTP.UI.WPF.csproj"

if ([string]::IsNullOrWhiteSpace($OutputPath)) {
    $OutputPath = Join-Path $repositoryRoot "artifacts\dev\wpf"
}
elseif (-not [System.IO.Path]::IsPathRooted($OutputPath)) {
    $OutputPath = Join-Path $repositoryRoot $OutputPath
}

$OutputPath = [System.IO.Path]::GetFullPath($OutputPath)
$executablePath = Join-Path $OutputPath "TOTP.UI.WPF.exe"

if (-not (Test-Path -LiteralPath $projectPath -PathType Leaf)) {
    throw "WPF project not found: $projectPath"
}

if ($StopRunningInstance) {
    Get-Process -Name "TOTP.UI.WPF" -ErrorAction SilentlyContinue |
        ForEach-Object {
            Write-Host "Stopping running TOTP Manager process $($_.Id)..."
            Stop-Process -Id $_.Id -ErrorAction Stop
            $_.WaitForExit(5000)
        }
}

$selfContainedValue = if ($SelfContained) { "true" } else { "false" }

Write-Host "Publishing TOTP Manager ($Configuration, $Runtime)..."
& dotnet publish $projectPath `
    --configuration $Configuration `
    --runtime $Runtime `
    --self-contained $selfContainedValue `
    --nologo `
    --verbosity minimal `
    -p:PublishSingleFile=false `
    -p:PublishTrimmed=false `
    --output $OutputPath

if ($LASTEXITCODE -ne 0) {
    throw "dotnet publish failed with exit code $LASTEXITCODE."
}

if (-not (Test-Path -LiteralPath $executablePath -PathType Leaf)) {
    throw "Published executable not found: $executablePath"
}

Write-Host "Starting $executablePath"
Start-Process -FilePath $executablePath -WorkingDirectory $OutputPath
