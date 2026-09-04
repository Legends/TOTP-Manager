Set-StrictMode -Version Latest
$ErrorActionPreference = 'Stop'

$repositoryRoot = (Resolve-Path -LiteralPath (Join-Path $PSScriptRoot '../..')).Path

function Read-RequiredFile {
    param([Parameter(Mandatory)][string]$RelativePath)

    $path = Join-Path $repositoryRoot $RelativePath
    if (-not (Test-Path -LiteralPath $path -PathType Leaf)) {
        throw "Required website file is missing: $RelativePath"
    }
    return [IO.File]::ReadAllText($path)
}

$index = Read-RequiredFile 'site/index.html'
$styles = Read-RequiredFile 'site/styles.css'
$robots = Read-RequiredFile 'site/robots.txt'
$sitemap = Read-RequiredFile 'site/sitemap.xml'
Read-RequiredFile 'docs/images/readme/app.png' | Out-Null
Read-RequiredFile 'TOTP.UI.Avalonia.Desktop/Assets/Icons/app-1024.png' | Out-Null

$socialPreviewPath = Join-Path $repositoryRoot 'docs/images/social/otp-harbor-social-preview.jpg'
if (-not (Test-Path -LiteralPath $socialPreviewPath -PathType Leaf)) {
    throw 'The optimized social-preview image is missing.'
}
if ((Get-Item -LiteralPath $socialPreviewPath).Length -ge 1MB) {
    throw 'The social-preview image must remain smaller than 1 MB for GitHub upload.'
}

foreach ($requiredText in @(
    '<title>OTP Harbor — Local-first TOTP authenticator</title>',
    '<meta name="google-site-verification" content="I36j8PWZYmhKsRKKNVM-fmcGW7wXbJ10fmbOe_4Az0U">',
    '<link rel="canonical" href="https://legends.github.io/otp-harbor/">',
    '<meta property="og:image" content="https://legends.github.io/otp-harbor/assets/social-preview.jpg">',
    'type="application/ld+json"',
    '"@type": "SoftwareApplication"',
    'Microsoft Store is the planned primary Windows channel',
    'Current GitHub packages are explicitly labeled manual previews',
    'Can you test OTP Harbor on a MacBook?',
    'Test only with synthetic accounts'
)) {
    if (-not $index.Contains($requiredText, [StringComparison]::Ordinal)) {
        throw "The public website is missing required content: $requiredText"
    }
}

if ($index -match 'aggregateRating|reviewCount|downloadCount|google-analytics|googletagmanager') {
    throw 'The website must not publish unverified popularity signals or analytics.'
}
if (-not $styles.Contains('prefers-reduced-motion', [StringComparison]::Ordinal)) {
    throw 'The website is missing its reduced-motion accessibility rule.'
}
if (-not $robots.Contains('Sitemap: https://legends.github.io/otp-harbor/sitemap.xml', [StringComparison]::Ordinal) -or
    -not $sitemap.Contains('<loc>https://legends.github.io/otp-harbor/</loc>', [StringComparison]::Ordinal)) {
    throw 'The website crawl metadata does not use the canonical Pages URL.'
}

Write-Output 'Public website content, trust disclosure, and crawl metadata are present.'
