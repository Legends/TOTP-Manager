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
5. It writes a temporary PowerShell helper script into `%TEMP%`.
6. It starts that helper script in a separate process and passes:
   - package path
   - target install dir
   - current exe path
   - current app PID
   - temp staging dir
   - helper log path
7. After the helper process starts successfully, the app shuts itself down via WPF `Application.Shutdown()`.

Then the helper script takes over:

1. It waits for the parent app process to exit.
2. It clears any old staging directory.
3. It stages the update payload:
   - if the package path is a directory, it copies that directory into staging
   - if it is a zip file, it expands the zip into staging
4. It walks every staged file and copies it into the target install directory, creating subfolders as needed.
   There is retry logic for locked files.
5. It computes the target exe path inside the install directory.
6. It starts the updated app from that target location.
7. It logs progress to `%TEMP%\totp-update-helper.log`.
8. It removes the temporary staging directory.

The intended behavior is:

- no external installer is launched
- the installed app folder is updated in place
- the app is relaunched from the updated install location

Relevant code:

- `TOTP/AutoUpdate/TOTPDownloadProgressWindow.xaml.cs`
- `TOTP/Services/AutoUpdateService.cs`
