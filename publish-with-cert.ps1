$certPath = Read-Host "Path to certificate"
$pwd = Read-Host "Enter certificate password" -AsSecureString
.\publish.ps1 -CertPath $certPath -CertPassword $pwd
