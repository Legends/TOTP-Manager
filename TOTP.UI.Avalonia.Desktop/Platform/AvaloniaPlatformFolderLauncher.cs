using System.Diagnostics;
using FluentResults;
using TOTP.Core.Services.Interfaces;

namespace TOTP.Avalonia.Desktop.Platform;

public sealed class AvaloniaPlatformFolderLauncher : IPlatformFolderLauncher
{
    public Task<Result> OpenFolderAsync(
        string path,
        CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();
        if (string.IsNullOrWhiteSpace(path) || !Directory.Exists(path))
            return Task.FromResult(Result.Fail("The requested folder is unavailable."));

        try
        {
            using var process = Process.Start(new ProcessStartInfo
            {
                FileName = path,
                UseShellExecute = true
            });
            return Task.FromResult(process is null
                ? Result.Fail("The platform shell did not open the folder.")
                : Result.Ok());
        }
        catch (Exception)
        {
            return Task.FromResult(Result.Fail("The platform shell could not open the folder."));
        }
    }
}
