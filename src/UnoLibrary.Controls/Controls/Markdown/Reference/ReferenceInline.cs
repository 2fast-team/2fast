using Markdig.Helpers;
using Markdig.Syntax.Inlines;

namespace Symptum.Markdown.Reference;

public class ReferenceInline : Inline
{
    public char Delimiter { get; set; }

    public int DelimiterCount { get; set; }

    public StringSlice Content;
}
