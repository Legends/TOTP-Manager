param(
    [Parameter(Mandatory = $false)]
    [string]$ProjectPath = "D:\Repos\TOTP-Manager\TOTP\TOTP.UI.WPF.csproj",

    [Parameter(Mandatory = $false)]
    [string]$AppcastUrl = "https://github.com/Legends/TOTP-Manager/releases/latest/download/appcast.xml",

    [Parameter(Mandatory = $false)]
    [string]$PublicKey = "C76rlrTBbpiwB2MKOevcc4zcRyzK2AMkA0DODBrnd5I=",

    [Parameter(Mandatory = $false)]
    [bool]$Enabled = $true,

    [Parameter(Mandatory = $false)]
    [bool]$CheckOnStartup = $true,

    [Parameter(Mandatory = $false)]
    [bool]$ForceStartupCheck = $false,

    [Parameter(Mandatory = $false)]
    [bool]$InteractiveDebugCheckOnStartup = $false,

    [Parameter(Mandatory = $false)]
    [int]$CheckIntervalMinutes = 1440
)

$ErrorActionPreference = "Stop"

if (-not (Test-Path $ProjectPath)) {
    throw "Project file not found: $ProjectPath"
}

Write-Host "[update] Configuring GitHub-hosted auto-update settings"
Write-Host "[update] Project: $ProjectPath"
Write-Host "[update] Appcast URL: $AppcastUrl"

$settings = [ordered]@{
    "AutoUpdate:Enabled" = $Enabled.ToString().ToLowerInvariant()
    "AutoUpdate:AppcastUrl" = $AppcastUrl
    "AutoUpdate:PublicKey" = $PublicKey
    "AutoUpdate:CheckOnStartup" = $CheckOnStartup.ToString().ToLowerInvariant()
    "AutoUpdate:ForceStartupCheck" = $ForceStartupCheck.ToString().ToLowerInvariant()
    "AutoUpdate:InteractiveDebugCheckOnStartup" = $InteractiveDebugCheckOnStartup.ToString().ToLowerInvariant()
    "AutoUpdate:CheckIntervalMinutes" = $CheckIntervalMinutes.ToString()
}

foreach ($setting in $settings.GetEnumerator()) {
    Write-Host "[update] user-secrets set $($setting.Key)=$($setting.Value)"
    dotnet user-secrets set $setting.Key $setting.Value --project $ProjectPath | Out-Null
}

Write-Host "[update] GitHub-hosted auto-update settings applied."
Write-Host "[update] Run the app with: dotnet run --project `"$ProjectPath`" -- --debug"
