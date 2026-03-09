# 1. Commit and push current changes
$status = git status --porcelain
if ($status) {
    Write-Host "Uncommitted changes found. Committing and pushing..." -ForegroundColor Yellow
    git add .
    git commit -m "chore: updates before release candidate"
    git push origin (git branch --show-current)
} else {
    Write-Host "Clean working directory. Skipping commit phase..." -ForegroundColor Green
}

# 2. Get the latest tag matching the RC pattern
# Sorts by version number and then by the RC integer
$latestTag = git tag -l "v*-rc*" | Sort-Object { [version]($_ -replace '^v|(-rc.*)$','') }, { [int]($_ -replace '.*-rc','') } | Select-Object -Last 1

if (-not $latestTag) {
    Write-Error "No existing RC tags found. Please create the first one manually (e.g., v1.0.0-rc1)."
    return
}

# 3. Calculate new version numbers
$baseVersion = $latestTag -replace '-rc\d+$', ''
$currentRcNum = [int]($latestTag -replace '.*-rc', '')
$newRcNum = $currentRcNum + 1
$newTag = "$baseVersion-rc$newRcNum"

Write-Host "Creating and pushing tag: $newTag" -ForegroundColor Cyan

# 4. Create the tag and push it
git tag -a $newTag -m "Release candidate $newRcNum"
git push origin $newTag