$ErrorActionPreference = "Stop"
Set-StrictMode -Version Latest

function Normalize-Version {
    param(
        [Parameter(Mandatory = $true)]
        [version]$Version
    )

    $build = if ($Version.Build -lt 0) { 0 } else { $Version.Build }
    $revision = if ($Version.Revision -lt 0) { 0 } else { $Version.Revision }
    return [version]::new($Version.Major, $Version.Minor, $build, $revision)
}

function Get-VersionTagInfo {
    $pattern = '^v(?<version>\d+\.\d+\.\d+(?:\.\d+)?)(?:-rc(?<rc>\d+))?$'

    git tag -l "v*" |
        ForEach-Object {
            if ($_ -match $pattern) {
                $rcValue = if ($matches.ContainsKey('rc')) { $matches['rc'] } else { $null }

                [pscustomobject]@{
                    Tag = $_
                    RawVersion = $matches.version
                    Version = Normalize-Version ([version]$matches.version)
                    IsRc = [string]::IsNullOrWhiteSpace($rcValue) -eq $false
                    RcNumber = if ([string]::IsNullOrWhiteSpace($rcValue)) { 0 } else { [int]$rcValue }
                }
            }
        }
}

function Read-Choice {
    param(
        [Parameter(Mandatory = $true)]
        [string]$Prompt,

        [Parameter(Mandatory = $true)]
        [string[]]$ValidChoices
    )

    while ($true) {
        $choice = Read-Host $Prompt
        if ($ValidChoices -contains $choice) {
            return $choice
        }

        Write-Host "Invalid choice. Valid options: $($ValidChoices -join ', ')" -ForegroundColor Yellow
    }
}

function Read-CustomVersion {
    while ($true) {
        $inputVersion = Read-Host "Enter a version number (examples: 1.2.3 or 1.2.3.4)"
        if ([string]::IsNullOrWhiteSpace($inputVersion)) {
            Write-Host "Version cannot be empty." -ForegroundColor Yellow
            continue
        }

        try {
            return Normalize-Version ([version]$inputVersion.Trim())
        }
        catch {
            Write-Host "Invalid version format. Use numeric versions like 1.2.3 or 1.2.3.4." -ForegroundColor Yellow
        }
    }
}

function Read-CommitMessage {
    param(
        [Parameter(Mandatory = $true)]
        [string]$DefaultMessage
    )

    while ($true) {
        $message = Read-Host "Enter commit message or press Enter to use '$DefaultMessage'"
        if ([string]::IsNullOrWhiteSpace($message)) {
            return $DefaultMessage
        }

        $trimmedMessage = $message.Trim()
        if (-not [string]::IsNullOrWhiteSpace($trimmedMessage)) {
            return $trimmedMessage
        }
    }
}

function Get-NextRcTag {
    param(
        [Parameter(Mandatory = $false)]
        $LatestRcInfo,

        [Parameter(Mandatory = $true)]
        [version]$BaseVersion
    )

    if ($null -ne $LatestRcInfo) {
        return "v$($LatestRcInfo.RawVersion)-rc$($LatestRcInfo.RcNumber + 1)"
    }

    $rcBase = "{0}.{1}.{2}" -f $BaseVersion.Major, $BaseVersion.Minor, $BaseVersion.Build
    return "v$rcBase-rc1"
}

function Format-StableTag {
    param(
        [Parameter(Mandatory = $true)]
        [version]$Version
    )

    return "v$($Version.ToString(4))"
}

$tagInfos = @(Get-VersionTagInfo)
$latestStable = $tagInfos |
    Where-Object { -not $_.IsRc } |
    Sort-Object Version |
    Select-Object -Last 1
$latestRc = $tagInfos |
    Where-Object { $_.IsRc } |
    Sort-Object Version, RcNumber |
    Select-Object -Last 1

$currentVersion = if ($null -ne $latestStable) {
    $latestStable.Version
}
elseif ($null -ne $latestRc) {
    $latestRc.Version
}
else {
    [version]::new(0, 0, 0, 0)
}

$majorVersion = [version]::new($currentVersion.Major + 1, 0, 0, 0)
$minorVersion = [version]::new($currentVersion.Major, $currentVersion.Minor + 1, 0, 0)
$patchVersion = [version]::new($currentVersion.Major, $currentVersion.Minor, $currentVersion.Build + 1, 0)
$revisionVersion = [version]::new($currentVersion.Major, $currentVersion.Minor, $currentVersion.Build, $currentVersion.Revision + 1)
$nextRcTag = Get-NextRcTag -LatestRcInfo $latestRc -BaseVersion $currentVersion

Write-Host ""
Write-Host "Version overview" -ForegroundColor Cyan
Write-Host "Latest stable tag : $(if ($latestStable) { $latestStable.Tag } else { 'none found' })"
Write-Host "Latest RC tag     : $(if ($latestRc) { $latestRc.Tag } else { 'none found' })"
Write-Host "Base version      : $($currentVersion.ToString(4))"
Write-Host ""

while ($true) {
    Write-Host "Choose the next tag version:" -ForegroundColor Cyan
    Write-Host "1. Major    -> $(Format-StableTag $majorVersion)"
    Write-Host "2. Minor    -> $(Format-StableTag $minorVersion)"
    Write-Host "3. Patch    -> $(Format-StableTag $patchVersion)"
    Write-Host "4. Custom   -> enter a version manually"
    Write-Host "5. Revision -> $(Format-StableTag $revisionVersion)"
    Write-Host "6. Next RC  -> $nextRcTag"
    Write-Host ""

    $versionChoice = Read-Choice -Prompt "Select an option" -ValidChoices @("1", "2", "3", "4", "5", "6")

    $selectedTag = switch ($versionChoice) {
        "1" { Format-StableTag $majorVersion }
        "2" { Format-StableTag $minorVersion }
        "3" { Format-StableTag $patchVersion }
        "4" { Format-StableTag (Read-CustomVersion) }
        "5" { Format-StableTag $revisionVersion }
        "6" { $nextRcTag }
    }

    if ($tagInfos.Tag -contains $selectedTag) {
        Write-Host ""
        Write-Host "Tag $selectedTag already exists. Choose another version." -ForegroundColor Yellow
        Write-Host ""
        continue
    }

    Write-Host ""
    Write-Host "Chosen version: $selectedTag" -ForegroundColor Green
    Write-Host "1. Yes     -> commit pending changes if needed, push branch, create and push tag"
    Write-Host "2. Changes -> choose a different version"
    Write-Host "3. Abort   -> stop the tagging process"
    Write-Host ""

    $confirmationChoice = Read-Choice -Prompt "Select an option" -ValidChoices @("1", "2", "3")

    if ($confirmationChoice -eq "2") {
        Write-Host ""
        continue
    }

    if ($confirmationChoice -eq "3") {
        Write-Host "Tagging process aborted." -ForegroundColor Yellow
        return
    }

    $status = git status --porcelain
    if ($status) {
        $branch = git branch --show-current
        if ([string]::IsNullOrWhiteSpace($branch)) {
            throw "Unable to determine the current branch for push."
        }

        $commitMessage = Read-CommitMessage -DefaultMessage "chore: prepare release $selectedTag"
        Write-Host "Uncommitted changes found. Committing and pushing branch $branch..." -ForegroundColor Yellow
        git add .
        git commit -m $commitMessage
        git push origin $branch
    }
    else {
        Write-Host "Working directory is clean. Skipping commit phase." -ForegroundColor Green
    }

    $tagMessage = if ($selectedTag -match '-rc(\d+)$') {
        "Release candidate $($matches[1])"
    }
    else {
        "Release $selectedTag"
    }

    Write-Host "Creating and pushing tag $selectedTag..." -ForegroundColor Cyan
    git tag -a $selectedTag -m $tagMessage
    git push origin $selectedTag

    Write-Host "Tag pushed successfully: $selectedTag" -ForegroundColor Green
    return
}
