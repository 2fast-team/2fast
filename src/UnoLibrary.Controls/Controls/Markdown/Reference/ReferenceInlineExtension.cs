using Markdig;
using Markdig.Renderers;

namespace Symptum.Markdown.Reference;

public class ReferenceInlineExtension : IMarkdownExtension
{
    public void Setup(MarkdownPipelineBuilder pipeline)
    {
        if (!pipeline.InlineParsers.Contains<ReferenceInlineParser>())
        {
            pipeline.InlineParsers.Insert(0, new ReferenceInlineParser());
        }
    }

    public void Setup(MarkdownPipeline pipeline, IMarkdownRenderer renderer)
    {
    }
}
