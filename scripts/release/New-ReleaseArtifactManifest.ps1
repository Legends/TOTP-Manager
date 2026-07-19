param(
    [Parameter(Mandatory = $true)]
    [string]$ReleaseVersion,

    [Parameter(Mandatory = $true)]
    [string]$SourceCommit,

    [Parameter(Mandatory = $true)]
    [string[]]$ArtifactPath,

    [Parameter(Mandatory = $true)]
    [string]$OutputPath
)

$ErrorActionPreference = "Stop"

if ($ReleaseVersion -notmatch '^\d+\.\d+\.\d+(?:-rc\d+)?$') {
    throw "ReleaseVersion must match <major>.<minor>.<patch>[-rc<nr>]."
}
if ($SourceCommit -notmatch '^[0-9a-fA-F]{40}$') {
    throw "SourceCommit must be a full 40-character Git commit SHA."
}
if ($ArtifactPath.Count -eq 0) {
    throw "At least one artifact is required."
}

function Get-ArtifactTarget {
    param([string]$FileName)

    switch -Regex ($FileName) {
        '^TOTP-Manager-(?:windows-x64-[0-9A-Za-z.-]+|fast|portable)\.zip$' {
            return [ordered]@{
                operatingSystem = "windows"
                architecture = "x64"
                format = "zip"
                ownership = "application"
                updatePolicy = "signed-appcast"
            }
        }
        '^TOTP-Manager-macos-arm64-[0-9A-Za-z.-]+\.dmg$' {
            return [ordered]@{
                operatingSystem = "macos"
                architecture = "arm64"
                format = "dmg"
                ownership = "application"
                updatePolicy = "manual-signed-release"
            }
        }
        '^TOTP-Manager-linux-x64-[0-9A-Za-z.-]+\.tar\.gz$' {
            return [ordered]@{
                operatingSystem = "linux"
                architecture = "x64"
                format = "tar.gz"
                ownership = "application"
                updatePolicy = "manual-signed-release"
            }
        }
        '^totp-manager_[0-9A-Za-z.+~-]+_amd64\.deb$' {
            return [ordered]@{
                operatingSystem = "linux"
                architecture = "x64"
                format = "deb"
                ownership = "package-manager"
                updatePolicy = "package-manager"
            }
        }
        default {
            throw "Artifact name does not match a supported release target: $FileName"
        }
    }
}

$resolvedOutput = [IO.Path]::GetFullPath($OutputPath)
$outputDirectory = [IO.Path]::GetDirectoryName($resolvedOutput)
if ([string]::IsNullOrWhiteSpace($outputDirectory)) {
    throw "OutputPath must include a valid parent directory."
}
[IO.Directory]::CreateDirectory($outputDirectory) | Out-Null

$seenNames = [Collections.Generic.HashSet[string]]::new([StringComparer]::OrdinalIgnoreCase)
$artifacts = foreach ($path in $ArtifactPath) {
    $resolved = (Resolve-Path -LiteralPath $path).Path
    $file = Get-Item -LiteralPath $resolved
    if (-not $file.PSIsContainer -and $file.Length -gt 0) {
        if (-not $seenNames.Add($file.Name)) {
            throw "Artifact names must be unique: $($file.Name)"
        }
        $target = Get-ArtifactTarget $file.Name
        [ordered]@{
            fileName = $file.Name
            operatingSystem = $target.operatingSystem
            architecture = $target.architecture
            format = $target.format
            bytes = $file.Length
            sha256 = (Get-FileHash -LiteralPath $resolved -Algorithm SHA256).Hash.ToLowerInvariant()
            ownership = $target.ownership
            updatePolicy = $target.updatePolicy
        }
    }
    else {
        throw "Artifact must be a non-empty regular file: $path"
    }
}

$manifest = [ordered]@{
    schemaVersion = 1
    releaseVersion = $ReleaseVersion
    sourceCommit = $SourceCommit.ToLowerInvariant()
    artifacts = @($artifacts | Sort-Object fileName)
}
$json = $manifest | ConvertTo-Json -Depth 5
[IO.File]::WriteAllText($resolvedOutput, "$json`n", [Text.UTF8Encoding]::new($false))
Write-Output $resolvedOutput
