using Microsoft.UI.Xaml.Documents;

namespace Symptum.UI.Markdown.TextElements;

public class LineBreakElement : IAddChild
{
    private SInline inline;

    public STextElement TextElement => inline;

    public LineBreakElement()
    {
        inline = new()
        {
            Inline = new LineBreak()
        };
    }

    public void AddChild(IAddChild child) { }
}
