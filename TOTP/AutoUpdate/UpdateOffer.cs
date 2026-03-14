using NetSparkleUpdater;
using System;
using System.Collections.Generic;
using System.IO;

namespace TOTP.AutoUpdate;

internal sealed class UpdateOffer
{
    private UpdateOffer(
        AppCastItem item,
        string title,
        string shortVersion,
        string packageSummary,
        string sourceHost,
        bool isRecommended)
    {
        Item = item;
        Title = title;
        ShortVersion = shortVersion;
        PackageSummary = packageSummary;
        SourceHost = sourceHost;
        IsRecommended = isRecommended;
    }

    public AppCastItem Item { get; }

    public string Title { get; }

    public string ShortVersion { get; }

    public string PackageSummary { get; }

    public string SourceHost { get; }

    public bool IsRecommended { get; }

    public static IReadOnlyList<UpdateOffer> CreateMany(IReadOnlyList<AppCastItem> items)
    {
        var offers = new List<UpdateOffer>(items.Count);
        for (var i = 0; i < items.Count; i++)
        {
            offers.Add(Create(items[i], isRecommended: i == 0));
        }

        return offers;
    }

    private static UpdateOffer Create(AppCastItem item, bool isRecommended)
    {
        var shortVersion = item.ShortVersion ?? item.Version?.ToString() ?? "unknown";
        var sourceUri = Uri.TryCreate(item.DownloadLink, UriKind.Absolute, out var uri) ? uri : null;
        var fileName = sourceUri == null ? string.Empty : Path.GetFileName(sourceUri.LocalPath);
        var title = !string.IsNullOrWhiteSpace(item.Title)
            ? item.Title
            : !string.IsNullOrWhiteSpace(fileName)
                ? fileName
                : "TOTP Manager package";

        var packageSummary = BuildPackageSummary(fileName, item.UpdateSize);
        var sourceHost = sourceUri?.Host ?? "unknown source";

        return new UpdateOffer(item, title, shortVersion, packageSummary, sourceHost, isRecommended);
    }

    private static string BuildPackageSummary(string? fileName, long updateSize)
    {
        var packageName = string.IsNullOrWhiteSpace(fileName) ? "Signed package" : fileName;
        if (updateSize <= 0)
        {
            return packageName;
        }

        return $"{packageName} ({FormatBytes(updateSize)})";
    }

    private static string FormatBytes(long bytes)
    {
        if (bytes <= 0)
        {
            return "0 B";
        }

        string[] sizes = ["B", "KB", "MB", "GB"];
        var value = (double)bytes;
        var order = 0;
        while (value >= 1024 && order < sizes.Length - 1)
        {
            order++;
            value /= 1024;
        }

        return $"{value:0.#} {sizes[order]}";
    }
}
