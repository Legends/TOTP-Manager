param(
    [Parameter(Mandatory = $true)]
    [string]$PublishDirectory,

    [Parameter(Mandatory = $true)]
    [string]$OutputDirectory,

    [Parameter(Mandatory = $true)]
    [string]$ReleaseVersion,

    [string]$SigningIdentity,
    [string]$NotaryKeychainProfile
)

$ErrorActionPreference = "Stop"
if (-not $IsMacOS) { throw "The macOS package must be assembled on macOS." }
if ($ReleaseVersion -notmatch '^(?<base>\d+\.\d+\.\d+)(?:-rc(?<rc>\d+))?$') {
    throw "ReleaseVersion must match <major>.<minor>.<patch>[-rc<nr>]."
}
$baseVersion = $Matches.base
$releaseCandidateNumber = $Matches.rc

$resolvedPublish = (Resolve-Path -LiteralPath $PublishDirectory).Path
$resolvedOutput = [IO.Path]::GetFullPath($OutputDirectory)
if (Test-Path -LiteralPath $resolvedOutput) {
    if (@(Get-ChildItem -LiteralPath $resolvedOutput -Force).Count -ne 0) {
        throw "OutputDirectory must be absent or empty."
    }
}
else {
    New-Item -ItemType Directory -Path $resolvedOutput | Out-Null
}

$bundleVersion = if ($releaseCandidateNumber) {
    "$baseVersion.$releaseCandidateNumber"
}
else {
    "$baseVersion.0"
}
$appRoot = Join-Path $resolvedOutput "TOTP Manager.app"
$contents = Join-Path $appRoot "Contents"
$macOS = Join-Path $contents "MacOS"
$resources = Join-Path $contents "Resources"
New-Item -ItemType Directory -Path $macOS -Force | Out-Null
New-Item -ItemType Directory -Path $resources -Force | Out-Null
Copy-Item -Path (Join-Path $resolvedPublish "*") -Destination $macOS -Recurse

$plist = @"
<?xml version="1.0" encoding="UTF-8"?>
<!DOCTYPE plist PUBLIC "-//Apple//DTD PLIST 1.0//EN" "https://www.apple.com/DTDs/PropertyList-1.0.dtd">
<plist version="1.0">
<dict>
  <key>CFBundleDevelopmentRegion</key><string>en</string>
  <key>CFBundleDisplayName</key><string>TOTP Manager</string>
  <key>CFBundleExecutable</key><string>TOTP.UI.Avalonia.Desktop</string>
  <key>CFBundleIdentifier</key><string>io.github.legends.totpmanager</string>
  <key>CFBundleInfoDictionaryVersion</key><string>6.0</string>
  <key>CFBundleName</key><string>TOTP Manager</string>
  <key>CFBundlePackageType</key><string>APPL</string>
  <key>CFBundleShortVersionString</key><string>$baseVersion</string>
  <key>CFBundleVersion</key><string>$bundleVersion</string>
  <key>LSMinimumSystemVersion</key><string>14.0</string>
  <key>LSMultipleInstancesProhibited</key><true/>
  <key>NSCameraUsageDescription</key><string>Scan a TOTP setup QR code after you explicitly start the camera scanner.</string>
  <key>NSHighResolutionCapable</key><true/>
</dict>
</plist>
"@
$plistPath = Join-Path $contents "Info.plist"
[IO.File]::WriteAllText($plistPath, $plist, [Text.UTF8Encoding]::new($false))

$entitlements = @"
<?xml version="1.0" encoding="UTF-8"?>
<!DOCTYPE plist PUBLIC "-//Apple//DTD PLIST 1.0//EN" "https://www.apple.com/DTDs/PropertyList-1.0.dtd">
<plist version="1.0">
<dict>
  <key>com.apple.security.cs.allow-jit</key><true/>
  <key>com.apple.security.cs.allow-unsigned-executable-memory</key><true/>
  <key>com.apple.security.cs.allow-dyld-environment-variables</key><true/>
  <key>com.apple.security.cs.disable-library-validation</key><true/>
  <key>com.apple.security.device.camera</key><true/>
</dict>
</plist>
"@
$entitlementsPath = Join-Path $resolvedOutput "TOTP.Manager.entitlements"
[IO.File]::WriteAllText($entitlementsPath, $entitlements, [Text.UTF8Encoding]::new($false))

$mainExecutable = Join-Path $macOS "TOTP.UI.Avalonia.Desktop"
& chmod "+x" $mainExecutable
if ($LASTEXITCODE -ne 0) { throw "Could not mark the macOS host executable." }
& plutil -lint $plistPath
if ($LASTEXITCODE -ne 0) { throw "The generated Info.plist is invalid." }

if (-not [string]::IsNullOrWhiteSpace($SigningIdentity)) {
    $machOFiles = Get-ChildItem -LiteralPath $macOS -File -Recurse |
        Where-Object { $_.FullName -ne $mainExecutable -and (& file --brief $_.FullName) -like 'Mach-O*' } |
        Sort-Object { $_.FullName.Split([IO.Path]::DirectorySeparatorChar).Count } -Descending
    foreach ($file in $machOFiles) {
        & codesign --force --options runtime --timestamp --sign $SigningIdentity $file.FullName
        if ($LASTEXITCODE -ne 0) { throw "Could not sign a nested Mach-O file." }
    }

    & codesign --force --options runtime --timestamp --entitlements $entitlementsPath --sign $SigningIdentity $appRoot
    if ($LASTEXITCODE -ne 0) { throw "Could not sign the application bundle." }
    & codesign --verify --deep --strict --verbose=2 $appRoot
    if ($LASTEXITCODE -ne 0) { throw "Application signature verification failed." }
}
elseif (-not [string]::IsNullOrWhiteSpace($NotaryKeychainProfile)) {
    throw "Notarization requires SigningIdentity."
}

$dmgPath = Join-Path $resolvedOutput "TOTP-Manager-macos-arm64-$ReleaseVersion.dmg"
& hdiutil create -volname "TOTP Manager" -srcfolder $appRoot -ov -format UDZO $dmgPath
if ($LASTEXITCODE -ne 0) { throw "Could not create the DMG." }

if (-not [string]::IsNullOrWhiteSpace($SigningIdentity)) {
    & codesign --force --timestamp --sign $SigningIdentity $dmgPath
    if ($LASTEXITCODE -ne 0) { throw "Could not sign the DMG." }
}

if (-not [string]::IsNullOrWhiteSpace($NotaryKeychainProfile)) {
    & xcrun notarytool submit $dmgPath --keychain-profile $NotaryKeychainProfile --wait
    if ($LASTEXITCODE -ne 0) { throw "Apple notarization failed." }
    & xcrun stapler staple $dmgPath
    if ($LASTEXITCODE -ne 0) { throw "Could not staple the notarization ticket." }
    & xcrun stapler validate $dmgPath
    if ($LASTEXITCODE -ne 0) { throw "The stapled notarization ticket is invalid." }
    & spctl --assess --type open --context context:primary-signature --verbose=2 $dmgPath
    if ($LASTEXITCODE -ne 0) { throw "Gatekeeper rejected the notarized DMG." }
}

Write-Output $dmgPath
