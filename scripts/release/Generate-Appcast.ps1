param(
    [Parameter(Mandatory = $true)]
    [string]$ReleaseFolder,

    [Parameter(Mandatory = $true)]
    [string]$BaseDownloadUrl,

    [Parameter(Mandatory = $true)]
    [string]$PrivateKeyPath,

    [Parameter(Mandatory = $true)]
    [string]$PublicKeyPath,

    [Parameter(Mandatory = $false)]
    [string]$MainArtifactName = "TOTP.UI.WPF.exe",

    [Parameter(Mandatory = $false)]
    [string]$FileVersion,

    [Parameter(Mandatory = $false)]
    [string]$DisplayVersion
)

$ErrorActionPreference = "Stop"

if (-not (Test-Path $ReleaseFolder)) {
    throw "Release folder not found: $ReleaseFolder"
}

if (-not (Test-Path $PrivateKeyPath)) {
    throw "Private key file not found: $PrivateKeyPath"
}

if (-not (Test-Path $PublicKeyPath)) {
    throw "Public key file not found: $PublicKeyPath"
}

if ([string]::IsNullOrWhiteSpace($MainArtifactName)) {
    throw "Main artifact name must not be empty."
}

$mainArtifactPath = Join-Path $ReleaseFolder $MainArtifactName
if (-not (Test-Path $mainArtifactPath)) {
    throw "Main artifact not found: $mainArtifactPath"
}

Write-Host "[update] Generating appcast for $ReleaseFolder"
Write-Host "[update] Base download URL: $BaseDownloadUrl"
Write-Host "[update] Main artifact: $MainArtifactName"
if (-not [string]::IsNullOrWhiteSpace($FileVersion)) {
    Write-Host "[update] Override appcast version: $FileVersion"
}
if (-not [string]::IsNullOrWhiteSpace($DisplayVersion)) {
    Write-Host "[update] Override appcast display version: $DisplayVersion"
}

# Requires: dotnet tool install --global NetSparkleUpdater.Tools.AppCastGenerator
$tempBinaryDirectory = Join-Path ([IO.Path]::GetTempPath()) ("netsparkle-appcast-" + [Guid]::NewGuid().ToString("N"))
New-Item -ItemType Directory -Path $tempBinaryDirectory -Force | Out-Null

$artifactExtension = [IO.Path]::GetExtension($MainArtifactName).TrimStart('.')
if ([string]::IsNullOrWhiteSpace($artifactExtension)) {
    throw "Main artifact must have a file extension: $MainArtifactName"
}

$arguments = @(
    '--binaries', "$tempBinaryDirectory",
    '--ext', $artifactExtension,
    '--base-url', "$BaseDownloadUrl",
    '--appcast-output-directory', "$ReleaseFolder",
    '--output-file-name', 'appcast',
    '--key-path', ([IO.Path]::GetDirectoryName($PrivateKeyPath)),
    '--private-key-override', (Get-Content $PrivateKeyPath -Raw).Trim(),
    '--public-key-override', (Get-Content $PublicKeyPath -Raw).Trim(),
    '--human-readable'
)

if (-not [string]::IsNullOrWhiteSpace($FileVersion)) {
    $arguments += @('--file-version', $FileVersion)
}

try {
    Copy-Item -Path $mainArtifactPath -Destination (Join-Path $tempBinaryDirectory $MainArtifactName) -Force
    netsparkle-generate-appcast @arguments
}
finally {
    if (Test-Path $tempBinaryDirectory) {
        Remove-Item -Path $tempBinaryDirectory -Recurse -Force
    }
}

$appcastPath = Join-Path $ReleaseFolder "appcast.xml"
if (-not (Test-Path $appcastPath)) {
    throw "Generated appcast not found: $appcastPath"
}

$xml = [xml](Get-Content -Path $appcastPath -Raw)
$sparkleNs = "http://www.andymatuschak.org/xml-namespaces/sparkle"
$nsManager = New-Object System.Xml.XmlNamespaceManager($xml.NameTable)
$nsManager.AddNamespace("sparkle", $sparkleNs)

$itemNode = $xml.SelectSingleNode("/rss/channel/item")
if ($null -eq $itemNode) {
    throw "Generated appcast does not contain /rss/channel/item."
}

$enclosureNode = $itemNode.SelectSingleNode("enclosure")
if ($null -eq $enclosureNode) {
    throw "Generated appcast does not contain an enclosure element."
}

if (-not [string]::IsNullOrWhiteSpace($FileVersion)) {
    $itemVersionNode = $itemNode.SelectSingleNode("sparkle:version", $nsManager)
    if ($null -eq $itemVersionNode) {
        $itemVersionNode = $xml.CreateElement("sparkle", "version", $sparkleNs)
        [void]$itemNode.AppendChild($itemVersionNode)
    }

    $itemVersionNode.InnerText = $FileVersion
    $enclosureNode.SetAttribute("version", $sparkleNs, $FileVersion)
}

if (-not [string]::IsNullOrWhiteSpace($DisplayVersion)) {
    $shortVersionNode = $itemNode.SelectSingleNode("sparkle:shortVersionString", $nsManager)
    if ($null -eq $shortVersionNode) {
        $shortVersionNode = $xml.CreateElement("sparkle", "shortVersionString", $sparkleNs)
        [void]$itemNode.AppendChild($shortVersionNode)
    }

    $shortVersionNode.InnerText = $DisplayVersion
    $enclosureNode.SetAttribute("shortVersionString", $sparkleNs, $DisplayVersion)

    $titleNode = $itemNode.SelectSingleNode("title")
    if ($null -ne $titleNode) {
        $titleNode.InnerText = "Application $DisplayVersion"
    }
}

$settings = New-Object System.Xml.XmlWriterSettings
$settings.Indent = $true
$settings.Encoding = [System.Text.UTF8Encoding]::new($false)
$writer = [System.Xml.XmlWriter]::Create($appcastPath, $settings)
try {
    $xml.Save($writer)
}
finally {
    $writer.Dispose()
}

$appcastSignature = $null
if (-not [string]::IsNullOrWhiteSpace($PrivateKeyPath) -and -not [string]::IsNullOrWhiteSpace($PublicKeyPath)) {
    $appcastSignature = & netsparkle-generate-appcast `
        --generate-signature $appcastPath `
        --private-key-override ((Get-Content $PrivateKeyPath -Raw).Trim()) `
        --public-key-override ((Get-Content $PublicKeyPath -Raw).Trim())

    $signatureValue = ($appcastSignature | Select-String -Pattern '^Signature:\s*(.+)$').Matches.Groups[1].Value.Trim()
    if ([string]::IsNullOrWhiteSpace($signatureValue)) {
        throw "Failed to regenerate appcast signature for $appcastPath"
    }

    $appcastSignaturePath = "$appcastPath.signature"
    Set-Content -Path $appcastSignaturePath -Value $signatureValue -NoNewline -Encoding utf8
    Write-Host "[update] Regenerated appcast signature: $appcastSignaturePath"
}

Write-Host "[update] Appcast generation complete."
