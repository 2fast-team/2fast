using Markdig.Helpers;
using Markdig.Parsers;
using Markdig.Syntax;

namespace Symptum.Markdown.Embedding;

public class ExportBlock : ContainerBlock
{
    public ExportBlock(BlockParser? parser) : base(parser) { }

    public StringSlice Id { get; set; }
}
