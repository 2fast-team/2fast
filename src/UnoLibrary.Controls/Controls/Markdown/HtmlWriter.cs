using HtmlAgilityPack;
using Symptum.UI.Markdown.Renderers;
using Symptum.UI.Markdown.TextElements;
using Symptum.UI.Markdown.TextElements.Html;

namespace Symptum.UI.Markdown;

internal class HtmlWriter
{
    public static void WriteHtml(WinUIRenderer renderer, HtmlNodeCollection nodes)
    {
        if (nodes == null || nodes.Count == 0) return;
        foreach (var node in nodes)
        {
            HtmlElementType elementType = node.Name.TagToType();
            if (node.NodeType == HtmlNodeType.Text)
            {
                renderer.WriteText(node.InnerText);
            }
            else if (node.NodeType == HtmlNodeType.Element && elementType == HtmlElementType.Inline)
            {
                // detect br here
                var inlineTagName = node.Name.ToLower();
                if (inlineTagName == "br")
                {
                    renderer.WriteInline(new LineBreakElement());
                }
                else if (inlineTagName == "a")
                {
                    IAddChild hyperLink;
                    if (node.ChildNodes.Any(n => n.Name != "#text"))
                    {
                        hyperLink = new HyperlinkButtonElement(node, renderer.Configuration.BaseUrl, renderer.Configuration, renderer.LinkHandler);
                    }
                    else
                    {
                        hyperLink = new HyperlinkElement(node, renderer.Configuration.BaseUrl, renderer.LinkHandler);
                    }
                    renderer.Push(hyperLink);
                    WriteHtml(renderer, node.ChildNodes);
                    renderer.Pop();
                }
                else if (inlineTagName == "img")
                {
                    var image = new ImageElement(node, renderer.Configuration);
                    renderer.WriteInline(image);
                }
                else
                {
                    var inline = new HtmlInlineElement(node);
                    renderer.Push(inline);
                    WriteHtml(renderer, node.ChildNodes);
                    renderer.Pop();
                }
            }
            else if (node.NodeType == HtmlNodeType.Element && elementType == HtmlElementType.Block)
            {
                IAddChild block;
                var tag = node.Name.ToLower();
                if (tag == "details")
                {
                    block = new HtmlDetailsElement(node, renderer.Configuration);
                    if (node.ChildNodes.FirstOrDefault(x => x.Name == "summary" || x.Name == "header") is HtmlNode child)
                            node.ChildNodes.Remove(child);
                    renderer.Push(block);
                    WriteHtml(renderer, node.ChildNodes);
                }
                else if (tag.IsHeading())
                {
                    var heading = new HeadingElement(node, renderer.Configuration, renderer.DocumentOutline);
                    renderer.Push(heading);
                    WriteHtml(renderer, node.ChildNodes);
                }
                else
                {
                    block = new HtmlBlockElement(node, renderer.Configuration);
                    renderer.Push(block);
                    WriteHtml(renderer, node.ChildNodes);
                }
                renderer.Pop();
            }
        }
    }
}
