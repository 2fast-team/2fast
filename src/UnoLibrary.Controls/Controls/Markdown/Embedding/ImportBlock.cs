using Markdig.Helpers;
using Markdig.Parsers;
using Markdig.Syntax;

namespace Symptum.Markdown.Embedding;

public class ImportBlock : ContainerBlock
{
    public ImportBlock(BlockParser? parser) : base(parser) { }

    public StringSlice Id { get; set; }
}
