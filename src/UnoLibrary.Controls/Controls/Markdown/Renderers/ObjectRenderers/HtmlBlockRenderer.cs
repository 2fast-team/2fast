using System.Text;
using System.Text.RegularExpressions;
using HtmlAgilityPack;
using Markdig.Syntax;

namespace Symptum.UI.Markdown.Renderers.ObjectRenderers;

internal partial class HtmlBlockRenderer : WinUIObjectRenderer<HtmlBlock>
{
    protected override void Write(WinUIRenderer renderer, HtmlBlock obj)
    {
        ArgumentNullException.ThrowIfNull(renderer);
        ArgumentNullException.ThrowIfNull(obj);

        var stringBuilder = new StringBuilder();
        foreach (var line in obj.Lines.Lines)
        {
            var lineText = line.Slice.ToString().Trim();
            if (string.IsNullOrWhiteSpace(lineText))
            {
                continue;
            }
            stringBuilder.AppendLine(lineText);
        }

        var html = WhiteSpaceRegex().Replace(stringBuilder.ToString(), "");
        html = HtmlWhiteSpaceRegex().Replace(html, " ");
        var doc = new HtmlDocument();
        doc.LoadHtml(html);
        HtmlWriter.WriteHtml(renderer, doc.DocumentNode.ChildNodes);
    }

    [GeneratedRegex(@"\t|\n|\r", RegexOptions.Compiled)]
    private static partial Regex WhiteSpaceRegex();

    [GeneratedRegex(@"&nbsp;", RegexOptions.Compiled)]
    private static partial Regex HtmlWhiteSpaceRegex();
}
