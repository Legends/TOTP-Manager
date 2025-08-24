## Trigger a github release via github workflow

The workflow file is here:

 "TOTP-Manager-App\.github\workflows\build-and-test.yml"

 `#` Step 1: Tag the current commit

 `#` Step 2: Push the tag to GitHub

`git tag v1.0.0`

`git push origin v1.0.0`

## Windows Icon Cache Not Refreshed

Windows caches icons aggressively. Try this first:

🔄 Refresh Windows Explorer icon cache
Press Ctrl + F5 in the folder where your .exe is.

If that doesn’t help:

Open cmd.exe as Administrator.

Run:

 ie4uinit.exe -ClearIconCache
 taskkill /IM explorer.exe /F
 start explorer.exe

Or delete icon cache manually:

 del %localappdata%\IconCache.db /a

## "Send to Desktop" Quirk

When you "Send to Desktop", Windows uses cached metadata from a prior version of the file — or even old icon data embedded in the PE header.

To fix:

Delete the old shortcut from the desktop.

Create a new shortcut manually:

>Right-click desktop → New → Shortcut

Point to your TOTP.UI.WPF.exe

After it's created, right-click → Properties → Confirm the icon is correct

If still wrong, click "Change Icon" again → Re-select the one embedded.
