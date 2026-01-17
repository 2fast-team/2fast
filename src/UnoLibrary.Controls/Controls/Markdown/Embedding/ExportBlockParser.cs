using Markdig.Helpers;
using Markdig.Parsers;
using Markdig.Syntax;

namespace Symptum.Markdown.Embedding;

public class ExportBlockParser : BlockParser
{
    // <= { Id }
    // { Content }
    // <=
    public ExportBlockParser()
    {
        OpeningCharacters = ['<'];
    }

    public override BlockState TryOpen(BlockProcessor processor)
    {
        if (processor.IsCodeIndent)
            return BlockState.None;

        var line = processor.Line;
        var column = processor.Column;
        var c = line.NextChar();
        if (c == '=')
            line.SkipChar();
        else return BlockState.None;

        int startPosition = processor.Start;
        var export = new ExportBlock(this)
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

        return BlockState.ContinueDiscard;
    }

    public override BlockState TryContinue(BlockProcessor processor, Block block)
    {
        var line = processor.Line;
        char openingChar = line.CurrentChar;
        var column = processor.Column;
        var sourcePosition = line.Start;

        var export = (ExportBlock)block;

        if (!processor.IsCodeIndent && processor.NextChar() == '=')
        {
            export.UpdateSpanEnd(line.End);
            return BlockState.BreakDiscard;
        }

        processor.GoToColumn(processor.ColumnBeforeIndent);
        export.UpdateSpanEnd(line.End);

        return BlockState.Continue;
    }
}
