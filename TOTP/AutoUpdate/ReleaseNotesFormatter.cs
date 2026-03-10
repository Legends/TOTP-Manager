using NetSparkleUpdater;
using System;
using System.Linq;
using System.Net;
using System.Text;
using System.Text.RegularExpressions;

namespace TOTP.AutoUpdate;

internal static class ReleaseNotesFormatter
{
    private static readonly Regex HtmlTagRegex = new("<[^>]+>", RegexOptions.Compiled);
    private static readonly Regex MarkdownHeaderRegex = new(@"^(#{1,6})\s+(.+)$", RegexOptions.Compiled);
    private static readonly Regex MarkdownBulletRegex = new(@"^\s*[-*]\s+(.+)$", RegexOptions.Compiled);
    private static readonly Regex MarkdownNumberRegex = new(@"^\s*\d+\.\s+(.+)$", RegexOptions.Compiled);
    private static readonly Regex MarkdownBoldRegex = new(@"\*\*(.+?)\*\*", RegexOptions.Compiled);
    private static readonly Regex MarkdownItalicRegex = new(@"\*(.+?)\*", RegexOptions.Compiled);
    private static readonly Regex MarkdownCodeRegex = new(@"`([^`]+)`", RegexOptions.Compiled);
    private static readonly Regex MarkdownLinkRegex = new(@"\[([^\]]+)\]\(([^)]+)\)", RegexOptions.Compiled);

    public static string ToHtmlDocument(AppCastItem item)
    {
        var title = WebUtility.HtmlEncode(item.Title ?? $"Version {item.ShortVersion ?? item.Version?.ToString() ?? "unknown"}");
        var body = RenderBody(item.Description);

        return $$"""
        <!DOCTYPE html>
        <html>
        <head>
          <meta http-equiv="X-UA-Compatible" content="IE=edge" />
          <meta charset="utf-8" />
          <style>
            body {
              margin: 0;
              padding: 0;
              background: #10213f;
              color: #f4f7ff;
              font-family: "Segoe UI", sans-serif;
              font-size: 13px;
              line-height: 1.55;
            }
            .notes {
              padding: 0 2px 4px 2px;
            }
            h1, h2, h3, h4, h5, h6 {
              margin: 0 0 10px 0;
              color: white;
            }
            p {
              margin: 0 0 10px 0;
            }
            ul, ol {
              margin: 0 0 12px 18px;
              padding: 0;
            }
            li {
              margin: 0 0 6px 0;
            }
            a {
              color: #9cc3ff;
            }
            code {
              padding: 1px 4px;
              border-radius: 4px;
              background: #162b4f;
              color: #d7e6ff;
            }
            blockquote {
              margin: 0 0 12px 0;
              padding: 8px 12px;
              border-left: 3px solid #7d7ff4;
              background: rgba(255,255,255,0.05);
              color: #cbdaf3;
            }
            .empty {
              color: #cbdaf3;
              font-style: italic;
            }
          </style>
        </head>
        <body>
          <div class="notes">
            <h3>{{title}}</h3>
            {{body}}
          </div>
        </body>
        </html>
        """;
    }

    public static string ToPlainText(string? content)
    {
        if (string.IsNullOrWhiteSpace(content))
        {
            return "No embedded release notes were provided in the appcast.";
        }

        var withoutTags = HtmlTagRegex.Replace(content, " ");
        return WebUtility.HtmlDecode(withoutTags).Trim();
    }

    private static string RenderBody(string? content)
    {
        if (string.IsNullOrWhiteSpace(content))
        {
            return "<p class=\"empty\">No embedded release notes were provided in the appcast.</p>";
        }

        if (LooksLikeHtml(content))
        {
            return content;
        }

        return RenderMarkdownLikeText(content);
    }

    private static bool LooksLikeHtml(string content)
    {
        return content.Contains("<p", StringComparison.OrdinalIgnoreCase)
            || content.Contains("<ul", StringComparison.OrdinalIgnoreCase)
            || content.Contains("<ol", StringComparison.OrdinalIgnoreCase)
            || content.Contains("<li", StringComparison.OrdinalIgnoreCase)
            || content.Contains("<br", StringComparison.OrdinalIgnoreCase)
            || content.Contains("<h1", StringComparison.OrdinalIgnoreCase)
            || content.Contains("<h2", StringComparison.OrdinalIgnoreCase)
            || content.Contains("<div", StringComparison.OrdinalIgnoreCase);
    }

    private static string RenderMarkdownLikeText(string content)
    {
        var lines = content.Replace("\r\n", "\n").Split('\n');
        var html = new StringBuilder();
        var paragraph = new StringBuilder();
        var inUnorderedList = false;
        var inOrderedList = false;

        void FlushParagraph()
        {
            if (paragraph.Length == 0)
            {
                return;
            }

            html.Append("<p>")
                .Append(FormatInline(paragraph.ToString().Trim()))
                .AppendLine("</p>");
            paragraph.Clear();
        }

        void CloseLists()
        {
            if (inUnorderedList)
            {
                html.AppendLine("</ul>");
                inUnorderedList = false;
            }

            if (inOrderedList)
            {
                html.AppendLine("</ol>");
                inOrderedList = false;
            }
        }

        foreach (var rawLine in lines)
        {
            var line = rawLine.TrimEnd();
            if (string.IsNullOrWhiteSpace(line))
            {
                FlushParagraph();
                CloseLists();
                continue;
            }

            var headerMatch = MarkdownHeaderRegex.Match(line);
            if (headerMatch.Success)
            {
                FlushParagraph();
                CloseLists();
                var level = Math.Min(headerMatch.Groups[1].Value.Length + 1, 6);
                html.Append('<').Append('h').Append(level).Append('>')
                    .Append(FormatInline(headerMatch.Groups[2].Value))
                    .Append("</h").Append(level).AppendLine(">");
                continue;
            }

            var bulletMatch = MarkdownBulletRegex.Match(line);
            if (bulletMatch.Success)
            {
                FlushParagraph();
                if (!inUnorderedList)
                {
                    CloseLists();
                    html.AppendLine("<ul>");
                    inUnorderedList = true;
                }

                html.Append("<li>")
                    .Append(FormatInline(bulletMatch.Groups[1].Value))
                    .AppendLine("</li>");
                continue;
            }

            var numberMatch = MarkdownNumberRegex.Match(line);
            if (numberMatch.Success)
            {
                FlushParagraph();
                if (!inOrderedList)
                {
                    CloseLists();
                    html.AppendLine("<ol>");
                    inOrderedList = true;
                }

                html.Append("<li>")
                    .Append(FormatInline(numberMatch.Groups[1].Value))
                    .AppendLine("</li>");
                continue;
            }

            if (line.StartsWith(">", StringComparison.Ordinal))
            {
                FlushParagraph();
                CloseLists();
                html.Append("<blockquote>")
                    .Append(FormatInline(line.TrimStart('>', ' ')))
                    .AppendLine("</blockquote>");
                continue;
            }

            if (paragraph.Length > 0)
            {
                paragraph.Append(' ');
            }

            paragraph.Append(line.Trim());
        }

        FlushParagraph();
        CloseLists();

        return html.Length == 0
            ? "<p class=\"empty\">No embedded release notes were provided in the appcast.</p>"
            : html.ToString();
    }

    private static string FormatInline(string value)
    {
        var encoded = WebUtility.HtmlEncode(value);
        encoded = MarkdownLinkRegex.Replace(encoded, m => $"<a href=\"{WebUtility.HtmlEncode(m.Groups[2].Value)}\">{m.Groups[1].Value}</a>");
        encoded = MarkdownBoldRegex.Replace(encoded, "<strong>$1</strong>");
        encoded = MarkdownItalicRegex.Replace(encoded, "<em>$1</em>");
        encoded = MarkdownCodeRegex.Replace(encoded, "<code>$1</code>");
        return encoded.Replace("\n", "<br />");
    }
}
