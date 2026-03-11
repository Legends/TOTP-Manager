using System.Diagnostics;
using System.IO;
using System.IO.Compression;
using System.Text;
using System.Windows;

namespace TOTP.Updater;

public partial class UpdateInstallerForm : Window
{
    private readonly UpdateInstallArguments _arguments;
    private bool _readySignalWritten;

    public UpdateInstallerForm(string[] args)
    {
        _arguments = UpdateInstallArguments.Parse(args);
        InitializeComponent();
        Loaded += OnLoaded;
    }

    private async void OnLoaded(object sender, RoutedEventArgs e)
    {
        Loaded -= OnLoaded;
        await RunInstallAsync();
    }

    private async Task RunInstallAsync()
    {
        try
        {
            CleanupOldTemporaryFolders();
            Directory.CreateDirectory(Path.GetDirectoryName(_arguments.LogPath)!);
            Log("updater started");
            SignalReady();

            UpdateMarquee("Closing TOTP Manager...", _arguments.PackagePath);
            await CloseParentApplicationAsync(_arguments.ParentProcessId);
            UpdateMarquee("Waiting for the app to close...", _arguments.PackagePath);
            await WaitForParentProcessExitAsync(_arguments.ParentProcessId);

            UpdateMarquee("Staging update package...", _arguments.PackagePath);
            var stageDirectory = Path.Combine(Path.GetTempPath(), $"totp-update-stage-{Guid.NewGuid():N}");
            try
            {
                await StagePackageAsync(stageDirectory);
                var files = EnumerateStageFiles(stageDirectory);
                var totalBytes = files.Sum(static file => file.Length);

                UpdateProgress(0, "Installing files...", $"{files.Count} file(s) queued", $"{files.Count} item(s) queued");
                await CopyFilesAsync(files, stageDirectory, _arguments.TargetDirectory, totalBytes);

                UpdateProgress(
                    100,
                    "Relaunching app...",
                    Path.Combine(_arguments.TargetDirectory, Path.GetFileName(_arguments.ExecutablePath)),
                    "100% complete");
                RelaunchApplication();
                Log("application relaunched");
                ScheduleSelfCleanup();
                Close();
            }
            finally
            {
                TryDeleteDirectory(stageDirectory);
            }
        }
        catch (Exception ex)
        {
            Log($"updater failed: {ex}");
            TitleTextBlock.Text = "Update failed";
            StatusTextBlock.Text = "The update could not be installed.";
            DetailTextBlock.Text = ex.Message;
            ProgressTextBlock.Text = string.Empty;
            InstallProgressBar.IsIndeterminate = false;
            InstallProgressBar.Value = 0;
            CloseButton.IsEnabled = true;
        }
    }

    private async Task WaitForParentProcessExitAsync(int parentProcessId)
    {
        try
        {
            using var process = Process.GetProcessById(parentProcessId);
            await process.WaitForExitAsync();
            Log($"parent process exited: {parentProcessId}");
        }
        catch (ArgumentException)
        {
            Log($"parent process already exited: {parentProcessId}");
        }
    }

    private async Task CloseParentApplicationAsync(int parentProcessId)
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
                await Task.Delay(100);
            }
        }
        catch (ArgumentException)
        {
            Log($"parent process already exited before close request: {parentProcessId}");
        }
    }

    private async Task StagePackageAsync(string stageDirectory)
    {
        TryDeleteDirectory(stageDirectory);
        Directory.CreateDirectory(stageDirectory);

        if (Directory.Exists(_arguments.PackagePath))
        {
            await CopyDirectoryAsync(_arguments.PackagePath, stageDirectory);
            Log("package directory copied to stage");
            return;
        }

        if (!File.Exists(_arguments.PackagePath))
        {
            throw new FileNotFoundException("The downloaded update package was not found.", _arguments.PackagePath);
        }

        var tempZipPath = Path.Combine(Path.GetTempPath(), $"totp-update-package-{Guid.NewGuid():N}.zip");
        try
        {
            File.Copy(_arguments.PackagePath, tempZipPath, overwrite: true);
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

    private async Task CopyFilesAsync(IReadOnlyList<FileInfo> files, string stageDirectory, string targetDirectory, long totalBytes)
    {
        long copiedBytes = 0;
        var copiedFiles = 0;

        foreach (var sourceFile in files)
        {
            var relativePath = Path.GetRelativePath(stageDirectory, sourceFile.FullName);
            var destinationPath = Path.Combine(targetDirectory, relativePath);
            var destinationDirectory = Path.GetDirectoryName(destinationPath);
            if (!string.IsNullOrWhiteSpace(destinationDirectory))
            {
                Directory.CreateDirectory(destinationDirectory);
            }

            copiedFiles++;
            UpdateProgress(
                totalBytes == 0 ? 0 : (int)Math.Clamp((copiedBytes * 100L) / totalBytes, 0, 100),
                "Installing files...",
                $"{copiedFiles}/{files.Count}: {relativePath}",
                $"{copiedFiles}/{files.Count} file(s)");

            copiedBytes += await CopyFileWithRetriesAsync(sourceFile.FullName, destinationPath, copiedBytes, totalBytes);
        }

        UpdateProgress(100, "Installing files...", "Finalizing copied files", "100% complete");
        Log($"files copied: {files.Count}");
    }

    private async Task<long> CopyFileWithRetriesAsync(string sourcePath, string destinationPath, long copiedBytesBeforeFile, long totalBytes)
    {
        const int maxAttempts = 10;
        for (var attempt = 1; attempt <= maxAttempts; attempt++)
        {
            try
            {
                return await CopyFileAsync(sourcePath, destinationPath, copiedBytesBeforeFile, totalBytes);
            }
            catch when (attempt < maxAttempts)
            {
                await Task.Delay(500);
            }
        }

        throw new IOException($"Failed to replace '{destinationPath}' after {maxAttempts} attempts.");
    }

    private async Task<long> CopyFileAsync(string sourcePath, string destinationPath, long copiedBytesBeforeFile, long totalBytes)
    {
        const int bufferSize = 1024 * 128;
        await using var source = new FileStream(sourcePath, FileMode.Open, FileAccess.Read, FileShare.Read, bufferSize, useAsync: true);
        await using var destination = new FileStream(destinationPath, FileMode.Create, FileAccess.Write, FileShare.None, bufferSize, useAsync: true);

        var buffer = new byte[bufferSize];
        int bytesRead;
        long fileBytesCopied = 0;

        while ((bytesRead = await source.ReadAsync(buffer.AsMemory(0, buffer.Length))) > 0)
        {
            await destination.WriteAsync(buffer.AsMemory(0, bytesRead));
            fileBytesCopied += bytesRead;

            if (totalBytes > 0)
            {
                var percentage = (int)Math.Clamp(((copiedBytesBeforeFile + fileBytesCopied) * 100L) / totalBytes, 0, 100);
                UpdateProgress(percentage, "Installing files...", $"{percentage}% complete", $"{percentage}% complete");
            }
        }

        return fileBytesCopied;
    }

    private async Task CopyDirectoryAsync(string sourceDirectory, string destinationDirectory)
    {
        foreach (var directory in Directory.EnumerateDirectories(sourceDirectory, "*", SearchOption.AllDirectories))
        {
            var relative = Path.GetRelativePath(sourceDirectory, directory);
            Directory.CreateDirectory(Path.Combine(destinationDirectory, relative));
        }

        foreach (var file in Directory.EnumerateFiles(sourceDirectory, "*", SearchOption.AllDirectories))
        {
            var relative = Path.GetRelativePath(sourceDirectory, file);
            var destination = Path.Combine(destinationDirectory, relative);
            var parent = Path.GetDirectoryName(destination);
            if (!string.IsNullOrWhiteSpace(parent))
            {
                Directory.CreateDirectory(parent);
            }

            await using var source = File.OpenRead(file);
            await using var target = File.Create(destination);
            await source.CopyToAsync(target);
        }
    }

    private void RelaunchApplication()
    {
        var targetExecutablePath = Path.Combine(_arguments.TargetDirectory, Path.GetFileName(_arguments.ExecutablePath));
        if (!File.Exists(targetExecutablePath))
        {
            throw new FileNotFoundException("The updated application executable was not found.", targetExecutablePath);
        }

        var startInfo = new ProcessStartInfo
        {
            FileName = targetExecutablePath,
            WorkingDirectory = _arguments.TargetDirectory,
            UseShellExecute = true
        };

        Process.Start(startInfo);
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
        }
    }

    private void CleanupOldTemporaryFolders()
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
            }
        }
    }

    private void UpdateMarquee(string status, string detail)
    {
        Dispatcher.Invoke(() =>
        {
            InstallProgressBar.IsIndeterminate = true;
            InstallProgressBar.Value = 0;
            StatusTextBlock.Text = status;
            DetailTextBlock.Text = detail;
            ProgressTextBlock.Text = string.Empty;
        });
    }

    private void UpdateProgress(int percentage, string status, string detail, string progressText)
    {
        Dispatcher.Invoke(() =>
        {
            InstallProgressBar.IsIndeterminate = false;
            InstallProgressBar.Value = Math.Clamp(percentage, 0, 100);
            StatusTextBlock.Text = status;
            DetailTextBlock.Text = detail;
            ProgressTextBlock.Text = progressText;
        });
    }

    private void Log(string message)
    {
        var line = $"{DateTimeOffset.Now:yyyy-MM-dd HH:mm:ss} {message}{Environment.NewLine}";
        File.AppendAllText(_arguments.LogPath, line, Encoding.UTF8);
    }

    private void SignalReady()
    {
        if (_readySignalWritten || string.IsNullOrWhiteSpace(_arguments.ReadySignalPath))
        {
            return;
        }

        File.WriteAllText(_arguments.ReadySignalPath, "ready", Encoding.UTF8);
        _readySignalWritten = true;
        Log($"ready signal written: {_arguments.ReadySignalPath}");
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

    private void CloseButton_OnClick(object sender, RoutedEventArgs e)
    {
        Close();
    }
}

internal sealed class UpdateInstallArguments
{
    public required string PackagePath { get; init; }
    public required string TargetDirectory { get; init; }
    public required string ExecutablePath { get; init; }
    public required int ParentProcessId { get; init; }
    public required string LogPath { get; init; }
    public string? ReadySignalPath { get; init; }

    public static UpdateInstallArguments Parse(string[] args)
    {
        var values = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
        for (var index = 0; index < args.Length - 1; index += 2)
        {
            values[args[index]] = args[index + 1];
        }

        return new UpdateInstallArguments
        {
            PackagePath = GetRequired(values, "--packagePath"),
            TargetDirectory = GetRequired(values, "--targetDir"),
            ExecutablePath = GetRequired(values, "--exePath"),
            ParentProcessId = int.Parse(GetRequired(values, "--parentPid")),
            LogPath = values.TryGetValue("--logPath", out var logPath)
                ? logPath
                : Path.Combine(Path.GetTempPath(), "totp-updater.log"),
            ReadySignalPath = values.TryGetValue("--readySignalPath", out var readySignalPath)
                ? readySignalPath
                : null
        };
    }

    private static string GetRequired(IReadOnlyDictionary<string, string> values, string key)
    {
        if (values.TryGetValue(key, out var value) && !string.IsNullOrWhiteSpace(value))
        {
            return value;
        }

        throw new ArgumentException($"Missing required argument '{key}'.");
    }
}
