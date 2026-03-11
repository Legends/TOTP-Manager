using System.Diagnostics;
using System.IO.Compression;
using System.Text;
using System.Windows.Forms;

namespace TOTP.Updater;

internal static class Program
{
    [STAThread]
    private static void Main(string[] args)
    {
        ApplicationConfiguration.Initialize();
        using var form = new UpdateInstallerForm(args);
        Application.Run(form);
    }
}

internal sealed class UpdateInstallerForm : Form
{
    private readonly Label _titleLabel;
    private readonly Label _statusLabel;
    private readonly Label _detailLabel;
    private readonly ProgressBar _progressBar;
    private readonly Button _closeButton;
    private readonly UpdateInstallArguments _arguments;

    public UpdateInstallerForm(string[] args)
    {
        _arguments = UpdateInstallArguments.Parse(args);

        Text = "TOTP Manager Updater";
        StartPosition = FormStartPosition.CenterScreen;
        FormBorderStyle = FormBorderStyle.FixedDialog;
        MaximizeBox = false;
        MinimizeBox = false;
        ClientSize = new Size(520, 220);

        _titleLabel = new Label
        {
            Left = 22,
            Top = 20,
            Width = 470,
            Font = new Font("Segoe UI Semibold", 14f, FontStyle.Bold),
            Text = "Installing update"
        };

        _statusLabel = new Label
        {
            Left = 22,
            Top = 64,
            Width = 470,
            Height = 26,
            Font = new Font("Segoe UI", 10.5f, FontStyle.Regular),
            Text = "Preparing updater..."
        };

        _detailLabel = new Label
        {
            Left = 22,
            Top = 94,
            Width = 470,
            Height = 44,
            Font = new Font("Segoe UI", 9f, FontStyle.Regular),
            Text = string.Empty
        };

        _progressBar = new ProgressBar
        {
            Left = 22,
            Top = 148,
            Width = 470,
            Height = 22,
            Style = ProgressBarStyle.Marquee,
            MarqueeAnimationSpeed = 25
        };

        _closeButton = new Button
        {
            Left = 392,
            Top = 182,
            Width = 100,
            Height = 28,
            Text = "Close",
            Enabled = false
        };
        _closeButton.Click += (_, _) => Close();

        Controls.AddRange([_titleLabel, _statusLabel, _detailLabel, _progressBar, _closeButton]);
        Shown += async (_, _) => await RunInstallAsync();
    }

    private async Task RunInstallAsync()
    {
        try
        {
            CleanupOldTemporaryFolders();
            Directory.CreateDirectory(Path.GetDirectoryName(_arguments.LogPath)!);
            Log("updater started");

            UpdateMarquee("Waiting for the app to close...", _arguments.PackagePath);
            await WaitForParentProcessExitAsync(_arguments.ParentProcessId);

            UpdateMarquee("Staging update package...", _arguments.PackagePath);
            var stageDirectory = Path.Combine(Path.GetTempPath(), $"totp-update-stage-{Guid.NewGuid():N}");
            try
            {
                await StagePackageAsync(stageDirectory);
                var files = EnumerateStageFiles(stageDirectory);
                var totalBytes = files.Sum(static file => file.Length);

                UpdateProgress(0, "Installing files...", $"{files.Count} file(s) queued");
                await CopyFilesAsync(files, stageDirectory, _arguments.TargetDirectory, totalBytes);

                UpdateProgress(100, "Relaunching app...", Path.Combine(_arguments.TargetDirectory, Path.GetFileName(_arguments.ExecutablePath)));
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
            _titleLabel.Text = "Update failed";
            _statusLabel.Text = "The update could not be installed.";
            _detailLabel.Text = ex.Message;
            _progressBar.Style = ProgressBarStyle.Blocks;
            _progressBar.Value = 0;
            _closeButton.Enabled = true;
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
                $"{copiedFiles}/{files.Count}: {relativePath}");

            copiedBytes += await CopyFileWithRetriesAsync(sourceFile.FullName, destinationPath, copiedBytes, totalBytes);
        }

        UpdateProgress(100, "Installing files...", "Finalizing copied files");
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
                UpdateProgress(percentage, "Installing files...", $"{percentage}% complete");
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
            // best-effort cleanup only
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
                // best-effort cleanup only
            }
        }
    }

    private void UpdateMarquee(string status, string detail)
    {
        if (InvokeRequired)
        {
            Invoke(() => UpdateMarquee(status, detail));
            return;
        }

        _progressBar.Style = ProgressBarStyle.Marquee;
        _progressBar.MarqueeAnimationSpeed = 25;
        _statusLabel.Text = status;
        _detailLabel.Text = detail;
    }

    private void UpdateProgress(int percentage, string status, string detail)
    {
        if (InvokeRequired)
        {
            Invoke(() => UpdateProgress(percentage, status, detail));
            return;
        }

        _progressBar.Style = ProgressBarStyle.Blocks;
        _progressBar.MarqueeAnimationSpeed = 0;
        _progressBar.Value = Math.Clamp(percentage, 0, 100);
        _statusLabel.Text = status;
        _detailLabel.Text = detail;
    }

    private void Log(string message)
    {
        var line = $"{DateTimeOffset.Now:yyyy-MM-dd HH:mm:ss} {message}{Environment.NewLine}";
        File.AppendAllText(_arguments.LogPath, line, Encoding.UTF8);
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

internal sealed class UpdateInstallArguments
{
    public required string PackagePath { get; init; }
    public required string TargetDirectory { get; init; }
    public required string ExecutablePath { get; init; }
    public required int ParentProcessId { get; init; }
    public required string LogPath { get; init; }

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
                : Path.Combine(Path.GetTempPath(), "totp-updater.log")
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
