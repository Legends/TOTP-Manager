[Convert]::ToBase64String([IO.File]::ReadAllBytes("totp-signing-cert.pfx")) | Set-Content "cert.txt"
