using Markdig.Syntax;

namespace Symptum.UI.Markdown;

public class MarkdownParsedEventArgs
{
    public MarkdownDocument? Document { get; set; }

    public MarkdownParsedEventArgs(MarkdownDocument? document)
    {
        Document = document;
    }
}
