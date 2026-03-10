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

Write-Host "[update] Generating appcast for $ReleaseFolder"
Write-Host "[update] Base download URL: $BaseDownloadUrl"
if (-not [string]::IsNullOrWhiteSpace($FileVersion)) {
    Write-Host "[update] Override appcast version: $FileVersion"
}
if (-not [string]::IsNullOrWhiteSpace($DisplayVersion)) {
    Write-Host "[update] Override appcast display version: $DisplayVersion"
}

# Requires: dotnet tool install --global NetSparkleUpdater.Tools.AppCastGenerator
$arguments = @(
    '--binaries', "$ReleaseFolder",
    '--ext', 'exe',
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

netsparkle-generate-appcast @arguments

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

Write-Host "[update] Appcast generation complete."
