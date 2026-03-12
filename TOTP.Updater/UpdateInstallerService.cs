using System.Diagnostics;
using System.IO;
using System.IO.Compression;
using System.Text;

namespace TOTP.Updater;

internal sealed class UpdateInstallerService(UpdateInstallArguments arguments)
{
    private bool _readySignalWritten;

    public async Task RunAsync(IProgress<InstallerProgressState> progress, CancellationToken cancellationToken = default)
    {
        CleanupOldTemporaryFolders();
        Directory.CreateDirectory(Path.GetDirectoryName(arguments.LogPath)!);
        Log("updater started");
        SignalReady();

        progress.Report(CreateState("Installing update", "Closing TOTP Manager...", arguments.PackagePath, string.Empty, true, 0, false));
        await CloseParentApplicationAsync(arguments.ParentProcessId, cancellationToken);

        progress.Report(CreateState("Installing update", "Waiting for the app to close...", arguments.PackagePath, string.Empty, true, 0, false));
        await WaitForParentProcessExitAsync(arguments.ParentProcessId, cancellationToken);

        progress.Report(CreateState("Installing update", "Staging update package...", arguments.PackagePath, string.Empty, true, 0, false));
        var stageDirectory = Path.Combine(Path.GetTempPath(), $"totp-update-stage-{Guid.NewGuid():N}");

        try
        {
            await StagePackageAsync(stageDirectory, cancellationToken);
            var files = EnumerateStageFiles(stageDirectory);
            var totalBytes = files.Sum(static file => file.Length);

            progress.Report(CreateState("Installing update", "Installing files...", $"{files.Count} file(s) queued", $"{files.Count} item(s) queued", false, 0, false));
            await CopyFilesAsync(files, stageDirectory, arguments.TargetDirectory, totalBytes, progress, cancellationToken);

            var relaunchTarget = Path.Combine(arguments.TargetDirectory, Path.GetFileName(arguments.ExecutablePath));
            progress.Report(CreateState("Installing update", "Relaunching app...", relaunchTarget, "100% complete", false, 100, false));
            RelaunchApplication();
            Log("application relaunched");
            ScheduleSelfCleanup();
        }
        finally
        {
            TryDeleteDirectory(stageDirectory);
        }
    }

    public void LogFailure(Exception exception)
    {
        Log($"updater failed: {exception}");
    }

    private static InstallerProgressState CreateState(
        string title,
        string status,
        string detail,
        string progressText,
        bool isIndeterminate,
        int progressValue,
        bool isCloseEnabled)
    {
        return new InstallerProgressState
        {
            Title = title,
            Status = status,
            Detail = detail,
            ProgressText = progressText,
            IsIndeterminate = isIndeterminate,
            ProgressValue = progressValue,
            IsCloseEnabled = isCloseEnabled
        };
    }

    private async Task WaitForParentProcessExitAsync(int parentProcessId, CancellationToken cancellationToken)
    {
        try
        {
            using var process = Process.GetProcessById(parentProcessId);
            await process.WaitForExitAsync(cancellationToken);
            Log($"parent process exited: {parentProcessId}");
        }
        catch (ArgumentException)
        {
            Log($"parent process already exited: {parentProcessId}");
        }
    }

    private async Task CloseParentApplicationAsync(int parentProcessId, CancellationToken cancellationToken)
    {
        try
        {
            using var process = Process.GetProcessById(parentProcessId);
            if (process.HasExited)
            {
                Log($"parent process already closed before close request: {parentProcessId}");
                return;
            }

            process.CloseMainWindow();
            Log($"parent process close requested: {parentProcessId}");

            var deadline = DateTimeOffset.UtcNow.AddSeconds(5);
            while (!process.HasExited && DateTimeOffset.UtcNow < deadline)
            {
                cancellationToken.ThrowIfCancellationRequested();
                await Task.Delay(100, cancellationToken);
            }
        }
        catch (ArgumentException)
        {
            Log($"parent process already exited before close request: {parentProcessId}");
        }
    }

    private async Task StagePackageAsync(string stageDirectory, CancellationToken cancellationToken)
    {
        TryDeleteDirectory(stageDirectory);
        Directory.CreateDirectory(stageDirectory);

        if (Directory.Exists(arguments.PackagePath))
        {
            await CopyDirectoryAsync(arguments.PackagePath, stageDirectory, cancellationToken);
            Log("package directory copied to stage");
            return;
        }

        if (!File.Exists(arguments.PackagePath))
        {
            throw new FileNotFoundException("The downloaded update package was not found.", arguments.PackagePath);
        }

        var tempZipPath = Path.Combine(Path.GetTempPath(), $"totp-update-package-{Guid.NewGuid():N}.zip");
        try
        {
            File.Copy(arguments.PackagePath, tempZipPath, overwrite: true);
            ZipFile.ExtractToDirectory(tempZipPath, stageDirectory, overwriteFiles: true);
            Log("package archive extracted to stage");
        }
        finally
        {
            TryDeleteFile(tempZipPath);
        }
    }

    private static List<FileInfo> EnumerateStageFiles(string stageDirectory)
    {
        return Directory.EnumerateFiles(stageDirectory, "*", SearchOption.AllDirectories)
            .Select(path => new FileInfo(path))
            .ToList();
    }

    private async Task CopyFilesAsync(
        IReadOnlyList<FileInfo> files,
        string stageDirectory,
        string targetDirectory,
        long totalBytes,
        IProgress<InstallerProgressState> progress,
        CancellationToken cancellationToken)
    {
        long copiedBytes = 0;
        var copiedFiles = 0;

        foreach (var sourceFile in files)
        {
            cancellationToken.ThrowIfCancellationRequested();

            var relativePath = Path.GetRelativePath(stageDirectory, sourceFile.FullName);
            var destinationPath = Path.Combine(targetDirectory, relativePath);
            var destinationDirectory = Path.GetDirectoryName(destinationPath);
            if (!string.IsNullOrWhiteSpace(destinationDirectory))
            {
                Directory.CreateDirectory(destinationDirectory);
            }

            copiedFiles++;
            progress.Report(CreateState(
                "Installing update",
                "Installing files...",
                $"{copiedFiles}/{files.Count}: {relativePath}",
                $"{copiedFiles}/{files.Count} file(s)",
                false,
                totalBytes == 0 ? 0 : (int)Math.Clamp((copiedBytes * 100L) / totalBytes, 0, 100),
                false));

            copiedBytes += await CopyFileWithRetriesAsync(sourceFile.FullName, destinationPath, copiedBytes, totalBytes, progress, cancellationToken);
        }

        progress.Report(CreateState("Installing update", "Installing files...", "Finalizing copied files", "100% complete", false, 100, false));
        Log($"files copied: {files.Count}");
    }

    private async Task<long> CopyFileWithRetriesAsync(
        string sourcePath,
        string destinationPath,
        long copiedBytesBeforeFile,
        long totalBytes,
        IProgress<InstallerProgressState> progress,
        CancellationToken cancellationToken)
    {
        const int maxAttempts = 10;
        for (var attempt = 1; attempt <= maxAttempts; attempt++)
        {
            try
            {
                return await CopyFileAsync(sourcePath, destinationPath, copiedBytesBeforeFile, totalBytes, progress, cancellationToken);
            }
            catch when (attempt < maxAttempts)
            {
                await Task.Delay(500, cancellationToken);
            }
        }

        throw new IOException($"Failed to replace '{destinationPath}' after {maxAttempts} attempts.");
    }

    private static async Task<long> CopyFileAsync(
        string sourcePath,
        string destinationPath,
        long copiedBytesBeforeFile,
        long totalBytes,
        IProgress<InstallerProgressState> progress,
        CancellationToken cancellationToken)
    {
        const int bufferSize = 1024 * 128;
        await using var source = new FileStream(sourcePath, FileMode.Open, FileAccess.Read, FileShare.Read, bufferSize, useAsync: true);
        await using var destination = new FileStream(destinationPath, FileMode.Create, FileAccess.Write, FileShare.None, bufferSize, useAsync: true);

        var buffer = new byte[bufferSize];
        int bytesRead;
        long fileBytesCopied = 0;

        while ((bytesRead = await source.ReadAsync(buffer.AsMemory(0, buffer.Length), cancellationToken)) > 0)
        {
            await destination.WriteAsync(buffer.AsMemory(0, bytesRead), cancellationToken);
            fileBytesCopied += bytesRead;

            if (totalBytes > 0)
            {
                var percentage = (int)Math.Clamp(((copiedBytesBeforeFile + fileBytesCopied) * 100L) / totalBytes, 0, 100);
                progress.Report(CreateState("Installing update", "Installing files...", $"{percentage}% complete", $"{percentage}% complete", false, percentage, false));
            }
        }

        return fileBytesCopied;
    }

    private static async Task CopyDirectoryAsync(string sourceDirectory, string destinationDirectory, CancellationToken cancellationToken)
    {
        foreach (var directory in Directory.EnumerateDirectories(sourceDirectory, "*", SearchOption.AllDirectories))
        {
            var relative = Path.GetRelativePath(sourceDirectory, directory);
            Directory.CreateDirectory(Path.Combine(destinationDirectory, relative));
        }

        foreach (var file in Directory.EnumerateFiles(sourceDirectory, "*", SearchOption.AllDirectories))
        {
            cancellationToken.ThrowIfCancellationRequested();

            var relative = Path.GetRelativePath(sourceDirectory, file);
            var destination = Path.Combine(destinationDirectory, relative);
            var parent = Path.GetDirectoryName(destination);
            if (!string.IsNullOrWhiteSpace(parent))
            {
                Directory.CreateDirectory(parent);
            }

            await using var source = File.OpenRead(file);
            await using var target = File.Create(destination);
            await source.CopyToAsync(target, cancellationToken);
        }
    }

    private void RelaunchApplication()
    {
        var targetExecutablePath = Path.Combine(arguments.TargetDirectory, Path.GetFileName(arguments.ExecutablePath));
        if (!File.Exists(targetExecutablePath))
        {
            throw new FileNotFoundException("The updated application executable was not found.", targetExecutablePath);
        }

        Process.Start(new ProcessStartInfo
        {
            FileName = targetExecutablePath,
            WorkingDirectory = arguments.TargetDirectory,
            UseShellExecute = true
        });
    }

    private void ScheduleSelfCleanup()
    {
        var runtimeDirectory = AppContext.BaseDirectory.TrimEnd(Path.DirectorySeparatorChar);
        if (!runtimeDirectory.Contains("totp-updater-runtime-", StringComparison.OrdinalIgnoreCase))
        {
            return;
        }

        try
        {
            var command = $"/c timeout /t 2 /nobreak > nul & rd /s /q \"{runtimeDirectory}\"";
            Process.Start(new ProcessStartInfo
            {
                FileName = "cmd.exe",
                Arguments = command,
                CreateNoWindow = true,
                UseShellExecute = false
            });
        }
        catch
        {
            // best-effort cleanup only
        }
    }

    private static void CleanupOldTemporaryFolders()
    {
        var tempPath = Path.GetTempPath();
        foreach (var directory in Directory.EnumerateDirectories(tempPath, "totp-updater-runtime-*", SearchOption.TopDirectoryOnly))
        {
            try
            {
                var info = new DirectoryInfo(directory);
                if (DateTime.UtcNow - info.CreationTimeUtc < TimeSpan.FromDays(1))
                {
                    continue;
                }

                TryDeleteDirectory(directory);
            }
            catch
            {
                // best-effort cleanup only
            }
        }
    }

    private void Log(string message)
    {
        var line = $"{DateTimeOffset.Now:yyyy-MM-dd HH:mm:ss} {message}{Environment.NewLine}";
        File.AppendAllText(arguments.LogPath, line, Encoding.UTF8);
    }

    private void SignalReady()
    {
        if (_readySignalWritten || string.IsNullOrWhiteSpace(arguments.ReadySignalPath))
        {
            return;
        }

        File.WriteAllText(arguments.ReadySignalPath, "ready", Encoding.UTF8);
        _readySignalWritten = true;
        Log($"ready signal written: {arguments.ReadySignalPath}");
    }

    private static void TryDeleteDirectory(string path)
    {
        if (Directory.Exists(path))
        {
            Directory.Delete(path, recursive: true);
        }
    }

    private static void TryDeleteFile(string path)
    {
        if (File.Exists(path))
        {
            File.Delete(path);
        }
    }
}
