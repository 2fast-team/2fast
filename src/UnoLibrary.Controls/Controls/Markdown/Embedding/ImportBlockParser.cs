using Markdig.Helpers;
using Markdig.Parsers;
using Markdig.Syntax;

namespace Symptum.Markdown.Embedding;

public class ImportBlockParser : BlockParser
{
    // => { Id }
    public ImportBlockParser()
    {
        OpeningCharacters = ['='];
    }

    public override BlockState TryOpen(BlockProcessor processor)
    {
        if (processor.IsCodeIndent)
            return BlockState.None;

        var line = processor.Line;
        var column = processor.Column;
        var c = line.NextChar();
        if (c == '>')
            line.SkipChar();
        else return BlockState.None;

        int startPosition = processor.Start;
        var export = new ImportBlock(this)
        {
            Span = new SourceSpan(startPosition, line.End),
            Line = processor.LineIndex,
            Column = column
        };

        line.TrimStart();
        if (!line.IsEmpty)
        {
            export.Id = new StringSlice(line.Text, line.Start, line.End);
        }
        processor.NewBlocks.Push(export);

        return BlockState.BreakDiscard;
    }
}
