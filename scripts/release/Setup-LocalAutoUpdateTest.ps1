param(
    [Parameter(Mandatory = $false)]
    [string]$ProjectPath = "D:\Repos\TOTP-Manager\TOTP\TOTP.UI.WPF.csproj",

    [Parameter(Mandatory = $false)]
    [string]$AppcastUrl = "http://127.0.0.1:5005/appcast.xml",

    [Parameter(Mandatory = $false)]
    [string]$PublicKeyPath = "D:\Repos\TOTP-Manager\updatertest\netsparkle keys\NetSparkle_Ed25519.pub",

    [Parameter(Mandatory = $false)]
    [bool]$Enabled = $true,

    [Parameter(Mandatory = $false)]
    [bool]$CheckOnStartup = $true,

    [Parameter(Mandatory = $false)]
    [bool]$ForceStartupCheck = $true,

    [Parameter(Mandatory = $false)]
    [bool]$InteractiveDebugCheckOnStartup = $true,

    [Parameter(Mandatory = $false)]
    [int]$CheckIntervalMinutes = 5
)

$ErrorActionPreference = "Stop"

if (-not (Test-Path $ProjectPath)) {
    throw "Project file not found: $ProjectPath"
}

if (-not (Test-Path $PublicKeyPath)) {
    throw "Public key file not found: $PublicKeyPath"
}

$publicKey = (Get-Content $PublicKeyPath -Raw).Trim()
if ([string]::IsNullOrWhiteSpace($publicKey)) {
    throw "Public key file is empty: $PublicKeyPath"
}

Write-Host "[update] Configuring local auto-update test settings"
Write-Host "[update] Project: $ProjectPath"
Write-Host "[update] Appcast URL: $AppcastUrl"
Write-Host "[update] Public key path: $PublicKeyPath"

$settings = [ordered]@{
    "AutoUpdate:Enabled" = $Enabled.ToString().ToLowerInvariant()
    "AutoUpdate:AppcastUrl" = $AppcastUrl
    "AutoUpdate:PublicKey" = $publicKey
    "AutoUpdate:CheckOnStartup" = $CheckOnStartup.ToString().ToLowerInvariant()
    "AutoUpdate:ForceStartupCheck" = $ForceStartupCheck.ToString().ToLowerInvariant()
    "AutoUpdate:InteractiveDebugCheckOnStartup" = $InteractiveDebugCheckOnStartup.ToString().ToLowerInvariant()
    "AutoUpdate:CheckIntervalMinutes" = $CheckIntervalMinutes.ToString()
}

foreach ($setting in $settings.GetEnumerator()) {
    Write-Host "[update] user-secrets set $($setting.Key)=$($setting.Value)"
    dotnet user-secrets set $setting.Key $setting.Value --project $ProjectPath | Out-Null
}

Write-Host "[update] Local auto-update test settings applied."
Write-Host "[update] Start the app with: dotnet run --project `"$ProjectPath`" -- --debug"
