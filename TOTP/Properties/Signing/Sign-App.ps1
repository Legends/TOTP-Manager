# Change these values as needed
# Execute in root folder "TOTP" to sign the assembly with a PFX certificate.
# CMD: powershell -file E:\Repos\Github2FA\TOTP\Properties\Signing\Sign-App.ps1
# or:
# PS: .\Sign-App.ps1
$SignToolPath = "C:\Program Files (x86)\Windows Kits\10\bin\10.0.26100.0\x64\signtool.exe"
$CertificatePath = "totp-signing-cert.pfx"
$CertificatePassword = "freerunner"
$TimestampUrl = "http://timestamp.digicert.com"

# Search for your built .exe file dynamically
$ExePath = Get-ChildItem -Path ".\bin\Release" -Recurse -Filter "TOTP.Manager.exe" | Select-Object -First 1

#try

if (-not $ExePath) {
    Write-Error "❌ Executable not found. Did you build the project in Release mode?"
    exit 1
}

Write-Host "✅ Found executable at:" $ExePath.FullName

# Run SignTool
& "$SignToolPath" sign `
    /f "$CertificatePath" `
    /p "$CertificatePassword" `
    /tr "$TimestampUrl" `
    /td sha256 `
    /fd sha256 `
    "$($ExePath.FullName)"

if ($LASTEXITCODE -eq 0) {
    Write-Host "🎉 Successfully signed the executable."
} else {
    Write-Error "❌ Signing failed with exit code $LASTEXITCODE"
}

# Falls du später mehrere Dateien signieren willst (z. B. DLLs oder MSIX), kannst du das Get-ChildItem entsprechend anpassen:
# $FilesToSign = Get-ChildItem -Path ".\bin\Release" -Recurse -Include "*.exe", "*.dll"

# foreach ($file in $FilesToSign) {
#     & "$SignToolPath" sign `
#         /f "$CertificatePath" `
#         /p "$CertificatePassword" `
#         /tr "$TimestampUrl" `
#         /td sha256 `
#         /fd sha256 `
#         "$($file.FullName)"
# }
