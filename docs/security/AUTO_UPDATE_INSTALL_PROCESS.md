# Auto-Update Install Process

When you click `Install update` in the current custom updater flow, this is what happens:

1. The click lands in `TOTPDownloadProgressWindow.xaml.cs`.
2. If the download is marked complete and valid, it calls the custom handler from `AutoUpdateService.cs`: `InstallDownloadedUpdateAsync(...)`.
3. That handler checks the downloaded payload path.
   It accepts either:
   - a `.zip` file
   - a temp directory containing already-extracted update files
4. It resolves:
   - the current install directory from the running app assembly location
   - the current executable path from the running process
5. It copies the bundled `TOTP.Updater` runtime into a fresh temp directory under `%TEMP%`.
6. It starts `TOTP.Updater.exe` from that temp runtime and passes:
   - package path
   - target install dir
   - current exe path
   - current app PID
   - helper log path
   - a ready-signal path used to coordinate handoff
7. The main app waits until the updater window has signaled that it is visible.
8. Once the updater is visible, the main app closes its progress window and yields control.

Then the dedicated updater process takes over:

1. It shows its own install progress window and signals readiness.
2. It requests the parent TOTP process to close.
3. It stages the update payload:
   - if the package path is a directory, it copies that directory into staging
   - if it is a zip file, it expands the zip into staging
4. It walks every staged file and copies it into the target install directory, creating subfolders as needed.
   There is retry logic for locked files, and the window updates progress while files are being replaced.
5. It computes the target exe path inside the install directory.
6. It starts the updated app from that target location.
7. It logs progress to `%TEMP%\totp-update-helper.log`.
8. It removes the temporary staging directory and schedules cleanup of its temp runtime folder.

The intended behavior is:

- no external installer is launched
- the installed app folder is updated in place
- the app is relaunched from the updated install location
- the updater window is visible before the main app disappears
- install feedback remains visible after the main app exits

Relevant code:

- `TOTP/AutoUpdate/TOTPDownloadProgressWindow.xaml.cs`
- `TOTP.Updater/Program.cs`
- `TOTP/Services/AutoUpdateService.cs`
