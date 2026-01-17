namespace Symptum.UI.Markdown.TextElements;

public interface IAddChild
{
    STextElement TextElement { get; }

    void AddChild(IAddChild child);
}
