using Markdig.Helpers;
using Markdig.Parsers;
using Markdig.Syntax;

namespace Symptum.Markdown.Reference;

public class ReferenceInlineParser : InlineParser
{
    public ReferenceInlineParser()
    {
        OpeningCharacters = ['@'];
    }

    public override bool Match(InlineProcessor processor, ref StringSlice slice)
    {
        var match = slice.CurrentChar;
        var pc = slice.PeekCharExtra(-1);
        if (pc == match || pc == '\\')
        {
            return false;
        }

        var startPosition = slice.Start;

        var c = slice.NextChar();

        pc.CheckUnicodeCategory(out bool openPrevIsWhiteSpace, out bool openPrevIsPunctuation);
        c.CheckUnicodeCategory(out bool openNextIsWhiteSpace, out bool openNextIsPunctuation);

        if ((!openPrevIsWhiteSpace && !openPrevIsPunctuation) || openNextIsWhiteSpace || openNextIsPunctuation)
        {
            return false;
        }

        var start = slice.Start;

        while (!c.IsSpaceOrTab() && !c.IsNewLineOrLineFeed() && !c.IsZero())
        {
            pc = c;
            c = slice.NextChar();
        }

        pc.CheckUnicodeCategory(out bool closePrevIsWhiteSpace, out _);
        c.CheckUnicodeCategory(out bool closeNextIsWhiteSpace, out bool closeNextIsPunctuation);

        if ((!closeNextIsPunctuation && !closeNextIsWhiteSpace) || (openNextIsWhiteSpace != closePrevIsWhiteSpace))
        {
            return false;
        }

        int end = slice.Start - 1;
        var inline = new ReferenceInline()
        {
            Span = new SourceSpan(processor.GetSourcePosition(startPosition, out int line, out int column), processor.GetSourcePosition(slice.Start - 1)),
            Line = line,
            Column = column,
            Delimiter = match,
            DelimiterCount = 1,
            Content = slice
        };
        inline.Content.Start = start;
        inline.Content.End = end;

        processor.Inline = inline;

        return true;
    }
}
