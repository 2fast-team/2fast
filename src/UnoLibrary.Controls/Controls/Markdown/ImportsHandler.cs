using Markdig;
using Markdig.Syntax;
//using Symptum.Core.Management.Resources;
using Symptum.Markdown.Embedding;
using Symptum.UI.Markdown.Renderers;
using Symptum.UI.Markdown.TextElements;

namespace Symptum.UI.Markdown;

public class ImportsHandler
{
    private Dictionary<string, ImportBlockElement> importBlocks = [];

    public void RegisterForImport(string importId, ImportBlockElement importBlockElement)
    {
        importBlocks.TryAdd(importId, importBlockElement);
    }

    internal void ResolveImports(IEnumerable<ExportBlock>? availableExports, WinUIRenderer renderer, MarkdownPipeline pipeline)
    {
        foreach (var kvp in importBlocks)
        {
            string id = kvp.Key;
            ExportBlock? match = null;
            if (id.StartsWith(nameof(Symptum)))
            {
                var ids = id.Split('?');
                string resId = ids[0];
                string impId = ids[1];
                //if (ResourceManager.TryGetResourceFromId(resId, out IResource? resource)
                //    && resource is MarkdownFileResource markdownFileResource
                //    && !string.IsNullOrEmpty(markdownFileResource.Markdown))
                //{
                //    MarkdownDocument doc = Markdig.Markdown.Parse(markdownFileResource.Markdown, pipeline);
                //    match = doc.Descendants<ExportBlock>().FirstOrDefault(e => impId.Equals(e.Id.ToString(), StringComparison.InvariantCulture));
                //}
            }
            else match = availableExports?.FirstOrDefault(e => kvp.Key.Equals(e.Id.ToString(), StringComparison.InvariantCulture));

            if (match != null)
            {
                var importBlock = kvp.Value;
                renderer.Push(importBlock);
                renderer.WriteChildren(match);
                renderer.Pop(false);
            }
        }
        importBlocks.Clear();
    }
}
