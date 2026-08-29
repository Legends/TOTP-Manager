Set-StrictMode -Version Latest
$ErrorActionPreference = "Stop"

$repositoryRoot = (Resolve-Path -LiteralPath (Join-Path $PSScriptRoot "../..")).Path

function Read-RepositoryFile {
    param([Parameter(Mandatory = $true)][string]$RelativePath)

    $path = Join-Path $repositoryRoot $RelativePath
    if (-not (Test-Path -LiteralPath $path -PathType Leaf)) {
        throw "Required SignPath readiness file is missing: $RelativePath"
    }

    return [IO.File]::ReadAllText($path)
}

$readme = Read-RepositoryFile "readme.md"
$policy = Read-RepositoryFile "CODE_SIGNING_POLICY.md"
$privacy = Read-RepositoryFile "PRIVACY.md"
$workflow = Read-RepositoryFile ".github/workflows/build-and-test.yml"
$codeOwners = Read-RepositoryFile ".github/CODEOWNERS"
$buildMetadata = Read-RepositoryFile "Directory.Build.props"
$testProject = Read-RepositoryFile "TOTP.Tests/TOTP.Tests.csproj"
$desktopProject = Read-RepositoryFile "TOTP.UI.Avalonia.Desktop/TOTP.UI.Avalonia.Desktop.csproj"
$assetProvenance = Read-RepositoryFile "docs/assets/ASSET_PROVENANCE.md"

$requiredReadmeText = @(
    "## Code signing policy",
    "Free code signing provided by [SignPath.io](https://signpath.io/), certificate by [SignPath Foundation](https://signpath.org/).",
    "[TOTP Manager privacy policy](PRIVACY.md)"
)
foreach ($requiredText in $requiredReadmeText) {
    if (-not $readme.Contains($requiredText, [StringComparison]::Ordinal)) {
        throw "The README is missing required code-signing text: $requiredText"
    }
}

foreach ($requiredText in @("## Team roles", "## Build and signing controls", "## Privacy")) {
    if (-not $policy.Contains($requiredText, [StringComparison]::Ordinal)) {
        throw "The code signing policy is missing: $requiredText"
    }
}
if (-not $privacy.Contains("does not contain telemetry", [StringComparison]::Ordinal)) {
    throw "The privacy policy does not state the telemetry posture."
}
if (-not $codeOwners.Contains("/.github/workflows/ @Legends", [StringComparison]::Ordinal)) {
    throw "Release workflows are not covered by CODEOWNERS."
}
if (-not $buildMetadata.Contains("<Product>TOTP Manager</Product>", [StringComparison]::Ordinal)) {
    throw "First-party PE product metadata is not centrally defined."
}
if ($testProject -notmatch '<PackageReference Include="FluentAssertions" Version="7\.[^"]+"') {
    throw "FluentAssertions must remain on the OSI-approved 7.x line."
}
if (($desktopProject.Split('IncludeSourceRevisionInInformationalVersion=$(IncludeSourceRevisionInInformationalVersion)').Count - 1) -lt 2 -or
    ($desktopProject.Split('RemoveProperties="RuntimeIdentifier;SelfContained;PublishSingleFile;PublishTrimmed"').Count - 1) -lt 2) {
    throw "The updater build does not consistently inherit release metadata independently of the desktop runtime identifier."
}

$reviewedAssets = [ordered]@{
    "TOTP.UI.Avalonia.Desktop/Assets/Icons/app-1024.png" = "49ad29d8b5a28a0529f4128eaa89d8000ee872974930f1c7ab9c66f9c55c4e54"
    "TOTP.UI.Avalonia.Desktop/Assets/Icons/app-128.png"  = "21cb7e3410deaab80b220ed031f942d0129f48a4eba36886f129252d64312420"
    "TOTP.UI.Avalonia.Desktop/Assets/Icons/app.ico"      = "b535e5c46ff90a9ebccf0dec0da40abc74dae7ff73de25e50e72f1b3a8c86141"
    "TOTP.UI.Avalonia.Desktop/Assets/flags/en.png"       = "1c2bcc20e5985e5f03a3a440f198b5d08a4ac609e9cebba00b639b0e50fba8fc"
    "TOTP.UI.Avalonia.Desktop/Assets/flags/de.png"       = "2c8f253f3401d18df0a47bd7906102cf78ea7e4a2caac9e4c6f4efebc906de0a"
}
foreach ($entry in $reviewedAssets.GetEnumerator()) {
    $assetPath = Join-Path $repositoryRoot $entry.Key
    if (-not (Test-Path -LiteralPath $assetPath -PathType Leaf)) {
        throw "Reviewed project asset is missing: $($entry.Key)"
    }
    $actualHash = (Get-FileHash -LiteralPath $assetPath -Algorithm SHA256).Hash.ToLowerInvariant()
    if ($actualHash -cne $entry.Value) {
        throw "Project asset changed without a matching provenance review: $($entry.Key)"
    }
    if (-not $assetProvenance.Contains($entry.Value, [StringComparison]::Ordinal)) {
        throw "The asset provenance record is missing the reviewed hash for $($entry.Key)."
    }
}

$requiredWorkflowText = @(
    "signpath/github-action-submit-signing-request@c92b958760219087e01f8d67a1669ed57afe2627",
    "signing-policy-slug: release-signing",
    "artifact-configuration-slug: windows-release-v1",
    "Get-AuthenticodeSignature"
)
foreach ($requiredText in $requiredWorkflowText) {
    if (-not $workflow.Contains($requiredText, [StringComparison]::Ordinal)) {
        throw "The release workflow is missing the SignPath control: $requiredText"
    }
}
if ($workflow -match 'SIGNING_CERT_(BASE64|PASSWORD)') {
    throw "The hosted release workflow must not accept exportable Windows certificate material."
}

Write-Output "SignPath Foundation repository-readiness controls are present."
