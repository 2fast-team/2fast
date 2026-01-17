using Markdig.Renderers;
using Markdig;

namespace Symptum.Markdown.Embedding;

public class ImportBlockExtension : IMarkdownExtension
{
    public void Setup(MarkdownPipelineBuilder pipeline)
    {
        if (!pipeline.BlockParsers.Contains<ImportBlockParser>())
        {
            pipeline.BlockParsers.Insert(0, new ImportBlockParser());
        }
    }

    public void Setup(MarkdownPipeline pipeline, IMarkdownRenderer renderer)
    {
    }
}
