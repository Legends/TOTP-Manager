$ErrorActionPreference = "Stop"

$testRoot = Join-Path ([IO.Path]::GetTempPath()) ("totp-unsigned-preview-" + [Guid]::NewGuid().ToString("N"))
$packageRoot = Join-Path $testRoot "package"
$artifactRoot = Join-Path $testRoot "artifacts"

try {
    New-Item -ItemType Directory -Path $packageRoot, $artifactRoot | Out-Null
    $settings = @{
        AutoUpdate = @{
            Enabled = $true
            AppcastUrl = "https://example.invalid/appcast-v2.xml"
            PublicKey = "synthetic-test-key"
            Channel = "stable"
            DistributionMode = "direct"
        }
    } | ConvertTo-Json -Depth 3
    [IO.File]::WriteAllText(
        (Join-Path $packageRoot "appsettings.json"),
        "$settings`n",
        [Text.UTF8Encoding]::new($false))

    & (Join-Path $PSScriptRoot "../release/Set-PackageUpdatePolicy.ps1") `
        -PackageDirectory $packageRoot `
        -DistributionMode direct `
        -Channel rc `
        -DisableUpdates

    $configured = Get-Content (Join-Path $packageRoot "appsettings.json") -Raw | ConvertFrom-Json
    if ($configured.AutoUpdate.Enabled -ne $false -or
        $configured.AutoUpdate.Channel -ne "rc" -or
        $configured.AutoUpdate.DistributionMode -ne "direct") {
        throw "Unsigned preview package policy did not disable automatic updates."
    }

    $artifactNames = @(
        "TOTP-Manager-windows-x64-2.0.0-rc3.zip",
        "TOTP-Manager-windows-x64-fast-2.0.0-rc3.zip",
        "TOTP-Manager-linux-x64-2.0.0-rc3.tar.gz",
        "totp-manager_2.0.0-rc3_amd64.deb")
    $artifactPaths = foreach ($name in $artifactNames) {
        $path = Join-Path $artifactRoot $name
        [IO.File]::WriteAllBytes($path, [byte[]](1, 2, 3, 4))
        $path
    }

    $manifestPath = Join-Path $artifactRoot "release-artifacts-unsigned-preview.json"
    & (Join-Path $PSScriptRoot "../release/New-ReleaseArtifactManifest.ps1") `
        -ReleaseVersion "2.0.0-rc3" `
        -SourceCommit ("a" * 40) `
        -ArtifactPath $artifactPaths `
        -OutputPath $manifestPath `
        -ReleaseProfile unsigned-preview | Out-Null
    & (Join-Path $PSScriptRoot "../release/Test-ReleaseArtifactManifest.ps1") `
        -ManifestPath $manifestPath `
        -ArtifactDirectory $artifactRoot | Out-Null

    $manifest = Get-Content $manifestPath -Raw | ConvertFrom-Json
    if ($manifest.releaseProfile -ne "unsigned-preview" -or
        @($manifest.artifacts).Count -ne 4 -or
        @($manifest.artifacts | Where-Object { $_.fileName -match '~' }).Count -ne 0 -or
        @($manifest.artifacts | Where-Object {
            $_.updatePolicy -ne "unsigned-preview-manual-download"
        }).Count -ne 0) {
        throw "Unsigned preview manifest contains an unsafe update policy."
    }

    Write-Output "Unsigned preview release policy is valid."
}
finally {
    if (Test-Path -LiteralPath $testRoot) {
        Remove-Item -LiteralPath $testRoot -Recurse -Force
    }
}
