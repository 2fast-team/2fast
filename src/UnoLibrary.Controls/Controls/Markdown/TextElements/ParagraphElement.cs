using Markdig.Syntax;

namespace Symptum.UI.Markdown.TextElements;

public class ParagraphElement : IAddChild
{
    private ParagraphBlock _paragraphBlock;
    private SParagraph _paragraph;
    private MarkdownConfiguration _config;

    public STextElement TextElement => _paragraph;

    public ParagraphElement(ParagraphBlock paragraphBlock, MarkdownConfiguration config)
    {
        _paragraphBlock = paragraphBlock;
        _config = config;
        _paragraph = new() { TextBlockStyle = config.Themes.BodyTextBlockStyle };
    }

    public void AddChild(IAddChild child)
    {
        _paragraph.AddInline(child.TextElement);
    }
}
