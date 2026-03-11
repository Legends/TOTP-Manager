$folderName = "TOTP-Manager-fast_rc50"
$sourcePath = "C:\Users\Joca\Downloads\TOTP-Manager-fast_rc50"
$destPath   = Join-Path "D:\" $folderName
$exePath    = Join-Path $destPath "TOTP.UI.WPF.exe"

if (-not (Test-Path $sourcePath)) {
    throw "Source folder not found: $sourcePath"
}

# Delete destination folder if it already exists
if (Test-Path $destPath) {
    Write-Host "Deleting existing folder: $destPath"

    attrib -r -h -s "$destPath\*" /s /d 2>$null
    Remove-Item $destPath -Recurse -Force -ErrorAction SilentlyContinue

    if (Test-Path $destPath) {
        Write-Host "Fallback delete via rd..."
        cmd /c "rd /s /q `"$destPath`""
    }

    if (Test-Path $destPath) {
        throw "Failed to delete destination folder: $destPath"
    }
}

Write-Host "Copying folder to D:\ ..."
Copy-Item -Path $sourcePath -Destination "D:\" -Recurse -Force

if (-not (Test-Path $exePath)) {
    throw "EXE not found after copy: $exePath"
}

Write-Host "Unblocking EXE: $exePath"
Unblock-File -Path $exePath

Write-Host "Starting EXE: $exePath"
Start-Process -FilePath $exePath

Write-Host "Done."