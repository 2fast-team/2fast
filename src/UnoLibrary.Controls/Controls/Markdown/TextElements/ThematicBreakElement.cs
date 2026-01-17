using Markdig.Syntax;

namespace Symptum.UI.Markdown.TextElements;

public class ThematicBreakElement : IAddChild
{
    private ThematicBreakBlock _thematicBreakBlock;
    private SContainer _container = new();

    public STextElement TextElement => _container;

    public ThematicBreakElement(ThematicBreakBlock thematicBreakBlock, MarkdownConfiguration config)
    {
        _thematicBreakBlock = thematicBreakBlock;

        Border border = new()
        {
            Style = config.Themes.ThematicBreakBorderStyle
        };
        _container.UIElement = border;
    }

    public void AddChild(IAddChild child) { }
}
