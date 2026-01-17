using Microsoft.UI;
using Microsoft.UI.Xaml.Documents;
using Symptum.Markdown.Reference;

namespace Symptum.UI.Markdown.TextElements;

public class ReferenceInlineElement : IAddChild
{
    private ReferenceInline _referenceInline;
    private SInline _inline;

    public STextElement TextElement => _inline;

    public ReferenceInlineElement(ReferenceInline referenceInline)
    {
        _referenceInline = referenceInline;
        _inline = new SInline()
        {
            Inline = new Run()
            {
                Foreground = new SolidColorBrush(Colors.Red),
                Text = referenceInline.Content.ToString()
            }
        };
    }

    public void AddChild(IAddChild child) { }
}
