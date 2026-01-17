using Markdig.Syntax.Inlines;
using Microsoft.UI.Xaml.Documents;

namespace Symptum.UI.Markdown.TextElements;

public class AutolinkInlineElement : IAddChild
{
    private AutolinkInline _autoLinkInline;
    private SInline inline;
    private Hyperlink _hyperlink;

    public STextElement TextElement { get; private set; }

    public AutolinkInlineElement(AutolinkInline autoLinkInline)
    {
        _autoLinkInline = autoLinkInline;
        _hyperlink = new Hyperlink()
        {
            NavigateUri = new Uri(autoLinkInline.Url),
        };
        inline = new SInline()
        {
            Inline = _hyperlink
        };
        TextElement = inline;
    }


    public void AddChild(IAddChild child)
    {
        if (child is TextInlineElement text && text.TextElement is SInline inline && inline.Inline is Run run)
            try
            {
                _hyperlink?.Inlines.Add(run);
            }
            catch (Exception ex)
            {
                throw new Exception("Error adding child to AutolinkInlineElement", ex);
            }
    }
}
