using Microsoft.UI.Xaml.Documents;

namespace Symptum.UI.Markdown.TextElements;

public class STextElement
{ }

public class SInline : STextElement
{
    public Inline Inline { get; set; }
}

public class SBlock : STextElement
{
    public virtual UIElement? UIElement { get; set; }
}

public class SParagraph : SBlock
{
    public List<Inline> Inlines { get; private set; } = [];

    public Style? TextBlockStyle { get; set; }

    public TextAlignment TextAlignment { get; set; } = TextAlignment.Left;

    #region Include UI

    // Indices of the inlines where the UIElements should be inserted.
    public List<int> UIIndices { get; private set; } = [];

    public bool ContainsUI { get; internal set; } = false;

    public List<UIElement> UIElements { get; private set; } = [];

    #endregion

    // This method should be called after all the inlines have been added to the Paragraph and finalized.
    // It decides whether the resultant UI should be a TextBlock or a StackPanel based on the inclusion of inline UI.
    public void CreateUIElement()
    {
        if (!ContainsUI || UIIndices.Count == 0)
        {
            UIElement = CreateTextBlock(Inlines);
        }
        else // Ugly hack to include UIElements in Paragraph
        {
            CommunityToolkit.WinUI.Controls.WrapPanel wrapPanel = new()
            {
                Orientation = Orientation.Horizontal,
                HorizontalSpacing = 4,
                VerticalSpacing = 2
            };

            for (int i = 0; i < UIIndices.Count; i++)
            {
                int j = i > 0 ? UIIndices[i - 1] : 0; // Gets the previous index (0 if no previous index).
                int k = UIIndices[i]; // Gets the index.
                if (j < k)
                {
                    List<Inline> _inlines = Inlines[j..k]; // Get the inlines in the index range.
                    wrapPanel.Children.Add(CreateTextBlock(_inlines));
                }
                wrapPanel.Children.Add(UIElements[i]);

                if (i == UIIndices.Count - 1 && UIIndices[i] < Inlines.Count) // Adding trailing inlines.
                {
                    List<Inline> _inlines = Inlines[k..];
                    wrapPanel.Children.Add(CreateTextBlock(_inlines));
                }
            }

            UIElement = wrapPanel;
        }
    }

    public void AddInline(STextElement element)
    {
        if (element is SInline inlineChild)
        {
            Inlines.Add(inlineChild.Inline);
        }
        else if (element is SBlock blockChild)
        {
            if (blockChild is SParagraph _para)
                _para.CreateUIElement();
            if (blockChild.UIElement is not UIElement uiElement) return;

            UIIndices.Add(Inlines.Count);
            ContainsUI = true;
            UIElements.Add(uiElement);
        }
    }

    private TextBlock CreateTextBlock(List<Inline> inlines)
    {
        TextBlock _textBlock = new()
        {
            Style = TextBlockStyle,
            TextWrapping = TextWrapping.Wrap,
            TextAlignment = TextAlignment
        };

        foreach (Inline _inline in inlines)
            _textBlock.Inlines.Add(_inline);

        return _textBlock;
    }
}

public class SContainer : SBlock
{
}
