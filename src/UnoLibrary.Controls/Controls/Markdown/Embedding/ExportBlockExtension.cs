using Markdig;
using Markdig.Renderers;

namespace Symptum.Markdown.Embedding;

public class ExportBlockExtension : IMarkdownExtension
{
    public void Setup(MarkdownPipelineBuilder pipeline)
    {
        if (!pipeline.BlockParsers.Contains<ExportBlockParser>())
        {
            pipeline.BlockParsers.Insert(0, new ExportBlockParser());
        }
    }

    public void Setup(MarkdownPipeline pipeline, IMarkdownRenderer renderer)
    {
    }
}
