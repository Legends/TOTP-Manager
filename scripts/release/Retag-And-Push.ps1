param(
    [Parameter(Mandatory = $true)]
    [string]$Tag
)

$ErrorActionPreference = "Stop"

function Step {
    param([string]$Message)
    Write-Host "[retag] $Message"
}

function Exec-Git {
    param([Parameter(ValueFromRemainingArguments = $true)][string[]]$Args)
    & git @Args
    if ($LASTEXITCODE -ne 0) {
        throw "git $($Args -join ' ') failed with exit code $LASTEXITCODE"
    }
}

if ([string]::IsNullOrWhiteSpace($Tag)) {
    throw "Tag must not be empty."
}

if ($Tag -notmatch '^v[0-9]+(\.[0-9]+){1,3}([\-\.][A-Za-z0-9]+)*$') {
    throw "Tag '$Tag' does not look like a release tag (example: v1.0.0-rc1)."
}

Step "Pushing latest master to origin..."
Exec-Git push origin master

Step "Deleting remote tag (if exists): $Tag"
& git push origin ":refs/tags/$Tag"
if ($LASTEXITCODE -ne 0) {
    throw "Failed deleting remote tag '$Tag'"
}

Step "Recreating local tag at current HEAD: $Tag"
Exec-Git tag -f $Tag

Step "Pushing tag: $Tag"
Exec-Git push origin $Tag

Step "Done. Check GitHub Actions for tag '$Tag'."
