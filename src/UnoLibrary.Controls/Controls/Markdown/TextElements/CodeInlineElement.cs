using Markdig.Syntax.Inlines;

namespace Symptum.UI.Markdown.TextElements;

public class CodeInlineElement : IAddChild
{
    private CodeInline _codeInline;
    private SContainer _container = new();
    private MarkdownConfiguration _config;

    public STextElement TextElement => _container;

    public CodeInlineElement(CodeInline codeInline, MarkdownConfiguration config)
    {
        _codeInline = codeInline;
        _config = config;
        Border border = new()
        {
            Style = config.Themes.CodeInlineBorderStyle,
        };
        TextBlock textBlock = new()
        {
            Text = codeInline.Content,
            Style = config.Themes.CodeTextBlockStyle,
        };
        border.Child = textBlock;
        _container.UIElement = border;
    }

    public void AddChild(IAddChild child) { }
}
